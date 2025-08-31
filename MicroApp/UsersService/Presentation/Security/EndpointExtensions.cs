using Common.Security;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Presentation.Security;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, UserPermissions permission)
    {
        return builder.RequireAuthorization().AddEndpointFilter(new PermissionFilter(permission));
    }
}
