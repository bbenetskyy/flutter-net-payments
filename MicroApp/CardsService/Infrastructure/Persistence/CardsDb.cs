using CardsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CardsService.Domain.Enums;

namespace CardsService.Infrastructure.Persistence;

public class CardsDb(DbContextOptions<CardsDb> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Card>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Type).HasConversion<int>();
            e.Property(x => x.Options).HasConversion<long>();
            e.Property(x => x.SingleTransactionLimit).HasPrecision(18, 2);
            e.Property(x => x.MonthlyLimit).HasPrecision(18, 2);
            e.Property(x => x.Printed).HasDefaultValue(false);
            e.HasIndex(x => x.AssignedUserId);
        });
    }
}
