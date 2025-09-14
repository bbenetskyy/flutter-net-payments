using Common.Validation;
using MicroApp.UsersService.Application;
using MicroApp.UsersService.Application.DTOs;
using MicroApp.UsersService.Domain.Entities;
using MicroApp.UsersService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.UsersService.Presentation.Endpoints;

public static class InternalUserEndpoints
{
    public static void MapInternalUserEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;

        app.MapPost("/internal/users", async (HttpRequest http, UsersDb db, IEmailSender emailSender, IVerificationStore store,
            IHttpClientFactory httpFactory) =>
        {
            if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"])
                return Results.Unauthorized();

            var dto = await http.ReadFromJsonAsync<InternalCreateUserDto>();
            if (dto is null) return Results.BadRequest();

            // If user already exists by email, return its id (idempotency by email)
            var existing = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == dto.email.Trim());
            if (existing is not null)
                return Results.Conflict("Email exists");

            var role = await db.Roles.FirstAsync(r => r.Name == "CTO"); // default role
            var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
            var (pwdHash, salt) = Common.Security.Hashing.HashSecret(dto.password, null, pepper);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.email.Trim(),
                DisplayName = dto.displayName.Trim(),
                RoleId = role.Id,
                PasswordHash = pwdHash,
                HashSalt = salt,
                VerificationStatus = VerificationStatus.Pending // Set verification status to Pending
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Best-effort wallet sync (do not fail user creation if this fails)
            try
            {
                var client = httpFactory.CreateClient("wallet");
                client.DefaultRequestHeaders.Add("X-Internal-ApiKey", builder.Configuration["InternalApiKey"]);
                using var _ = await client.PostAsync("/internal/wallets/sync-all-users", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wallet sync failed: {ex.Message}");
            }

            // Determine creator from internal header X-Acting-UserId if present; fallback to system (Guid.Empty)
            Guid createdBy = Guid.Empty;
            if (http.Headers.TryGetValue("X-Acting-UserId", out var acting) && Guid.TryParse(acting.ToString(), out var parsed))
                createdBy = parsed;

            var v = await store.Create(VerificationAction.NewUserCreated, user.Id, createdBy, user.Id);
            var link =
                $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}?id={user.Id}&code={v.Code}";
            await emailSender.SendAsync(user.Email, "Verify your account",
                $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nYou can set your password during verification.");
            return Results.Created($"/users/{user.Id}", new { id = user.Id });
        }).ExcludeFromDescription();

// Internal auth verify
        app.MapGet("/internal/users/ids", async (HttpRequest http, UsersDb db) =>
        {
            if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"])
                return Results.Unauthorized();
            var ids = await db.Users.AsNoTracking().Select(u => new { u.Id }).ToListAsync();
            return Results.Ok(ids);
        }).ExcludeFromDescription();

        app.MapPost("/internal/auth/verify", async (HttpRequest http, UsersDb db) =>
        {
            if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"])
                return Results.Unauthorized();

            var dto = await http.ReadFromJsonAsync<InternalVerifyDto>();
            if (dto is null) return Results.BadRequest();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.email.Trim());
            if (user is null || user.IsDeleted || user.VerificationStatus != VerificationStatus.Completed)
                return Results.Unauthorized();

            var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
            var (pwdHash2, _) = Common.Security.Hashing.HashSecret(dto.password, user.HashSalt, pepper);
            var ok = user.PasswordHash == pwdHash2;
            if (!ok) return Results.Unauthorized();

            return Results.Ok(new { id = user.Id });
        }).ExcludeFromDescription();
    }
}
