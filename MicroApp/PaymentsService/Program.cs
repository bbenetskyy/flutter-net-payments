using System.Text.Json.Serialization;
using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Endpoints;
using PaymentsService.Application;
using PaymentsService.Application.DTOs;
using PaymentsService.Application.Validation;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();

// JSON: serialize enums as strings (e.g., "EUR")
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<PaymentsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<VerificationsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddScoped<IVerificationStore, VerificationStore>();

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
        IssuerSigningKey = new SymmetricSecurityKey(JwtSigning.GetKeyBytes(cfg["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

// Named HTTP client to talk to UsersService for permission checks
builder.Services.AddHttpClient("users", c =>
{
    var baseAddress = builder.Configuration["Services:Users"]!;
    c.BaseAddress = new Uri(baseAddress);
});

// Named HTTP client to publish domain events to WalletService
builder.Services.AddHttpClient("wallet", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:Wallet"]!);
});

// Exchange rates
builder.Services.AddSingleton<IExchangeRateService, HardcodedExchangeRateService>();
// Validators
builder.Services.AddSingleton<IValidator<CreatePaymentRequest>, CreatePaymentRequestValidator>();

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure DB created and idempotent schema
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDb>();
    await db.Database.EnsureCreatedAsync();

    try
    {
        await db.Database.ExecuteSqlRawAsync(SqlCreateScript.Script);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[PaymentsService] Schema bootstrap failed: {ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapPaymentsEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
