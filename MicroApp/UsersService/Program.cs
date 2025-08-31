using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using MicroApp.UsersService;
using MicroApp.UsersService.Application;
using MicroApp.UsersService.Application.DTOs;
using MicroApp.UsersService.Application.Validation;
using MicroApp.UsersService.Domain.Entities;
using MicroApp.UsersService.Infrastructure.Email;
using MicroApp.UsersService.Infrastructure.Persistence;
using MicroApp.UsersService.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<UsersDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<VerificationsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddSingleton<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
builder.Services.AddScoped<IVerificationStore, VerificationStore>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddAuthentication("Bearer").AddJwtBearer(o =>
{
    var cfg = builder.Configuration;
    o.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["Jwt:Issuer"],
        ValidAudience = cfg["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(JwtSigning.GetKeyBytes(cfg["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri(builder.Configuration["Services:Users"]!));
builder.Services.AddHttpClient("wallet", c => c.BaseAddress = new Uri(builder.Configuration["Services:Wallet"] ?? "http://walletservice"));

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDb>();

    // Ensure database exists without migrations
    await db.Database.EnsureCreatedAsync();

    // Idempotent schema bootstrap for PostgreSQL when DB already exists (no EF migrations used)
    var createSql = @"
CREATE TABLE IF NOT EXISTS ""Roles"" (
    ""Id"" uuid NOT NULL,
    ""Name"" character varying(100) NOT NULL,
    ""Permissions"" bigint NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL,
    CONSTRAINT ""PK_Roles"" PRIMARY KEY (""Id"")
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Roles_Name"" ON ""Roles"" (""Name"");

CREATE TABLE IF NOT EXISTS ""Users"" (
    ""Id"" uuid NOT NULL,
    ""Email"" character varying(256) NOT NULL,
    ""DisplayName"" character varying(200) NOT NULL,
    ""PasswordHash"" character varying(200) NOT NULL,
    ""IbanHash"" character varying(256) NULL,
    ""DobHash"" character varying(256) NULL,
    ""HashSalt"" character varying(64) NULL,
    ""RoleId"" uuid NOT NULL,
    ""OverridePermissions"" bigint NULL,
    ""VerificationStatus"" integer NOT NULL DEFAULT 0,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Users"" PRIMARY KEY (""Id"")
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
CREATE INDEX IF NOT EXISTS ""IX_Users_RoleId"" ON ""Users"" (""RoleId"");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Users_Roles_RoleId'
    ) THEN
        ALTER TABLE ""Users"" ADD CONSTRAINT ""FK_Users_Roles_RoleId""
        FOREIGN KEY (""RoleId"") REFERENCES ""Roles"" (""Id"") ON DELETE CASCADE;
    END IF;
END
$$;

-- Add VerificationStatus column if it doesn't exist (for existing databases)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'VerificationStatus'
    ) THEN
        ALTER TABLE ""Users"" ADD COLUMN ""VerificationStatus"" integer NOT NULL DEFAULT 0;
    END IF;
END
$$;
";
    await db.Database.ExecuteSqlRawAsync(createSql);

    await UsersDbSeeder.SeedAsync(db);

    // Create default admin user if not exists
    var defaultAdminEmail = "admin@microapp.local"; // hard-coded email
    var defaultAdminName = "System Admin";          // hard-coded display name
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
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = defaultAdminEmail,
                DisplayName = defaultAdminName,
                RoleId = role.Id,
                PasswordHash = hash,
                HashSalt = salt,
                VerificationStatus = VerificationStatus.Completed, // Admin is pre-verified
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
        u.Id,
        u.Email,
        u.DisplayName,
        Role = new { u.Role.Id, u.Role.Name },
        EffectivePermissions = eff
    });
}).RequireAuthorization();

app.MapPost("/internal/users", async (HttpRequest http, UsersDb db, IEmailSender emailSender, IVerificationStore store, IHttpClientFactory httpFactory) =>
{
    if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"])
        return Results.Unauthorized();

    var dto = await http.ReadFromJsonAsync<InternalCreateUserDto>();
    if (dto is null) return Results.BadRequest();

    // If user already exists by email, return its id (idempotency by email)
    var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == dto.email.Trim());
    if (existing is not null)
        return Results.Conflict("Email exists");

    var role = await db.Roles.FirstAsync(r => r.Name == "CTO"); // default role
    var pepper = builder.Configuration["Security:HashPepper"] ?? string.Empty;
    var (pwdHash, salt) = Common.Security.Hashing.HashSecret(dto.password, null, pepper);

    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = dto.email.Trim(),
        DisplayName = dto.displayName.Trim(),
        RoleId = role.Id,
        PasswordHash = pwdHash,
        HashSalt = salt,
        VerificationStatus = VerificationStatus.Pending // Set verification status to Pending
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Best-effort wallet sync (do not fail user creation if this fails)
    try
    {
        var client = httpFactory.CreateClient("wallet");
        client.DefaultRequestHeaders.Add("X-Internal-ApiKey", builder.Configuration["InternalApiKey"]);
        using var _ = await client.PostAsync("/internal/wallets/sync-all-users", null);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Wallet sync failed: {ex.Message}");
    }

    var v = await store.Create(VerificationAction.NewUserCreated, user.Id, Guid.Empty, user.Id);
    var link = $"{builder.Configuration["Frontend:VerificationUrl"] ?? "http://localhost:5072/users/"}?id={user.Id}&code={v.Code}";
    await emailSender.SendAsync(user.Email, "Verify your account", $"Hello {user.DisplayName},\n\nPlease verify your account.\nVerification code: {v.Code}\nOr click: {link}\n\nYou can set your password during verification.");
    return Results.Created($"/users/{user.Id}", new { id = user.Id });
}).ExcludeFromDescription();

// Internal auth verify
app.MapGet("/internal/users/ids", async (HttpRequest http, UsersDb db) =>
{
    if (http.Headers["X-Internal-ApiKey"] != builder.Configuration["InternalApiKey"])
        return Results.Unauthorized();
    var ids = await db.Users.AsNoTracking().Select(u => new { u.Id }).ToListAsync();
    return Results.Ok(ids);
}).ExcludeFromDescription();

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

namespace MicroApp.UsersService
{
    record InternalCreateUserDto(string email, string displayName, string password);
    record InternalVerifyDto(string email, string password);
}