using System.Globalization;
using MicroApp.Application.DTOs;
using MicroApp.Application.Security;
using MicroApp.Application.Validation;
using MicroApp.Domain.Entities;
using MicroApp.Domain.Enums;
using MicroApp.Infrastructure.Persistence;
using MicroApp.Presentation.Security;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.Presentation.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;

        string pepper = builder.Configuration["Security:HashPepper"] ?? throw new("Missing pepper");

        // Public create user endpoint removed. Users must be created via /internal/users.

        // Update user
        app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest req, MicroAppDb db, IValidator<UpdateUserRequest> validator) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            var vr = validator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            if (req.DisplayName is not null)
            {
                user.DisplayName = req.DisplayName.Trim();
            }
            if (req.RoleId is Guid rid)
            {
                if (await db.Roles.FindAsync(rid) is null) return Results.BadRequest("Role not found");
                user.RoleId = rid;
            }
            if (req.OverridePermissions is not null) user.OverridePermissions = req.OverridePermissions;

            if (req.Iban is not null)
            {
                var ibanNorm = Hashing.NormalizeIban(req.Iban);
                var (h, s) = Hashing.HashSecret(ibanNorm, user.HashSalt, pepper);
                user.IbanHash = h; user.HashSalt ??= s;
            }
            if (req.DateOfBirth is not null)
            {
                var dobStr = req.DateOfBirth.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
                user.DobHash = h; user.HashSalt ??= s2;
            }
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(await ToResponse(user, db));
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // List users
        app.MapGet("/users", async (MicroAppDb db) =>
            Results.Ok(await db.Users
                .Include(u => u.Role)
                .OrderBy(x => x.DisplayName)
                .Select(u => new {
                    u.Id, u.Email, u.DisplayName,
                    Role = u.Role.Name,
                    EffectivePermissions = (long)(u.OverridePermissions ?? u.Role.Permissions),
                    IbanHash = u.IbanHash, DobHash = u.DobHash,
                    u.CreatedAt
                }).ToListAsync())
        ).RequirePermission(UserPermissions.ViewUsers);

        // Verify IBAN/DOB
        app.MapPost("/users/{id:guid}/verify", async (Guid id, string iban, DateOnly dob, MicroAppDb db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            var ibanNorm = Hashing.NormalizeIban(iban);
            var (ibanHash, _) = Hashing.HashSecret(ibanNorm, user.HashSalt, pepper);
            var dobStr = dob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var (dobHash, _) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);

            var ok = user.IbanHash == ibanHash && user.DobHash == dobHash;
            return ok ? Results.Ok() : Results.Unauthorized();
        }).RequirePermission(UserPermissions.ViewUsers);
    }

    private static async Task<UserResponse> ToResponse(User u, MicroAppDb db)
    {
        var role = await db.Roles.FindAsync(u.RoleId) ?? throw new InvalidOperationException();
        var eff = u.OverridePermissions ?? role.Permissions;
        return new UserResponse(
            u.Id, u.Email, u.DisplayName,
            role.Id, role.Name, eff,
            IbanHash: u.IbanHash, DobHash: u.DobHash, u.CreatedAt);
    }
}
