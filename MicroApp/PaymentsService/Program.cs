using System.Security.Claims;
using Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PaymentsDb>(o =>
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

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure DB created and idempotent schema
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDb>();
    await db.Database.EnsureCreatedAsync();

    var createSql = @"
CREATE TABLE IF NOT EXISTS ""Payments"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""BeneficiaryName"" character varying(200) NOT NULL,
    ""BeneficiaryAccount"" character varying(64) NOT NULL,
    ""FromAccount"" character varying(64) NOT NULL,
    ""Amount"" numeric(18,2) NOT NULL,
    ""Currency"" character varying(3) NOT NULL,
    ""Details"" text NULL,
    ""Status"" integer NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Payments"" PRIMARY KEY (""Id"")
);
CREATE INDEX IF NOT EXISTS ""IX_Payments_UserId"" ON ""Payments"" (""UserId"");
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

app.MapPaymentsEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
