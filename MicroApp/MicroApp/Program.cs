using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Core;
using MicroApp;
using MicroApp.Application.DTOs;
using MicroApp.Application.Security;
using MicroApp.Application.Validation;
using MicroApp.Domain.Entities;
using MicroApp.Infrastructure.Persistence;
using MicroApp.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UsersDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<CardsDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Validators
builder.Services.AddSingleton<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddSingleton<IValidator<CreateCardRequest>, CreateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardRequest>, UpdateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<AssignCardRequest>, AssignCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardOperation>, UpdateCardOperationValidator>();
builder.Services.AddSingleton<IValidator<AssignCardOperation>, AssignCardOperationValidator>();

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

builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri(builder.Configuration["Services:Users"]!));

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var usersDb = scope.ServiceProvider.GetRequiredService<UsersDb>();
    var cardsDb = scope.ServiceProvider.GetRequiredService<CardsDb>();

    // Apply EF Core migrations
    await usersDb.Database.MigrateAsync();
    await cardsDb.Database.MigrateAsync();

    await UsersDbSeeder.SeedAsync(usersDb);

    // Create default admin user if not exists
    var defaultAdminEmail = "admin@microapp.local"; // hard-coded email
    var defaultAdminName = "System Admin";          // hard-coded display name
    var adminPassword = builder.Configuration["DefaultAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(adminPassword))
    {
        var exists = await usersDb.Users.AnyAsync(u => u.Email == defaultAdminEmail);
        if (!exists)
        {
            var role = await usersDb.Roles.FirstOrDefaultAsync(r => r.Name == "CEO")
                       ?? await usersDb.Roles.FirstAsync(); // fallback to any role if CEO not found
            var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
            var (hash, salt) = Hashing.HashSecret(adminPassword, null, pepper);
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = defaultAdminEmail,
                DisplayName = defaultAdminName,
                RoleId = role.Id,
                PasswordHash = hash,
                HashSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
            usersDb.Users.Add(admin);
            await usersDb.SaveChangesAsync();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Grouped endpoint mappings
app.MapUsersEndpoints();
app.MapRolesEndpoints();
app.MapCardsEndpoints();

app.MapGet("/me", async (ClaimsPrincipal user, UsersDb db) =>
{
    var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (sub is null) return Results.Unauthorized();
    var uid = Guid.Parse(sub);

    var u = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == uid);
    if (u is null) return Results.NotFound();

    var eff = u.OverridePermissions ?? u.Role.Permissions;
    return Results.Ok(new
    {
        u.Id, u.Email, u.DisplayName,
        Role = new { u.Role.Id, u.Role.Name },
        EffectivePermissions = eff
    });
}).RequireAuthorization();

app.MapPost("/internal/users", async (HttpRequest http, UsersDb db) =>
{
    if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"]) 
        return Results.Unauthorized();

    var dto = await http.ReadFromJsonAsync<InternalCreateUserDto>();
    if (dto is null) return Results.BadRequest();

    // If user already exists by email, return its id (idempotency by email)
    var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == dto.email.Trim());
    if (existing is not null)
        return Results.Conflict("Email exists");

    var role = await db.Roles.FirstAsync(r => r.Name == "Viewer"); // default role
    var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
    var (pwdHash, salt) = Hashing.HashSecret(dto.password, null, pepper);

    var user = new User 
    { 
        Id = Guid.NewGuid(), 
        Email = dto.email.Trim(), 
        DisplayName = dto.displayName.Trim(), 
        RoleId = role.Id,
        PasswordHash = pwdHash,
        HashSalt = salt
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", new { id = user.Id });
}).ExcludeFromDescription(); // hide from Swagger

// Internal auth verify
app.MapPost("/internal/auth/verify", async (HttpRequest http, UsersDb db) =>
{
    if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"]) 
        return Results.Unauthorized();

    var dto = await http.ReadFromJsonAsync<InternalVerifyDto>();
    if (dto is null) return Results.BadRequest();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.email.Trim());
    if (user is null) return Results.Unauthorized();

    var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
    var (pwdHash2, _) = Hashing.HashSecret(dto.password, user.HashSalt, pepper);
    var ok = user.PasswordHash == pwdHash2;
    if (!ok) return Results.Unauthorized();

    return Results.Ok(new { id = user.Id });
}).ExcludeFromDescription();





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

namespace MicroApp
{
    record RegisterDto(string Email, string Password, string DisplayName);
    record LoginDto(string Email, string Password);

    // Internal DTOs for this unified API
    record InternalCreateUserDto(string email, string displayName, string password);
    record InternalVerifyDto(string email, string password);

    record CreateUserInternalResponse(Guid id);
    record VerifyInternalResponse(Guid id);
}