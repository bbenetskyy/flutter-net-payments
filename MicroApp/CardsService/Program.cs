using CardsService.Application.DTOs;
using Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CardsService.Infrastructure.Persistence;
using CardsService.Presentation.Endpoints;
using Common.Validation;
using CardsService.Application.Validation;
using Common.Infrastucture.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CardsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<VerificationsDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddSingleton<IValidator<CreateCardRequest>, CreateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardRequest>, UpdateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<AssignCardRequest>, AssignCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardOperation>, UpdateCardOperationValidator>();
builder.Services.AddSingleton<IValidator<AssignCardOperation>, AssignCardOperationValidator>();
builder.Services.AddScoped<IVerificationStore, VerificationStore>();
builder.Services.AddScoped<ICardVerificationService, CardVerificationService>();

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

// Named HTTP client to talk to UsersService for permission checks
builder.Services.AddHttpClient("users", c =>
{
    var baseAddress = builder.Configuration["Services:Users"]!;
    c.BaseAddress = new Uri(baseAddress);
});

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CardsDb>();
    await db.Database.EnsureCreatedAsync();
    
    try
    {
        await db.Database.ExecuteSqlRawAsync(SqlCreateScript.Script);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CardsService] Schema bootstrap failed: {ex.Message}");
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

app.MapCardsEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();