using System.Net.Http.Headers;
using System.Text.Json;
using Common.Security;

namespace WalletService.Presentation.Security;

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
            return Results.Unauthorized();

        var authHeader = http.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Results.Unauthorized();

        var factory = http.RequestServices.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("users");
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

        using var res = await client.GetAsync("/me");
        if (!res.IsSuccessStatusCode)
        {
            if ((int)res.StatusCode == StatusCodes.Status401Unauthorized) return Results.Unauthorized();
            if ((int)res.StatusCode == StatusCodes.Status403Forbidden) return Results.Forbid();
            return Results.StatusCode((int)res.StatusCode);
        }

        var me = await res.Content.ReadFromJsonAsync<MeResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (me is null) return Results.Unauthorized();

        var has = ((UserPermissions)me.EffectivePermissions & _required) == _required;
        if (!has) return Results.Forbid();

        return await next(context);
    }

    private sealed class MeResponse
    {
        public Guid Id { get; set; }
        public long EffectivePermissions { get; set; }
    }
}
