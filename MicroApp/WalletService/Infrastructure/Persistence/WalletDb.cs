using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;
using Common.Domain.Entities;

namespace WalletService.Infrastructure.Persistence;

public class WalletDb(DbContextOptions<WalletDb> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerEntry> Ledger => Set<LedgerEntry>();
    public DbSet<Common.Domain.Entities.Account> Accounts => Set<Common.Domain.Entities.Account>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Wallet>(e =>
        {
            e.ToTable("Wallets");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
        });

        b.Entity<LedgerEntry>(e =>
        {
            e.ToTable("LedgerEntries");
            e.HasKey(x => x.Id);
            e.Property(x => x.AmountMinor).HasConversion<long>();
            e.Property(x => x.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            e.Property(x => x.Type).HasConversion<int>();
            e.Property(x => x.Account).HasConversion<int>();
            e.Property(x => x.CounterpartyAccount).HasConversion<int>();
            e.HasIndex(x => new { x.WalletId, x.CorrelationId }).IsUnique(); // idempotency per wallet
            e.HasOne<Wallet>().WithMany().HasForeignKey(x => x.WalletId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Common.Domain.Entities.Account>(e =>
        {
            e.ToTable("Accounts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Iban).IsRequired().HasMaxLength(64);
            e.Property(x => x.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            // Enforce global uniqueness of IBAN
            e.HasIndex(x => x.Iban).IsUnique();
        });
    }
}
