using System.Net.Http.Headers;
using System.Text.Json;
using Common.Security;

namespace CardsService.Presentation.Security;

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

        // Ensure authenticated
        var user = http.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        // Read bearer token to forward to UsersService
        var authHeader = http.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Results.Unauthorized();

        var factory = http.RequestServices.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("users");

        // forward the same bearer token
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

        using var res = await client.GetAsync("/me");
        if (!res.IsSuccessStatusCode)
        {
            // Unauthorized or Forbidden from UsersService - propagate
            if ((int)res.StatusCode == StatusCodes.Status401Unauthorized) return Results.Unauthorized();
            if ((int)res.StatusCode == StatusCodes.Status403Forbidden) return Results.Forbid();
            return Results.StatusCode((int)res.StatusCode);
        }

        var contentStream = await res.Content.ReadAsStreamAsync();
        var me = await JsonSerializer.DeserializeAsync<MeResponse>(contentStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (me is null) return Results.Unauthorized();

        var has = ((UserPermissions)me.EffectivePermissions & _required) == _required;
        if (!has) return Results.Forbid();

        return await next(context);
    }

    private sealed class MeResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public long EffectivePermissions { get; set; }
    }
}
