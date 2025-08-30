using System.Security.Claims;
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

    // Idempotent schema bootstrap for PostgreSQL when DB already exists (no EF migrations used)
    var createSql = @"
CREATE TABLE IF NOT EXISTS ""Cards"" (
    ""Id"" uuid NOT NULL,
    ""Type"" integer NOT NULL,
    ""Name"" character varying(200) NOT NULL,
    ""SingleTransactionLimit"" numeric(18,2) NOT NULL,
    ""MonthlyLimit"" numeric(18,2) NOT NULL,
    ""AssignedUserId"" uuid NULL,
    ""Options"" bigint NOT NULL,
    ""Printed"" boolean NOT NULL DEFAULT FALSE,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Cards"" PRIMARY KEY (""Id"")
);
CREATE INDEX IF NOT EXISTS ""IX_Cards_AssignedUserId"" ON ""Cards"" (""AssignedUserId"");
";
    await db.Database.ExecuteSqlRawAsync(createSql);
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