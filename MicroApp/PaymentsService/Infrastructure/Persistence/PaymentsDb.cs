using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Persistence;

public class PaymentsDb(DbContextOptions<PaymentsDb> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.BeneficiaryName).IsRequired().HasMaxLength(200);
            e.Property(x => x.BeneficiaryAccount).IsRequired().HasMaxLength(64);
            e.Property(x => x.FromAccount).IsRequired().HasMaxLength(64);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            // store enum as 3-letter string (EUR/USD/GBP)
            e.Property(x => x.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            e.Property(x => x.Status).HasConversion<int>();
            e.HasIndex(x => x.UserId);
        });
    }
}
