using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;
using WalletService.Domain.Events;
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
        .Where(x => x.WalletId == w.Id && x.Account == LedgerAccount.Cash)
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

// External top-up endpoint: credit user's wallet from outside system (idempotent via correlationId)
app.MapPost("/wallets/{userId:guid}/topup", async (Guid userId, TopUpRequest req, WalletDb db) =>
{
    if (userId == Guid.Empty || req.AmountMinor <= 0)
        return Results.BadRequest(new { error = "invalid_request" });

    // Create wallet if it doesn't exist
    var wallet = await db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
    if (wallet == null)
    {
        wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
    }

    var correlationId = req.CorrelationId ?? Guid.NewGuid();

    // Idempotency per wallet
    var exists = await db.Ledger.AsNoTracking().AnyAsync(x => x.WalletId == wallet.Id && x.CorrelationId == correlationId);
    if (exists)
        return Results.Ok(new { status = "idempotent", correlationId });

    var cash = LedgerAccount.Cash;
    var clearing = LedgerAccount.Clearing;

    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        // Credit cash, debit clearing (balanced within same wallet)
        db.Ledger.Add(new LedgerEntry
        {
            Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = req.AmountMinor, Currency = req.Currency,
            Type = LedgerEntryType.Credit, Account = cash, CounterpartyAccount = clearing,
            CorrelationId = correlationId, Description = req.Description
        });
        db.Ledger.Add(new LedgerEntry
        {
            Id = Guid.NewGuid(), WalletId = wallet.Id, AmountMinor = req.AmountMinor, Currency = req.Currency,
            Type = LedgerEntryType.Debit, Account = clearing, CounterpartyAccount = cash,
            CorrelationId = correlationId, Description = req.Description
        });

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }

    // Return updated balance snapshot for this currency
    var balance = await db.Ledger.AsNoTracking()
        .Where(x => x.WalletId == wallet.Id && x.Currency == req.Currency && x.Account == cash)
        .SumAsync(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor);

    return Results.Ok(new { status = "applied", correlationId, walletId = wallet.Id, userId, currency = req.Currency, balanceMinor = balance });
}).RequirePermission(UserPermissions.ConfirmPayments);

// internal endpoint to apply a payment domain event (idempotent double-entry with cross-wallet transfer)
app.MapPost("/internal/events/payment", async (PaymentEvent evt, WalletDb db) =>
{
    // Basic validation
    if (evt.IntentId == Guid.Empty || evt.UserId == Guid.Empty || evt.AmountMinor <= 0)
        return Results.BadRequest(new { error = "invalid_event" });

    var cash = LedgerAccount.Cash;
    var clearing = LedgerAccount.Clearing;

    // Determine direction
    var captured = evt.EventType == Common.Domain.Enums.PaymentEventType.PaymentCaptured;
    var payerId = captured ? evt.UserId : evt.BeneficiaryId; // source of funds
    var payeeId = captured ? evt.BeneficiaryId : evt.UserId; // destination

    var hasCounterparty = payeeId != Guid.Empty && payerId != Guid.Empty && payerId != payeeId;

    // Load/create wallets
    Wallet? payerWallet = null;
    if (payerId != Guid.Empty)
    {
        payerWallet = await db.Wallets.FirstOrDefaultAsync(x => x.UserId == payerId);
        if (payerWallet == null)
        {
            payerWallet = new Wallet { Id = Guid.NewGuid(), UserId = payerId };
            db.Wallets.Add(payerWallet);
            await db.SaveChangesAsync();
        }
    }

    Wallet? payeeWallet = null;
    if (hasCounterparty)
    {
        payeeWallet = await db.Wallets.FirstOrDefaultAsync(x => x.UserId == payeeId);
        if (payeeWallet == null)
        {
            payeeWallet = new Wallet { Id = Guid.NewGuid(), UserId = payeeId };
            db.Wallets.Add(payeeWallet);
            await db.SaveChangesAsync();
        }
    }

    // Idempotency across both wallets
    var anyExists = await db.Ledger.AnyAsync(x => x.CorrelationId == evt.IntentId);
    if (anyExists) return Results.Ok(new { status = "idempotent" });

    // For cross-wallet transfer ensure payer has enough cash
    if (hasCounterparty && payerWallet != null)
    {
        var payerCash = await db.Ledger
            .Where(x => x.WalletId == payerWallet.Id && x.Currency == evt.Currency && x.Account == cash)
            .SumAsync(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor);
        if (payerCash < evt.AmountMinor)
            return Results.BadRequest(new { error = "insufficient_funds", availableMinor = payerCash });
    }

    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        if (hasCounterparty && payerWallet != null && payeeWallet != null)
        {
            if (captured)
            {
                // Payer: outflow (debit cash, credit clearing)
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payerWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payerWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });

                // Payee: inflow (credit cash, debit clearing)
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payeeWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payeeWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
            }
            else
            {
                // Refund/Chargeback: reverse
                // Beneficiary (original payee) outflow
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payerWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payerWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });

                // User (original payer) inflow
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payeeWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = payeeWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
            }
        }
        else
        {
            // Fallback to single-wallet effect on evt.UserId for compatibility (top-up or reversal)
            var userWallet = payerWallet ?? await db.Wallets.FirstOrDefaultAsync(x => x.UserId == evt.UserId);
            if (userWallet == null)
            {
                userWallet = new Wallet { Id = Guid.NewGuid(), UserId = evt.UserId };
                db.Wallets.Add(userWallet);
                await db.SaveChangesAsync();
            }

            if (captured)
            {
                // Inflow to user's wallet
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = userWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = userWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
            }
            else
            {
                // Outflow from user's wallet
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = userWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Debit, Account = cash, CounterpartyAccount = clearing,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
                db.Ledger.Add(new LedgerEntry
                {
                    Id = Guid.NewGuid(), WalletId = userWallet.Id, AmountMinor = evt.AmountMinor, Currency = evt.Currency,
                    Type = LedgerEntryType.Credit, Account = clearing, CounterpartyAccount = cash,
                    CorrelationId = evt.IntentId, Description = evt.Description
                });
            }
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
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

public sealed record TopUpRequest(
    long AmountMinor, 
    Common.Domain.Enums.Currency Currency, 
    Guid? CorrelationId, 
    string? Description);

internal sealed class UserSlim
{
    public Guid Id { get; set; }
}
