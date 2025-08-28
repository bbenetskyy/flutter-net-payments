using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Presentation.Security;

public sealed class PermissionFilter : IEndpointFilter
{
    private readonly UserPermissions _required;

    public PermissionFilter(UserPermissions required)
    {
        _required = required;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var user = http.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        // Extract user id (sub)
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var userId))
        {
            return Results.Unauthorized();
        }

        // Resolve DB and compute effective permissions
        var db = http.RequestServices.GetRequiredService<UsersDb>();
        var u = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId);
        if (u is null)
        {
            return Results.Forbid();
        }

        var eff = u.OverridePermissions ?? u.Role.Permissions;
        var has = (eff & _required) == _required;
        if (!has)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
