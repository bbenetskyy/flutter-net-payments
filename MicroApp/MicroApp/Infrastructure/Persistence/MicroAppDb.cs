using MicroApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.Infrastructure.Persistence;

public class MicroAppDb(DbContextOptions<MicroAppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Roles configuration (from UsersDb)
        b.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Permissions).HasConversion<long>();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Users configuration (from UsersDb)
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            e.Property(x => x.IbanHash).HasMaxLength(256);
            e.Property(x => x.DobHash).HasMaxLength(256);
            e.Property(x => x.HashSalt).HasMaxLength(64);
            e.Property(x => x.OverridePermissions).HasConversion<long>();
            e.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
        });

        // Cards configuration (from CardsDb)
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