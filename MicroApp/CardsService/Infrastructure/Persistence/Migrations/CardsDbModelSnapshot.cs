using System;
using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CardsService.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CardsDb))]
    partial class CardsDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.8");

            modelBuilder.Entity("CardsService.Domain.Entities.Card", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uniqueidentifier");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<decimal>("MonthlyLimit").HasColumnType("decimal(18,2)");
                b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                b.Property<long>("Options").HasColumnType("bigint");
                b.Property<bool>("Printed").HasColumnType("bit");
                b.Property<decimal>("SingleTransactionLimit").HasColumnType("decimal(18,2)");
                b.Property<int>("Type").HasColumnType("int");
                b.Property<DateTime?>("UpdatedAt").HasColumnType("datetime2");
                b.Property<Guid?>("AssignedUserId").HasColumnType("uniqueidentifier");

                b.HasKey("Id");
                b.HasIndex("AssignedUserId");
                b.ToTable("Cards");
            });
#pragma warning restore 612, 618
        }
    }
}
