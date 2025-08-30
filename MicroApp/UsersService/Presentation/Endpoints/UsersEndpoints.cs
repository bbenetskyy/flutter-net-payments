using System.Globalization;
using System.Security.Claims;
using Common.Security;
using Common.Validation;
using MicroApp.UsersService.Application;
using MicroApp.UsersService.Application.DTOs;
using MicroApp.UsersService.Domain.Entities;
using MicroApp.UsersService.Domain.Enums;
using MicroApp.UsersService.Infrastructure.Persistence;
using MicroApp.UsersService.Presentation.Security;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.UsersService.Presentation.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;

        string pepper = builder.Configuration["Security:HashPepper"] ?? throw new("Missing pepper");

        // Map of verificationId -> desired role to apply when the user accepts registration
        //todo this may not be used and should removed later
        var desiredRoleByVerification = new Dictionary<Guid, Guid>();

        // Admin create user endpoint
        app.MapPost("/users", async (AdminCreateUserRequest req, UsersDb db, IEmailSender emailSender, IVerificationStore store) =>
            {
                // Validation
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.DisplayName))
                    return Results.BadRequest("Email and DisplayName are required");

                // Check if user already exists
                var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.Trim());
                if (existing is not null) return Results.Conflict("User with this email already exists");

                // Get role or use default
                Role? role;
                if (req.DesiredRoleId.HasValue)
                {
                    role = await db.Roles.FindAsync(req.DesiredRoleId.Value);
                    if (role is null) return Results.BadRequest("Desired role not found");
                }
                else
                {
                    role = await db.Roles.FirstAsync(r => r.Name == "CTO"); // default role
                }

                // Create user with pending verification status
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = req.Email.Trim(),
                    DisplayName = req.DisplayName.Trim(),
                    RoleId = role.Id,
                    PasswordHash = string.Empty, // Will be set during verification
                    HashSalt = Guid.NewGuid().ToString(),
                    VerificationStatus = VerificationStatus.Pending // Set verification status to Pending
                };

                // Hash IBAN if provided
                if (!string.IsNullOrWhiteSpace(req.Iban))
                {
                    var (h, s2) = Hashing.HashSecret(req.Iban, user.HashSalt, pepper);
                    user.Iban = h;
                    user.HashSalt ??= s2;
                }

                // Hash date of birth if provided
                if (req.DateOfBirth is DateOnly dob)
                {
                    var dobStr = dob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
                    user.DobHash = h;
                    user.HashSalt ??= s2;
                }

                db.Users.Add(user);
                await db.SaveChangesAsync();

                // Create verification for the new user; only that user can accept
                var v = await store.Create(VerificationAction.NewUserCreated, user.Id, Guid.Empty, user.Id);
                if (req.DesiredRoleId is Guid desiredRid)
                {
                    desiredRoleByVerification[v.TargetId] = desiredRid;
                }

                // Send email with verification code/link
                var link =
                    $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}{v.TargetId}/verify?code={v.Code}";
                await emailSender.SendAsync(user.Email, "Verify your account",
                    $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nIf you were invited by admin, you can set your password during verification.");
                return Results.Created($"/users/{user.Id}", new { userId = user.Id, verification = v });
            })
            .RequirePermission(UserPermissions.ManageCompanyUsers);

        // Public create user endpoint removed. Users must be created via /internal/users.

        // Create verification for new user acceptance (optionally bind a desiredRoleId)
        app.MapPost("/users/{id:guid}/verifications", async (Guid id, AdminAssignRoleForVerificationRequest req, UsersDb db,
            IEmailSender emailSender, IVerificationStore store) =>
        {
            if (req is not null && req.DesiredRoleId is Guid rid)
            {
                var role = await db.Roles.FindAsync(rid);
                if (role is null) return Results.BadRequest("Desired role not found");
            }
            var v = await store.Create(VerificationAction.NewUserCreated, id, Guid.Empty, id);
            if (req is not null && req.DesiredRoleId is Guid desiredRid)
            {
                desiredRoleByVerification[v.TargetId] = desiredRid;
            }

            // Send verification email
            var user = await db.Users.FindAsync(id);
            if (user is not null)
            {
                var link =
                    $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}{v.TargetId}/verify?code={v.Code}";
                await emailSender.SendAsync(user.Email, "Verify your account",
                    $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nYou can set your password during verification.");
            }

            return Results.Created($"/users/{id}/verifications/{v.TargetId}", v);
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // Decide on new user acceptance (only the new user can accept/reject)
        app.MapPost("/users/verifications/{vid:guid}/decision", async (HttpContext http, Guid vid,
            UsersVerificationDecisionRequest req, UsersDb db, IVerificationStore store) =>
        {
            if (vid != req.VerificationId) return Results.BadRequest("Mismatched verification id");
            var uidStr = http.User.FindFirstValue("sub") ??
                         http.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (uidStr is null || !Guid.TryParse(uidStr, out var uid)) return Results.Unauthorized();

            var v =await store.Get(vid);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();
            if (v.Action != VerificationAction.NewUserCreated) return Results.BadRequest("Invalid action");
            if (v.TargetId != uid) return Results.Forbid();

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = await store.Decide(vid, newStatus);

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
                return Results.BadRequest(
                    "Changing role via this endpoint is not allowed. Role must be set during registration verification.");
            }
            if (req.OverridePermissions is not null) user.OverridePermissions = req.OverridePermissions;

            if (req.Iban is not null)
            {
                // Store plain IBAN (normalized)
                user.Iban = Hashing.NormalizeIban(req.Iban);
            }
            if (req.DateOfBirth is not null)
            {
                var dobStr = req.DateOfBirth.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
                user.DobHash = h;
                user.HashSalt ??= s2;
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
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.DisplayName,
                    Role = u.Role.Name,
                    EffectivePermissions = (long)(u.OverridePermissions ?? u.Role.Permissions),
                    Iban = u.Iban,
                    DobHash = u.DobHash,
                    u.VerificationStatus,
                    u.CreatedAt
                }).ToListAsync())
        ).RequirePermission(UserPermissions.ViewUsers);

        // Verify IBAN/DOB
        app.MapPost("/users/{id:guid}/verify", async (Guid id, string code, UsersDb db, IVerificationStore store) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            var verificationDetails = await store.Get(id);
            var ok = verificationDetails?.Code == code &&
                        verificationDetails.Status == VerificationStatus.Pending;
            if (!ok)
            {
                Results.NotFound();
            }
            store.Decide(id, VerificationStatus.Completed);
            user.VerificationStatus = VerificationStatus.Completed;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }

    private static async Task<UserResponse> ToResponse(User u, UsersDb db)
    {
        var role = await db.Roles.FindAsync(u.RoleId) ?? throw new InvalidOperationException();
        var eff = u.OverridePermissions ?? role.Permissions;
        return new UserResponse(
            u.Id, u.Email, u.DisplayName,
            role.Id, role.Name, eff,
            u.VerificationStatus,
            Iban: u.Iban, DobHash: u.DobHash, u.CreatedAt);
    }
}

