using System.Globalization;
using AuthService.Application.DTOs;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
using Common.Security;
using AuthService.Infrastructure.Persistence;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Enums;
using AuthService.Presentation.Security;
using Common.Validation;
using System.Security.Claims;

namespace AuthService.Presentation.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;

        string pepper = builder.Configuration["Security:HashPepper"] ?? throw new("Missing pepper");

        // Simple in-memory verification store for user-related actions
        var store = new VerificationStore();
        // Map of verificationId -> desired role to apply when the user accepts registration
        var desiredRoleByVerification = new Dictionary<Guid, Guid>();

        // Admin create user endpoint
        app.MapPost("/users", async (AdminCreateUserRequest req, UsersDb db, AuthService.Application.IEmailSender emailSender) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.DisplayName))
                return Results.BadRequest("Email and displayName are required");

            var email = req.Email.Trim();
            var name = req.DisplayName.Trim();

            if (await db.Users.AnyAsync(u => u.Email == email))
                return Results.Conflict("Email already exists");

            // Default role is Viewer
            var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "CTO") ?? await db.Roles.FirstAsync();
            Guid effectiveRoleId = defaultRole.Id;

            if (req.DesiredRoleId is Guid rid)
            {
                var role = await db.Roles.FindAsync(rid);
                if (role is null) return Results.BadRequest("Desired role not found");
                // Final assignment occurs on verification accept; for now, keep default
                effectiveRoleId = defaultRole.Id;
            }

            // Generate a temporary random password to satisfy DB constraint; user will set a new one during verification
            var tempPassword = Guid.NewGuid().ToString("N");
            var (pwdHash, salt) = Common.Security.Hashing.HashSecret(tempPassword, null, pepper);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = name,
                RoleId = effectiveRoleId,
                PasswordHash = pwdHash,
                HashSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            // Optional pre-provide IBAN/DOB to hash
            if (!string.IsNullOrWhiteSpace(req.Iban))
            {
                var ibanNorm = Hashing.NormalizeIban(req.Iban);
                var (h, s) = Hashing.HashSecret(ibanNorm, user.HashSalt, pepper);
                user.IbanHash = h; user.HashSalt ??= s;
            }
            if (req.DateOfBirth is DateOnly dob)
            {
                var dobStr = dob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
                user.DobHash = h; user.HashSalt ??= s2;
            }

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Create verification for the new user; only that user can accept
            var v = store.Create(VerificationAction.NewUserCreated, user.Id, Guid.Empty, user.Id);
            if (req.DesiredRoleId is Guid desiredRid)
            {
                desiredRoleByVerification[v.Id] = desiredRid;
            }

            // Send email with verification code/link
            var link = $"{builder.Configuration["Frontend:VerificationUrl"] ?? "https://app.local/verify"}?id={v.Id}&code={v.Code}";
            await emailSender.SendAsync(user.Email, "Verify your account", $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nIf you were invited by admin, you can set your password during verification.");

            return Results.Created($"/users/{user.Id}", new { userId = user.Id, verification = v });
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // Public create user endpoint removed. Users must be created via /internal/users.

        // Create verification for new user acceptance (optionally bind a desiredRoleId)
        app.MapPost("/users/{id:guid}/verifications", async (Guid id, AdminAssignRoleForVerificationRequest req, UsersDb db, AuthService.Application.IEmailSender emailSender) =>
        {
            if (req is not null && req.DesiredRoleId is Guid rid)
            {
                var role = await db.Roles.FindAsync(rid);
                if (role is null) return Results.BadRequest("Desired role not found");
            }
            var v = store.Create(VerificationAction.NewUserCreated, id, Guid.Empty, id);
            if (req is not null && req.DesiredRoleId is Guid desiredRid)
            {
                desiredRoleByVerification[v.Id] = desiredRid;
            }

            // Send verification email
            var user = await db.Users.FindAsync(id);
            if (user is not null)
            {
                var link = $"{builder.Configuration["Frontend:VerificationUrl"] ?? "https://app.local/verify"}?id={v.Id}&code={v.Code}";
                await emailSender.SendAsync(user.Email, "Verify your account", $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nYou can set your password during verification.");
            }

            return Results.Created($"/users/{id}/verifications/{v.Id}", v);
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // Decide on new user acceptance (only the new user can accept/reject)
        app.MapPost("/users/verifications/{vid:guid}/decision", async (HttpContext http, Guid vid, AuthService.Application.DTOs.UsersVerificationDecisionRequest req, UsersDb db) =>
        {
            if (vid != req.VerificationId) return Results.BadRequest("Mismatched verification id");
            var uidStr = http.User.FindFirstValue("sub") ?? http.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (uidStr is null || !Guid.TryParse(uidStr, out var uid)) return Results.Unauthorized();

            var v = store.Get(vid);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();
            if (v.Action != VerificationAction.NewUserCreated) return Results.BadRequest("Invalid action");
            if (v.TargetId != uid) return Results.Forbid();

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = store.Decide(vid, newStatus);

            if (req.Accept)
            {
                var user = await db.Users.FindAsync(uid);
                if (user is not null)
                {
                    // If new password provided, set it now
                    if (!string.IsNullOrWhiteSpace(req.NewPassword))
                    {
                        var (newHash, newSalt) = Common.Security.Hashing.HashSecret(req.NewPassword, null, pepper);
                        user.PasswordHash = newHash;
                        user.HashSalt = newSalt;
                    }

                    // Apply desired role if any
                    if (desiredRoleByVerification.TryGetValue(vid, out var desiredRoleId))
                    {
                        var role = await db.Roles.FindAsync(desiredRoleId);
                        if (role is not null)
                        {
                            user.RoleId = desiredRoleId;
                        }
                        desiredRoleByVerification.Remove(vid);
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            return Results.Ok(v);
        }).RequireAuthorization();

        // Update user
        app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest req, UsersDb db, IValidator<UpdateUserRequest> validator) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            var vr = validator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            if (req.DisplayName is not null)
            {
                user.DisplayName = req.DisplayName.Trim();
            }
            if (req.RoleId is Guid)
            {
                return Results.BadRequest("Changing role via this endpoint is not allowed. Role must be set during registration verification.");
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
        app.MapGet("/users", async (UsersDb db) =>
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
        app.MapPost("/users/{id:guid}/verify", async (Guid id, string iban, DateOnly dob, UsersDb db) =>
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

    private static async Task<UserResponse> ToResponse(User u, UsersDb db)
    {
        var role = await db.Roles.FindAsync(u.RoleId) ?? throw new InvalidOperationException();
        var eff = u.OverridePermissions ?? role.Permissions;
        return new UserResponse(
            u.Id, u.Email, u.DisplayName,
            role.Id, role.Name, eff,
            IbanHash: u.IbanHash, DobHash: u.DobHash, u.CreatedAt);
    }
}

internal sealed class VerificationStore
{
    private readonly Dictionary<Guid, VerificationDto> _items = new();
    private readonly Random _rng = new();

    public VerificationDto Create(VerificationAction action, Guid targetId, Guid createdBy, Guid? assignee)
    {
        var id = Guid.NewGuid();
        var code = _rng.Next(100000, 1000000).ToString();
        var v = new VerificationDto(id, action, targetId, VerificationStatus.Pending, code, createdBy, assignee, DateTime.UtcNow, null);
        _items[id] = v;
        return v;
    }

    public VerificationDto? Get(Guid id) => _items.TryGetValue(id, out var v) ? v : null;

    public VerificationDto Decide(Guid id, VerificationStatus status)
    {
        var v = _items[id];
        var decided = v with { Status = status, DecidedAt = DateTime.UtcNow };
        _items[id] = decided;
        return decided;
    }
}
