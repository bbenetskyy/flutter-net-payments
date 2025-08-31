using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;
using WalletService.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();

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

    var createSql = @"
CREATE TABLE IF NOT EXISTS ""Wallets"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ""PK_Wallets"" PRIMARY KEY (""Id"")
);

CREATE UNIQUE INDEX IF NOT EXISTS ""UX_Wallets_UserId""
ON ""Wallets"" (""UserId"");

CREATE TABLE IF NOT EXISTS ""LedgerEntries"" (
    ""Id"" uuid NOT NULL,
    ""WalletId"" uuid NOT NULL,
    ""AmountMinor"" bigint NOT NULL,
    ""Currency"" character varying(3) NOT NULL DEFAULT 'EUR',
    ""Type"" integer NOT NULL,
    ""Account"" integer NOT NULL DEFAULT 1,
    ""CounterpartyAccount"" integer NOT NULL DEFAULT 2,
    ""Description"" text NULL,
    ""CorrelationId"" uuid NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ""PK_LedgerEntries"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_LedgerEntries_Wallets_WalletId""
        FOREIGN KEY (""WalletId"") REFERENCES ""Wallets"" (""Id"") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ""UX_LedgerEntries_WalletId_CorrelationId""
ON ""LedgerEntries"" (""WalletId"", ""CorrelationId"");
";
    await db.Database.ExecuteSqlRawAsync(createSql);
}
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Internal sync: ensure every existing user has a wallet
app.MapPost("/internal/wallets/sync-all-users", async (IHttpClientFactory httpFactory, WalletDb db) =>
{
    var client = httpFactory.CreateClient("users");
    client.DefaultRequestHeaders.Add("X-Internal-ApiKey", builder.Configuration["InternalApiKey"]);
    using var res = await client.GetAsync("/internal/users/ids");
    if (!res.IsSuccessStatusCode)
        return Results.StatusCode((int)res.StatusCode);

    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var users = await res.Content.ReadFromJsonAsync<List<UserSlim>>(options) ?? new();

    // Build a HashSet of existing userIds with wallets
    var existing = await db.Wallets.AsNoTracking().Select(w => w.UserId).ToListAsync();
    var existingSet = new HashSet<Guid>(existing);

    int created = 0;
    foreach (var u in users)
    {
        if (!existingSet.Contains(u.Id))
        {
            db.Wallets.Add(new Wallet { Id = Guid.NewGuid(), UserId = u.Id });
            created++;
        }
    }
    if (created > 0)
        await db.SaveChangesAsync();

    return Results.Ok(new { totalUsers = users.Count, created });
}).ExcludeFromDescription();

app.MapGet("/wallets/{userId:guid}", async (Guid userId, WalletDb db) =>
{
    var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    if (w == null) return Results.NotFound();

    // compute balance by currency in minor units
    var hasEntries = await db.Ledger.AsNoTracking().AnyAsync(x => x.WalletId == w.Id);
    if (!hasEntries)
    {
        return Results.Ok(new { walletId = w.Id, userId = w.UserId, balances = Array.Empty<object>() });
    }
    var byCurrency = await db.Ledger.AsNoTracking()
        .Where(x => x.WalletId == w.Id)
        .GroupBy(x => x.Currency)
        .Select(g => new
        {
            currency = g.Key,
            balanceMinor = g.Sum(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor)
        }).ToListAsync();

    return Results.Ok(new { walletId = w.Id, userId = w.UserId, balances = byCurrency });
});

// read ledger entries (optional filtering by correlation)
app.MapGet("/wallets/{userId:guid}/ledger", async (Guid userId, Guid? correlationId, WalletDb db) =>
{
    var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    if (w == null) return Results.NotFound();
    IQueryable<LedgerEntry> q = db.Ledger.AsNoTracking().Where(x => x.WalletId == w.Id);
    if (correlationId.HasValue) q = q.Where(x => x.CorrelationId == correlationId.Value);
    var items = await q.OrderBy(x => x.CreatedAt).ToListAsync();
    return Results.Ok(items);
});

// internal endpoint to apply a payment domain event (idempotent double-entry)
app.MapPost("/internal/events/payment", async (PaymentEvent evt, WalletDb db) =>
{
    // Find or create wallet for user
    var wallet = await db.Wallets.FirstOrDefaultAsync(x => x.UserId == evt.UserId);
    if (wallet == null)
    {
        wallet = new Wallet { Id = Guid.NewGuid(), UserId = evt.UserId };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
    }

    // Idempotency: if entries with same correlation exist, return 200 OK
    var exists = await db.Ledger.AnyAsync(x => x.WalletId == wallet.Id && x.CorrelationId == evt.IntentId);
    if (exists) return Results.Ok(new { status = "idempotent" });

    // Double-entry: for captured payment -> credit cash, debit clearing
    // Refund -> debit cash, credit clearing
    var isCredit = evt.EventType == Common.Domain.Enums.PaymentEventType.PaymentCaptured;
    // For credit: Cash <-, Clearing ->; For refund/chargeback: Cash ->, Clearing <-
    var cash = LedgerAccount.Cash;
    var clearing = LedgerAccount.Clearing;

    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        if (isCredit)
        {
            db.Ledger.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                Type = LedgerEntryType.Credit, Account = cash, CounterpartyAccount = clearing,
                CorrelationId = evt.IntentId, Description = evt.Description
            });
            db.Ledger.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                Type = LedgerEntryType.Debit, Account = clearing, CounterpartyAccount = cash,
                CorrelationId = evt.IntentId, Description = evt.Description
            });
        }
        else
        {
            db.Ledger.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                Type = LedgerEntryType.Debit, Account = cash, CounterpartyAccount = clearing,
                CorrelationId = evt.IntentId, Description = evt.Description
            });
            db.Ledger.Add(new LedgerEntry
            {
                Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                Type = LedgerEntryType.Credit, Account = clearing, CounterpartyAccount = cash,
                CorrelationId = evt.IntentId, Description = evt.Description
            });
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }

    return Results.Ok(new { status = "applied" });
}).ExcludeFromDescription();

// Ensure DB exists and migrations applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDb>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

internal sealed class UserSlim
{
    public Guid Id { get; set; }
}
