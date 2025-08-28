using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AuthService.Infrastructure.Persistence;

#nullable disable

namespace AuthService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(UsersDb))]
[Migration("20250827214800_AddPasswordAndSalt")]
public partial class AddPasswordAndSalt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent add columns (works whether columns exist or not)
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Users]') AND c.name = 'PasswordHash'
)
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordHash] NVARCHAR(200) NULL;
END
");
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Users]') AND c.name = 'HashSalt'
)
BEGIN
    ALTER TABLE [dbo].[Users] ADD [HashSalt] NVARCHAR(64) NULL;
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Make Down also idempotent for safety
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Users]') AND c.name = 'PasswordHash'
)
BEGIN
    ALTER TABLE [dbo].[Users] DROP COLUMN [PasswordHash];
END
");
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    WHERE o.object_id = OBJECT_ID('[dbo].[Users]') AND c.name = 'HashSalt'
)
BEGIN
    ALTER TABLE [dbo].[Users] DROP COLUMN [HashSalt];
END
");
    }
}
