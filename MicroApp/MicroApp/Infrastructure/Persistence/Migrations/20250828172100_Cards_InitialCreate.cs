using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MicroApp.Infrastructure.Persistence;

#nullable disable

namespace MicroApp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CardsDb))]
[Migration("20250828172100_Cards_InitialCreate")]
public partial class Cards_InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Cards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SingleTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                MonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Options = table.Column<long>(type: "bigint", nullable: false),
                Printed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cards", x => x.Id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_Cards_AssignedUserId",
            table: "Cards",
            column: "AssignedUserId"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Cards");
    }
}
