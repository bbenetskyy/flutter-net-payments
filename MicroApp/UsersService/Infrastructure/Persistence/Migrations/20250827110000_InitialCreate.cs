using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AuthService.Infrastructure.Persistence;

#nullable disable

namespace AuthService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(UsersDb))]
[Migration("20250827110000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Roles table if not exists
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles](
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Permissions] BIGINT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END
");
        // Unique index on Roles.Name
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_Name' AND object_id = OBJECT_ID('[dbo].[Roles]'))
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [dbo].[Roles]([Name]);
END
");

        // Create Users table if not exists
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [DisplayName] NVARCHAR(200) NOT NULL,
        [PasswordHash] NVARCHAR(200) NOT NULL,
        [IbanHash] NVARCHAR(256) NULL,
        [DobHash] NVARCHAR(256) NULL,
        [HashSalt] NVARCHAR(64) NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [OverridePermissions] BIGINT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END
");
        // Indexes for Users
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('[dbo].[Users]'))
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
END
");
        migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_RoleId' AND object_id = OBJECT_ID('[dbo].[Users]'))
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [dbo].[Users]([RoleId]);
END
");
        // Add FK if not exists
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Roles_RoleId' AND parent_object_id = OBJECT_ID('[dbo].[Users]')
)
BEGIN
    ALTER TABLE [dbo].[Users] WITH CHECK ADD CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY([RoleId])
    REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE;
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop FK if exists
        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Roles_RoleId' AND parent_object_id = OBJECT_ID('[dbo].[Users]'))
BEGIN
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Roles_RoleId];
END
");
        // Drop Users if exists
        migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[Users]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Users];
END
");
        // Drop Roles if exists
        migrationBuilder.Sql(@"
IF OBJECT_ID('[dbo].[Roles]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Roles];
END
");
    }
}
