using Common.Security;
using Microsoft.EntityFrameworkCore;
using WalletService.Application.DTOs;
using WalletService.Application.DTOs.Response;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;
using WalletService.Presentation.Security;

namespace WalletService.Presentation.Endpoints;

public static class WalletsEndpoints
{
    public static void MapWalletsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/{userId:guid}", async (Guid userId, WalletDb db) =>
        {
            var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            if (w == null) return Results.NotFound();

            // compute balance by currency in minor units
            var hasEntries = await db.Ledger.AsNoTracking().AnyAsync(x => x.WalletId == w.Id);
            if (!hasEntries)
            {
                return Results.Ok(new WalletOverviewResponse(w.Id, w.UserId, Array.Empty<WalletBalanceItem>()));
            }
            var byCurrency = await db.Ledger.AsNoTracking()
                .Where(x => x.WalletId == w.Id && x.Account == LedgerAccount.Cash)
                .GroupBy(x => x.Currency)
                .Select(g => new WalletBalanceItem(
                    g.Key,
                    g.Sum(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor)
                )).ToListAsync();

            return Results.Ok(new WalletOverviewResponse(w.Id, w.UserId, byCurrency));
        });

        // read ledger entries (optional filtering by correlation)
        app.MapGet("/wallets/{userId:guid}/ledger", async (Guid userId, Guid? correlationId, WalletDb db) =>
        {
            var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            if (w == null) return Results.NotFound();
            IQueryable<LedgerEntry> q = db.Ledger.AsNoTracking().Where(x => x.WalletId == w.Id && x.Account == LedgerAccount.Cash);
            if (correlationId.HasValue) q = q.Where(x => x.CorrelationId == correlationId.Value);
            var items = await q.OrderBy(x => x.CreatedAt)
                .Select(x => new LedgerEntryResponse(
                    x.Id,
                    x.WalletId,
                    x.AmountMinor,
                    x.Currency,
                    x.Type,
                    x.Account,
                    x.CounterpartyAccount,
                    x.Description,
                    x.CorrelationId,
                    x.CreatedAt
                ))
                .ToListAsync();
            return Results.Ok(items);
        });

        // External top-up endpoint: credit user's wallet from outside system (idempotent via correlationId)
        app.MapPost("/wallets/{userId:guid}/topup", async (Guid userId, TopUpRequest req, WalletDb db) =>
        {
            if (userId == Guid.Empty || req.AmountMinor <= 0)
                return Results.BadRequest(new ErrorResponse("invalid_request"));

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
                return Results.Ok(new TopUpIdempotentResponse("idempotent", correlationId));

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

            return Results.Ok(new TopUpAppliedResponse(
                "applied",
                correlationId,
                wallet.Id,
                userId,
                req.Currency,
                balance
            ));
        }).RequirePermission(UserPermissions.ConfirmPayments);
    }
}
