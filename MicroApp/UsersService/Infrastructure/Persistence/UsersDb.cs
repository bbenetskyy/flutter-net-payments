using MicroApp.UsersService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroApp.UsersService.Infrastructure.Persistence;

public class UsersDb(DbContextOptions<UsersDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Role>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Permissions).HasConversion<long>();
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            // Map Iban property to IbanHash column to avoid DB schema changes
            e.Property(u => u.Iban).HasColumnName("IbanHash").HasMaxLength(256);
            e.Property(x => x.DobHash).HasMaxLength(256);
            e.Property(x => x.HashSalt).HasMaxLength(64);
            e.Property(x => x.OverridePermissions).HasConversion<long>();
            e.Property(x => x.VerificationStatus).HasConversion<int>();
            e.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
        });
    }
}