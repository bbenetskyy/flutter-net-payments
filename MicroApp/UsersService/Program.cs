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

    try
    {
        await db.Database.ExecuteSqlRawAsync(SqlCreateScript.Script);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[UsersService] Schema bootstrap failed: {ex.Message}");
        throw;
    }
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
app.MapInternalUserEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
