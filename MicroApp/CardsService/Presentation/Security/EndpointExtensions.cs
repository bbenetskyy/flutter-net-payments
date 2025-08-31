using Common.Security;
using Microsoft.AspNetCore.Builder;

namespace CardsService.Presentation.Security;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, UserPermissions permission)
    {
        return builder.RequireAuthorization().AddEndpointFilter(new PermissionFilter(permission));
    }
}
