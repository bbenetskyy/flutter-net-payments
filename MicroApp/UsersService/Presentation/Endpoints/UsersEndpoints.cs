using System.Globalization;
using System.Security.Claims;
using Common.Domain.Entities;
using Common.Infrastucture.Persistence;
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
        app.MapPost("/users", async (ClaimsPrincipal currentUser, AdminCreateUserRequest req, UsersDb db, IEmailSender emailSender, IVerificationStore store, IHttpClientFactory httpFactory, IConfiguration cfg) =>
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

                // Best-effort wallet sync (do not fail user creation if this fails)
                try
                {
                    var client = httpFactory.CreateClient("wallet");
                    client.DefaultRequestHeaders.Add("X-Internal-ApiKey", cfg["InternalApiKey"]);
                    using var _ = await client.PostAsync("/internal/wallets/sync-all-users", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wallet sync failed for user {user.Id}: {ex.Message}");
                }

                // Create verification for the new user; createdBy = current admin, assignee = the new user
                var sub = currentUser.FindFirstValue("sub") ?? currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var createdBy))
                    return Results.Unauthorized();
                var v = await store.Create(VerificationAction.NewUserCreated, user.Id, createdBy, user.Id);
                if (req.DesiredRoleId is Guid desiredRid)
                {
                    desiredRoleByVerification[v.TargetId] = desiredRid;
                }

                // Send email with verification code/link
                var link =
                    $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}{v.Id}/verify?code={v.Code}";
                await emailSender.SendAsync(user.Email, "Verify your account",
                    $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nIf you were invited by admin, you can set your password during verification.");
                return Results.Created($"/users/{user.Id}", new { userId = user.Id, verification = v });
            })
            .RequirePermission(UserPermissions.ManageCompanyUsers);

        // Public create user endpoint removed. Users must be created via /internal/users.

        // Create verification for new user acceptance (optionally bind a desiredRoleId)
        app.MapPost("/users/{id:guid}/verifications", async (ClaimsPrincipal currentUser, Guid id, AdminAssignRoleForVerificationRequest req, UsersDb db,
            IEmailSender emailSender, IVerificationStore store) =>
        {
            if (req is not null && req.DesiredRoleId is Guid rid)
            {
                var role = await db.Roles.FindAsync(rid);
                if (role is null) return Results.BadRequest("Desired role not found");
            }
            var sub2 = currentUser.FindFirstValue("sub") ?? currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub2) || !Guid.TryParse(sub2, out var createdBy2))
                return Results.Unauthorized();
            var v = await store.Create(VerificationAction.NewUserCreated, id, createdBy2, id);
            if (req is not null && req.DesiredRoleId is Guid desiredRid)
            {
                desiredRoleByVerification[v.TargetId] = desiredRid;
            }

            // Send verification email
            var user = await db.Users.FindAsync(id);
            if (user is not null)
            {
                var link =
                    $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}{v.Id}/verify?code={v.Code}";
                await emailSender.SendAsync(user.Email, "Verify your account",
                    $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nYou can set your password during verification.");
            }

            return Results.Created($"/users/{id}/verifications/{v.Id}", v);
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        app.MapGet("/users/verifications", async (
                VerificationStatus? status,
                Guid? targetId,
                Guid? assigneeId,
                Guid? createdBy,
                string? q, // <-- free-text search
                int? skip,
                int? take,
                VerificationsDb db,
                CancellationToken ct) =>
            {
                var s = Math.Max(0, skip ?? 0);
                var t = take is int x ? Math.Clamp(x, 1, 500) : 25; // sane defaults + cap

                // base query
                IQueryable<Verification> query = db.Verifications.AsNoTracking()
                    .Where(v=> v.Action == VerificationAction.NewUserCreated);

                // filters
                if (status.HasValue) query = query.Where(v => v.Status == status.Value);
                if (targetId.HasValue) query = query.Where(v => v.TargetId == targetId.Value);
                if (assigneeId.HasValue) query = query.Where(v => v.AssigneeUserId == assigneeId.Value);
                if (createdBy.HasValue) query = query.Where(v => v.CreatedBy == createdBy.Value);

                // search:
                // - if q parses as Guid -> match Id/TargetId/Assignee/CreatedBy
                // - else -> LIKE search on Code (and optionally Action/Status names)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();

                    if (Guid.TryParse(term, out var g))
                    {
                        query = query.Where(v =>
                            v.Id == g ||
                            v.TargetId == g ||
                            v.AssigneeUserId == g ||
                            v.CreatedBy == g);
                    }
                    else
                    {
                        var like = $"%{term}%";
                        // Prefer provider-specific case-insensitive functions if available.
                        // This version uses LIKE on Code and also allows searching by enum names.
                        query = query.Where(v =>
                            (EF.Functions.Like(v.Code, like)) ||
                            EF.Functions.Like(v.Action.ToString(), like) ||
                            EF.Functions.Like(v.Status.ToString(), like));
                    }
                }

                // total BEFORE paging
                var total = await query.CountAsync(ct);

                // order newest first, then page
                var items = await query
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip(s)
                    .Take(t)
                    .Select(v => new
                    {
                        v.Id,
                        v.Action,
                        v.TargetId,
                        v.Status,
                        v.Code,
                        v.CreatedBy,
                        v.AssigneeUserId,
                        v.CreatedAt,
                        v.DecidedAt
                    })
                    .ToListAsync(ct);

                return Results.Ok(new
                {
                    total,
                    skip = s,
                    take = t,
                    items
                });
            })
            .RequirePermission(UserPermissions.ManageCompanyUsers);


        // Decide on new user acceptance (only the new user can accept/reject)
        app.MapPost("/users/verifications/{vid:guid}/decision", async (HttpContext http, Guid vid,
            UsersVerificationDecisionRequest req, UsersDb db, IVerificationStore store) =>
        {
            var user = await db.Users.FindAsync(req.TargetId);
            if (user is null)
                return Results.BadRequest("Target user not found");

            var v = await store.Get(vid);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();
            if (v.Action != VerificationAction.NewUserCreated) return Results.BadRequest("Invalid action");

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = await store.Decide(vid, newStatus);

            if (req.Accept)
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
            }

            user.VerificationStatus = newStatus;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(v);
        }).AllowAnonymous();

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

        app.MapDelete("/users/{id:guid}", async (HttpContext http, Guid id, UsersDb db) =>
            {
                // 1) Prevent self-delete
                var meStr = http.User.FindFirstValue("sub") ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(meStr, out var meId) && meId == id)
                    return Results.BadRequest("You cannot delete your own account.");

                // 2) Load target user
                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user is null) return Results.NotFound();

                // 3) Ensure we won't delete the last user with ManageCompanyUsers
                var remainingManagers = await db.Users
                    .Include(u => u.Role)
                    .Where(u => u.Id != id)
                    .Where(u =>
                        ((long)(u.OverridePermissions ?? u.Role.Permissions) &
                         (long)UserPermissions.ManageCompanyUsers) != 0)
                    .CountAsync();

                if (remainingManagers == 0)
                    return Results.BadRequest("Cannot delete the last user with ManageCompanyUsers permission.");

                // 4) Hard delete
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .RequirePermission(UserPermissions.ManageCompanyUsers);

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
                    DobHash = u.DobHash,
                    u.VerificationStatus,
                    u.CreatedAt
                }).ToListAsync())
        ).RequirePermission(UserPermissions.ViewUsers);


        // Verify IBAN/DOB
        // app.MapPost("/users/{id:guid}/verify", async (Guid id, string code, UsersDb db, IVerificationStore store) =>
        // {
        //     var user = await db.Users.FindAsync(id);
        //     if (user is null) return Results.NotFound();
        //
        //     var verificationDetails = await store.Get(id);
        //     var ok = verificationDetails?.Code == code &&
        //                 verificationDetails.Status == VerificationStatus.Pending;
        //     if (!ok)
        //     {
        //         Results.NotFound();
        //     }
        //     store.Decide(id, VerificationStatus.Completed);
        //     user.VerificationStatus = VerificationStatus.Completed;
        //     user.UpdatedAt = DateTime.UtcNow;
        //     await db.SaveChangesAsync();
        //     return Results.Ok();
        // });
    }

    private static async Task<UserResponse> ToResponse(User u, UsersDb db)
    {
        var role = await db.Roles.FindAsync(u.RoleId) ?? throw new InvalidOperationException();
        var eff = u.OverridePermissions ?? role.Permissions;
        return new UserResponse(
            u.Id, u.Email, u.DisplayName,
            role.Id, role.Name, eff,
            u.VerificationStatus,
            DobHash: u.DobHash, u.CreatedAt);
    }
}