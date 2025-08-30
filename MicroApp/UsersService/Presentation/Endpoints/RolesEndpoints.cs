using MicroApp.UsersService.Application.DTOs;
using MicroApp.UsersService.Domain.Entities;
using MicroApp.UsersService.Domain.Enums;
using MicroApp.UsersService.Infrastructure.Persistence;
using MicroApp.UsersService.Presentation.Security;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.UsersService.Presentation.Endpoints;

public static class RolesEndpoints
{
    public static void MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /roles
        app.MapGet("/roles", async (UsersDb db) =>
            Results.Ok(await db.Roles
                .Include(r => r.Users)
                .OrderBy(r => r.Name)
                .Select(r => new RoleResponse(r.Id, r.Name, r.Permissions, r.CreatedAt, r.Users.Count))
                .ToListAsync()))
            .RequirePermission(UserPermissions.ViewUsers);

        // POST /roles
        app.MapPost("/roles", async (CreateRoleRequest req, UsersDb db) =>
        {
            var name = req.Name.Trim();
            if (string.IsNullOrWhiteSpace(name)) return Results.BadRequest("Name is required");

            var exists = await db.Roles.AnyAsync(r => r.Name == name);
            if (exists) return Results.Conflict("Role name already exists");

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Permissions = req.Permissions,
                CreatedAt = DateTime.UtcNow
            };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            return Results.Created($"/roles/{role.Id}", new RoleResponse(role.Id, role.Name, role.Permissions, role.CreatedAt, 0));
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // PUT /roles/{id}
        app.MapPut("/roles/{id:guid}", async (Guid id, UpdateRoleRequest req, UsersDb db) =>
        {
            var role = await db.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == id);
            if (role is null) return Results.NotFound();

            if (!string.IsNullOrWhiteSpace(req.Name))
            {
                var newName = req.Name.Trim();
                if (await db.Roles.AnyAsync(r => r.Id != id && r.Name == newName))
                    return Results.Conflict("Role name already exists");
                role.Name = newName;
            }
            if (req.Permissions.HasValue)
            {
                role.Permissions = req.Permissions.Value;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new RoleResponse(role.Id, role.Name, role.Permissions, role.CreatedAt, role.Users.Count));
        }).RequirePermission(UserPermissions.ManageCompanyUsers);

        // DELETE /roles/{id}
        app.MapDelete("/roles/{id:guid}", async (Guid id, UsersDb db) =>
        {
            var role = await db.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == id);
            if (role is null) return Results.NotFound();

            if (role.Users.Any())
            {
                return Results.Conflict("Cannot delete role with assigned users");
            }

            db.Roles.Remove(role);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequirePermission(UserPermissions.ManageCompanyUsers);
    }
}
