using System.Security.Claims;
using Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CardsService.Infrastructure.Persistence;
using CardsService.Presentation.Endpoints;
using Common.Validation;
using CardsService.Application.Validation;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CardsDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddSingleton<IValidator<CreateCardRequest>, CreateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardRequest>, UpdateCardRequestValidator>();
builder.Services.AddSingleton<IValidator<AssignCardRequest>, AssignCardRequestValidator>();
builder.Services.AddSingleton<IValidator<UpdateCardOperation>, UpdateCardOperationValidator>();
builder.Services.AddSingleton<IValidator<AssignCardOperation>, AssignCardOperationValidator>();

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

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CardsDb>();
    await db.Database.MigrateAsync();
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