using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthService;
using Common.Security;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Minimal setup for AuthService placeholder (roles/users moved to UsersService)
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
{
    var cfg = builder.Configuration;
    o.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Jwt:Issuer"],
        ValidAudience = cfg["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(JwtSigning.GetKeyBytes(cfg["Jwt:Key"])), 
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();


// Register HTTP client factory and a named client for UsersService
builder.Services.AddHttpClient("users", c =>
{
    var baseAddress = builder.Configuration["Services:Users"]!;
    c.BaseAddress = new Uri(baseAddress);
});

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/auth/register", async (RegisterDto dto, IConfiguration cfg, IHttpClientFactory http) =>
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

app.MapPost("/auth/login", async (LoginDto dto, IConfiguration cfg, IHttpClientFactory http) =>
{
    var cli = http.CreateClient("users");
    var payload = new { email = dto.Email.Trim(), password = dto.Password };
    var req = new HttpRequestMessage(HttpMethod.Post, "/i ")
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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();


static string CreateJwt(Guid userId, string email, IConfiguration cfg)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim(ClaimTypes.Role, "User") // або роль з UsersService/пізніше
    };
    var key = new SymmetricSecurityKey(JwtSigning.GetKeyBytes(cfg["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(cfg["Jwt:Issuer"], cfg["Jwt:Audience"], claims,
        expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

namespace AuthService
{
    record RegisterDto(string Email, string Password, string DisplayName);
    record LoginDto(string Email, string Password);

    record CreateUserInternalResponse(Guid id);
    record VerifyInternalResponse(Guid id);
}