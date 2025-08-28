using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CardsService.Infrastructure.Persistence;

#nullable disable

namespace CardsService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CardsDb))]
[Migration("20250828140200_AddPrintedToCards")]
public partial class AddPrintedToCards : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotently add Printed column to Cards table
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Cards]') AND c.name = 'Printed'
)
BEGIN
    ALTER TABLE [dbo].[Cards] ADD [Printed] BIT NOT NULL CONSTRAINT DF_Cards_Printed DEFAULT (0);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Idempotently drop Printed column
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Cards]') AND c.name = 'Printed'
)
BEGIN
    ALTER TABLE [dbo].[Cards] DROP CONSTRAINT DF_Cards_Printed;
    ALTER TABLE [dbo].[Cards] DROP COLUMN [Printed];
END
");
    }
}
