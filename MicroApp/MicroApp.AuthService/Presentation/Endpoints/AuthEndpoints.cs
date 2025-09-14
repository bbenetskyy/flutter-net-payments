using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthService.Application.DTOs;
using Common.Security;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = AuthService.Application.DTOs.RegisterRequest;

namespace AuthService.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterRequest dto, IConfiguration cfg, IHttpClientFactory http) =>
        {
            var cli = http.CreateClient("users");
            var payload = new { email = dto.Email.Trim(), displayName = dto.DisplayName.Trim(), password = dto.Password };
            var req = new HttpRequestMessage(HttpMethod.Post, "/internal/users")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.Add("X-Internal-ApiKey", cfg["InternalApiKey"]);
            var res = await cli.SendAsync(req);
            if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
                return Results.Conflict("Email exists");
            if (!res.IsSuccessStatusCode) return Results.StatusCode((int)res.StatusCode);

            var created = await res.Content.ReadFromJsonAsync<CreateUserInternalResponse>();
            if (created is null) return Results.StatusCode(500);

            var token = CreateJwt(created.id, dto.Email.Trim(), cfg);
            var usersBase = (cfg["Services:Users"] ?? string.Empty).TrimEnd('/');
            var location = $"{usersBase}/users/{created.id}";
            return Results.Created(location, new { userId = created.id, token });
        });

        app.MapPost("/auth/login", async (LoginRequest dto, IConfiguration cfg, IHttpClientFactory http) =>
        {
            var cli = http.CreateClient("users");
            var payload = new { email = dto.Email.Trim(), password = dto.Password };
            var req = new HttpRequestMessage(HttpMethod.Post, "/internal/auth/verify")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.Add("X-Internal-ApiKey", cfg["InternalApiKey"]);
            var res = await cli.SendAsync(req);
            if (!res.IsSuccessStatusCode) return Results.Unauthorized();

            var ok = await res.Content.ReadFromJsonAsync<VerifyInternalResponse>();
            if (ok is null) return Results.Unauthorized();

            var token = CreateJwt(ok.id, dto.Email.Trim(), cfg);
            return Results.Ok(new { userId = ok.id, token });
        });
    }

    private static string CreateJwt(Guid userId, string email, IConfiguration cfg)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            //todo add roles from UsersService
            // new Claim(ClaimTypes.Role, "User") 
        };
        var key = new SymmetricSecurityKey(JwtSigning.GetKeyBytes(cfg["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(cfg["Jwt:Issuer"], cfg["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
