using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastucture.Persistence;

public class VerificationsDb(DbContextOptions<VerificationsDb> options) : DbContext(options)
{
    public DbSet<Verification> Verifications => Set<Verification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Verification>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Action).IsRequired();
                e.Property(x => x.TargetId).IsRequired();
                e.Property(x => x.Status).IsRequired();
                e.Property(x => x.Code).IsRequired().HasMaxLength(10);
                e.Property(x => x.CreatedBy).IsRequired();
            }
        );
    }
}
