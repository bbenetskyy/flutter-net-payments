using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.DTOs;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Presentation.Endpoints;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Common.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<UsersDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddSingleton<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();

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

builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri(builder.Configuration["Services:Users"]!));

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDb>();
    await db.Database.EnsureCreatedAsync();

    // Apply EF Core migrations (replaces previous ad-hoc SQL hotfix)
    await db.Database.MigrateAsync();

    await UsersDbSeeder.SeedAsync(db);

    // Create default admin user if not exists
    var defaultAdminEmail = "admin@microapp.local"; // hard-coded email
    var defaultAdminName = "System Admin"; // hard-coded display name
    var adminPassword = builder.Configuration["DefaultAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(adminPassword))
    {
        var exists = await db.Users.AnyAsync(u => u.Email == defaultAdminEmail);
        if (!exists)
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "CEO")
                       ?? await db.Roles.FirstAsync(); // fallback to any role if CEO not found
            var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
            var (hash, salt) = Common.Security.Hashing.HashSecret(adminPassword, null, pepper);
            var admin = new AuthService.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = defaultAdminEmail,
                DisplayName = defaultAdminName,
                RoleId = role.Id,
                PasswordHash = hash,
                HashSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();
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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

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
    var (pwdHash, salt) = Common.Security.Hashing.HashSecret(dto.password, null, pepper);

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
    var (pwdHash2, _) = Common.Security.Hashing.HashSecret(dto.password, user.HashSalt, pepper);
    var ok = user.PasswordHash == pwdHash2;
    if (!ok) return Results.Unauthorized();

    return Results.Ok(new { id = user.Id });
}).ExcludeFromDescription();


app.Run();


record InternalCreateUserDto(string email, string displayName, string password);
record InternalVerifyDto(string email, string password);

