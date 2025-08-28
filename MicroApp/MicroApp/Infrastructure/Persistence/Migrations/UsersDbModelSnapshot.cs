using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MicroApp.Domain.Entities;
using MicroApp.Infrastructure.Persistence;

#nullable disable

namespace MicroApp.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(UsersDb))]
    partial class UsersDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.8");

            modelBuilder.Entity("MicroApp.Domain.Entities.Role", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uniqueidentifier");
                b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<long>("Permissions").HasColumnType("bigint");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("Name").IsUnique();
                b.ToTable("Roles");
            });

            modelBuilder.Entity("MicroApp.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uniqueidentifier");
                b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("DisplayName").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("PasswordHash").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<string>("IbanHash").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("DobHash").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("HashSalt").HasMaxLength(64).HasColumnType("nvarchar(64)");
                b.Property<long?>("OverridePermissions").HasColumnType("bigint");
                b.Property<Guid>("RoleId").HasColumnType("uniqueidentifier");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<DateTime?>("UpdatedAt").HasColumnType("datetime2");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.HasIndex("RoleId");
                b.ToTable("Users");
            });

            modelBuilder.Entity("MicroApp.Domain.Entities.User", b =>
            {
                b.HasOne("MicroApp.Domain.Entities.Role", "Role")
                    .WithMany("Users")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("Role");
            });

            modelBuilder.Entity("MicroApp.Domain.Entities.Role", b =>
            {
                b.Navigation("Users");
            });
#pragma warning restore 612, 618
        }
    }
}
