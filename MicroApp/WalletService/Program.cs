using System.Text.Json.Serialization;
using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WalletService;
using WalletService.Domain.Entities;
using WalletService.Domain.Events;
using WalletService.Infrastructure.Persistence;
using WalletService.Presentation.Endpoints;
using WalletService.Presentation.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();

// JSON: parse enums as strings (e.g., "EUR") for incoming events
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Storage: SQLite for demo; keep PII out of wallet DB
builder.Services.AddDbContext<WalletDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<VerificationsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddScoped<IVerificationStore, VerificationStore>();

builder.Services.AddHttpClient("users", c => c.BaseAddress = new Uri(builder.Configuration["Services:Users"]!));


var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDb>();

    // Ensure database exists without migrations
    await db.Database.EnsureCreatedAsync();

    try
    {
        await db.Database.ExecuteSqlRawAsync(SqlCreateScript.Script);
        Console.WriteLine("[WalletService] Schema bootstrap executed: Wallets, LedgerEntries, Accounts ensured.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WalletService] Schema bootstrap failed: {ex.Message}");
        throw;
    }
}
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Accounts endpoints
app.MapAccountsEndpoints();
app.MapWalletsEndpoints();
app.MapInternalEndpoints();

// Ensure DB exists and migrations applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDb>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
