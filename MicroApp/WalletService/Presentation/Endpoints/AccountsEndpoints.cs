using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Common.Domain.Entities;
using Common.Validation;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Presentation.Endpoints;

public static class AccountsEndpoints
{
    public static void MapAccountsEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;

        // List current user's accounts (multi-IBAN)
        app.MapGet("/accounts/my", async (ClaimsPrincipal user, WalletDb db) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var items = await db.Accounts.AsNoTracking()
                .Where(a => a.UserId == uid)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            return Results.Ok(items.Select(a => new AccountDto(a.Id, a.UserId, a.Iban, a.Currency, a.CreatedAt)));
        }).RequireAuthorization();

        // Create a new account (IBAN + currency) for current user
        app.MapPost("/accounts", async (ClaimsPrincipal user, CreateAccountRequest req, WalletDb db) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var rawIban = (req.Iban ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawIban)) return Results.BadRequest("IBAN is required");
            var iban = Common.Security.Hashing.NormalizeIban(rawIban);
            if (iban.Length > 64) return Results.BadRequest("IBAN too long");
            if (!Validation.IsValidIban(iban)) return Results.BadRequest("Invalid IBAN format");

            // Ensure global uniqueness
            var exists = await db.Accounts.AsNoTracking().AnyAsync(a => a.Iban == iban);
            if (exists) return Results.Conflict("This IBAN is already linked to another user");

            var acc = new Account
            {
                Id = Guid.NewGuid(),
                UserId = uid,
                Iban = iban,
                Currency = req.Currency,
                CreatedAt = DateTime.UtcNow
            };

            db.Accounts.Add(acc);
            await db.SaveChangesAsync();
            return Results.Created($"/accounts/{acc.Id}", new AccountDto(acc.Id, acc.UserId, acc.Iban, acc.Currency, acc.CreatedAt));
        }).RequireAuthorization();

        // Delete an account of current user
        app.MapDelete("/accounts/{id:guid}", async (ClaimsPrincipal user, Guid id, WalletDb db) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var acc = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
            if (acc is null) return Results.NotFound();
            if (acc.UserId != uid) return Results.Forbid();

            db.Accounts.Remove(acc);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Internal: list accounts for userId (secured by internal API key)
        app.MapGet("/accounts/{userId:guid}", async (HttpRequest http, Guid userId, WalletDb db) =>
        {
            var items = await db.Accounts.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
            return Results.Ok(items.Select(a => new AccountDto(a.Id, a.UserId, a.Iban, a.Currency, a.CreatedAt)));
        }).RequireAuthorization();
    }
}

public record CreateAccountRequest(string Iban, Common.Domain.Enums.Currency Currency);
public record AccountDto(Guid Id, Guid UserId, string Iban, Common.Domain.Enums.Currency Currency, DateTime CreatedAt);
