using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using CardsService.Infrastructure.Persistence;

#nullable disable

namespace CardsService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CardsDb))]
[Migration("20250828135000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Cards table if not exists
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cards]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Cards](
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Type] INT NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [SingleTransactionLimit] DECIMAL(18,2) NOT NULL,
        [MonthlyLimit] DECIMAL(18,2) NOT NULL,
        [AssignedUserId] UNIQUEIDENTIFIER NULL,
        [Options] BIGINT NOT NULL,
        [Printed] BIT NOT NULL CONSTRAINT [DF_Cards_Printed] DEFAULT(0),
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Cards] PRIMARY KEY ([Id])
    );
END
");
        // Index on AssignedUserId if not exists
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cards_AssignedUserId' AND object_id = OBJECT_ID('[dbo].[Cards]'))
BEGIN
    CREATE INDEX [IX_Cards_AssignedUserId] ON [dbo].[Cards]([AssignedUserId]);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop default constraint if exists, then drop table
        migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[Cards]', 'U') IS NOT NULL
BEGIN
    DECLARE @dfName nvarchar(128);
    SELECT @dfName = df.name
    FROM sys.default_constraints df
    INNER JOIN sys.columns c ON c.default_object_id = df.object_id
    WHERE df.parent_object_id = OBJECT_ID('[dbo].[Cards]') AND c.name = 'Printed';
    IF @dfName IS NOT NULL EXEC('ALTER TABLE [dbo].[Cards] DROP CONSTRAINT [' + @dfName + ']');
    DROP TABLE [dbo].[Cards];
END
");
    }
}
