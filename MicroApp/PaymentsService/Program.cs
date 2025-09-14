using System.Security.Claims;
using System.Text.Json.Serialization;
using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Endpoints;
using PaymentsService.Application.Rates;
using Common.Domain.Enums;

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

// Provider webhook receiver -> emits domain events to WalletService
app.MapPost("/payments/webhooks/provider", async (HttpContext http) =>
{
    var payload = await http.Request.ReadFromJsonAsync<ProviderWebhook>();
    if (payload is null) return Results.BadRequest();

    // Minimal validation
    if (payload.IntentId == Guid.Empty || payload.UserId == Guid.Empty || payload.Amount <= 0)
        return Results.BadRequest();

    // Pass-through original currency; compute minor units from payload.Amount (2 decimals)
    var amountRounded = Math.Round(payload.Amount, 2, MidpointRounding.AwayFromZero);
    long amountMinor = (long)Math.Round(amountRounded * 100m, 0, MidpointRounding.AwayFromZero);

    var client = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("wallet");
    var evt = new WalletEvent
    {
        IntentId = payload.IntentId,
        UserId = payload.UserId,
        BeneficiaryId = payload.BeneficiaryId,
        AmountMinor = amountMinor,
        Currency = payload.Currency,
        EventType = payload.Type?.ToLowerInvariant()
                .Replace("_", "")
                .Replace("-","")
                .Replace(" ","")
            switch
        {
            "refundsucceeded" => Common.Domain.Enums.PaymentEventType.RefundSucceeded,
            "chargebackreceived" => Common.Domain.Enums.PaymentEventType.ChargebackReceived,
            _ => Common.Domain.Enums.PaymentEventType.PaymentCaptured
        },
        Description = payload.Description
    };

    using var res = await client.PostAsJsonAsync("/internal/events/payment", evt);
    if (!res.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)res.StatusCode);
    }

    return Results.Ok(new { status = "received" });
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
