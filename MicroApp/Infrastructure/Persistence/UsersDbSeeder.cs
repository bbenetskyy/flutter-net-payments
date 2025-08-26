using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public static class UsersDbSeeder
{
    public static async Task SeedAsync(UsersDb db)
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Id = Guid.NewGuid(), Name = "CEO", Permissions =
                    UserPermissions.ViewPayments | UserPermissions.CreatePayments | UserPermissions.ConfirmPayments |
                    UserPermissions.ViewUsers | UserPermissions.ManageCompanyUsers |
                    UserPermissions.EditCompanyDetails | UserPermissions.ViewCards | UserPermissions.ManageCompanyCards },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Permissions =
                    UserPermissions.ViewPayments | UserPermissions.CreatePayments |
                    UserPermissions.ViewUsers | UserPermissions.ViewCards | UserPermissions.ManageCompanyCards },
                new Role { Id = Guid.NewGuid(), Name = "Viewer", Permissions =
                    UserPermissions.ViewPayments | UserPermissions.ViewUsers | UserPermissions.ViewCards }
            );
            await db.SaveChangesAsync();
        }
    }
}
