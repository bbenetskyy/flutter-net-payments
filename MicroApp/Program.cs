using System.Globalization;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Services;
using AuthService.Application.DTOs;
using AuthService.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<UsersDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDb>();
    await db.Database.EnsureCreatedAsync();
    await UsersDbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


string pepper = builder.Configuration["Security:HashPepper"] ?? throw new("Missing pepper");

// Створення користувача (адмінський ендпойнт; додай авторизацію за потреби)
app.MapPost("/users", async (CreateUserRequest req, UsersDb db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == req.Email))
        return Results.Conflict("Email exists");

    var role = await db.Roles.FindAsync(req.RoleId);
    if (role is null) return Results.BadRequest("Role not found");

    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = req.Email.Trim(),
        DisplayName = req.DisplayName.Trim(),
        RoleId = role.Id,
        OverridePermissions = req.OverridePermissions
    };

    // IBAN
    if (!string.IsNullOrWhiteSpace(req.Iban))
    {
        var ibanNorm = Hashing.NormalizeIban(req.Iban);
        var (h, s) = Hashing.HashSecret(ibanNorm, null, pepper);
        user.IbanHash = h; user.HashSalt = s; // одну сіль використовуємо для обох
    }
    // DOB
    if (req.DateOfBirth is not null)
    {
        var dobStr = req.DateOfBirth.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
        user.DobHash = h; user.HashSalt ??= s2;
    }

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", await ToResponse(user, db));
});

// Оновлення користувача
app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest req, UsersDb db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(req.DisplayName)) user.DisplayName = req.DisplayName.Trim();
    if (req.RoleId is Guid rid)
    {
        if (await db.Roles.FindAsync(rid) is null) return Results.BadRequest("Role not found");
        user.RoleId = rid;
    }
    if (req.OverridePermissions is not null) user.OverridePermissions = req.OverridePermissions;

    if (!string.IsNullOrWhiteSpace(req.Iban))
    {
        var ibanNorm = Hashing.NormalizeIban(req.Iban);
        var (h, s) = Hashing.HashSecret(ibanNorm, user.HashSalt, pepper);
        user.IbanHash = h; user.HashSalt ??= s;
    }
    if (req.DateOfBirth is not null)
    {
        var dobStr = req.DateOfBirth.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var (h, s2) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);
        user.DobHash = h; user.HashSalt ??= s2;
    }
    user.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(await ToResponse(user, db));
});

// Отримання списку
app.MapGet("/users", async (UsersDb db) =>
    Results.Ok(await db.Users
        .Include(u => u.Role)
        .OrderBy(x => x.DisplayName)
        .Select(u => new {
            u.Id, u.Email, u.DisplayName,
            Role = u.Role.Name,
            EffectivePermissions = (long)(u.OverridePermissions ?? u.Role.Permissions),
            HasIban = u.IbanHash != null, HasDateOfBirth = u.DobHash != null,
            u.CreatedAt
        }).ToListAsync())
);

// Перевірка IBAN/DOB (наприклад, при валідації на бекенді без зберігання plain)
app.MapPost("/users/{id:guid}/verify", async (Guid id, string iban, DateOnly dob, UsersDb db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    var ibanNorm = Hashing.NormalizeIban(iban);
    var (ibanHash, _) = Hashing.HashSecret(ibanNorm, user.HashSalt, pepper);
    var dobStr = dob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    var (dobHash, _) = Hashing.HashSecret(dobStr, user.HashSalt, pepper);

    var ok = user.IbanHash == ibanHash && user.DobHash == dobHash;
    return ok ? Results.Ok() : Results.Unauthorized();
});

app.Run();

static async Task<UserResponse> ToResponse(User u, UsersDb db)
{
    var role = await db.Roles.FindAsync(u.RoleId) ?? throw new InvalidOperationException();
    var eff = u.OverridePermissions ?? role.Permissions;
    return new UserResponse(
        u.Id, u.Email, u.DisplayName, 
        role.Id, role.Name, eff,
        HasIban: u.IbanHash != null, HasDateOfBirth: u.DobHash != null, u.CreatedAt);
}