public interface IVerificationStore
{
    Task<VerificationDto> Create(VerificationAction action, Guid targetId, Guid createdBy, Guid? assignee);
    Task<VerificationDto?> Get(Guid id);
    Task<VerificationDto?> Decide(Guid id, VerificationStatus status);
}

internal sealed class VerificationStore : IVerificationStore
{
    private readonly UsersDb _db;
    private readonly Random _rng = new();

    public VerificationStore(UsersDb db)
    {
        _db = db;
    }

    public async Task<VerificationDto> Create(VerificationAction action, Guid targetId, Guid createdBy, Guid? assignee)
    {
        var code = _rng.Next(100000, 1000000).ToString();
        var verification = new Verification
        {
            Id = Guid.NewGuid(),
            Action = action,
            TargetId = targetId,
            Status = VerificationStatus.Pending,
            Code = code,
            CreatedBy = createdBy,
            AssigneeUserId = assignee,
            CreatedAt = DateTime.UtcNow
        };
        _db.Verifications.Add(verification);
        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationDto?> Get(Guid id)
    {
        var verification = await _db.Verifications.FindAsync(id);
        return verification is null ? null : ToDto(verification);
    }

    public async Task<VerificationDto?> Decide(Guid id, VerificationStatus status)
    {
        var verification = await _db.Verifications.FindAsync(id);
        if (verification is null) return null;

        verification.Status = status;
        verification.DecidedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(verification);
    }

    private static VerificationDto ToDto(Verification v) => new(v.Id, v.Action, v.TargetId, v.Status, v.Code, v.CreatedBy, v.AssigneeUserId, v.CreatedAt, v.DecidedAt);
}