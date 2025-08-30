using MicroApp.UsersService.Domain.Entities;
using MicroApp.UsersService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.UsersService.Infrastructure.Persistence;

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
                new Role { Id = Guid.NewGuid(), Name = "CFO", Permissions =
                    UserPermissions.ViewPayments | UserPermissions.CreatePayments |
                    UserPermissions.ViewUsers | UserPermissions.ViewCards | UserPermissions.ManageCompanyCards },
                new Role { Id = Guid.NewGuid(), Name = "CTO", Permissions =
                    UserPermissions.ViewPayments | UserPermissions.ViewUsers | UserPermissions.ViewCards }
            );
            await db.SaveChangesAsync();
        }
    }
}
