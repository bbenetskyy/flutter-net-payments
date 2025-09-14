using Microsoft.EntityFrameworkCore;
using WalletService.Application.DTOs;
using WalletService.Domain.Entities;
using WalletService.Domain.Events;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Presentation.Endpoints;

public static class InternalEndpoints
{
    public static void MapInternalEndpoints(this IEndpointRouteBuilder app)
    {
        var builder = app as WebApplication;
        if (builder is null) return;
        
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
    }
}
