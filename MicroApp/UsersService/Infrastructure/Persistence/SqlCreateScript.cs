namespace MicroApp.UsersService.Infrastructure.Persistence;

public class SqlCreateScript
{
    public const string Script = @"
CREATE TABLE IF NOT EXISTS ""Roles"" (
    ""Id"" uuid NOT NULL,
    ""Name"" character varying(100) NOT NULL,
    ""Permissions"" bigint NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL,
    CONSTRAINT ""PK_Roles"" PRIMARY KEY (""Id"")
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Roles_Name"" ON ""Roles"" (""Name"");

CREATE TABLE IF NOT EXISTS ""Users"" (
    ""Id"" uuid NOT NULL,
    ""Email"" character varying(256) NOT NULL,
    ""DisplayName"" character varying(200) NOT NULL,
    ""PasswordHash"" character varying(200) NOT NULL,
    ""IbanHash"" character varying(256) NULL,
    ""DobHash"" character varying(256) NULL,
    ""HashSalt"" character varying(64) NULL,
    ""RoleId"" uuid NOT NULL,
    ""OverridePermissions"" bigint NULL,
    ""VerificationStatus"" integer NOT NULL DEFAULT 0,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    ""IsDeleted"" boolean NOT NULL DEFAULT false,
    CONSTRAINT ""PK_Users"" PRIMARY KEY (""Id"")
);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
CREATE INDEX IF NOT EXISTS ""IX_Users_RoleId"" ON ""Users"" (""RoleId"");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Users_Roles_RoleId'
    ) THEN
        ALTER TABLE ""Users"" ADD CONSTRAINT ""FK_Users_Roles_RoleId""
        FOREIGN KEY (""RoleId"") REFERENCES ""Roles"" (""Id"") ON DELETE CASCADE;
    END IF;
END
$$;

-- Add VerificationStatus column if it doesn't exist (for existing databases)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'VerificationStatus'
    ) THEN
        ALTER TABLE ""Users"" ADD COLUMN ""VerificationStatus"" integer NOT NULL DEFAULT 0;
    END IF;
END
$$;

-- Add IsDeleted column if it doesn't exist (for existing databases)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'IsDeleted'
    ) THEN
        ALTER TABLE ""Users"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT false;
    END IF;
END
$$;

CREATE TABLE IF NOT EXISTS ""Verifications"" (
    ""Id"" uuid NOT NULL,
    ""Action"" integer NOT NULL,
    ""TargetId"" uuid NOT NULL,
    ""Status"" integer NOT NULL,
    ""Code"" character varying(10) NOT NULL,
    ""CreatedBy"" uuid NOT NULL,
    ""AssigneeUserId"" uuid NULL,
    ""CreatedAt"" timestamptz NOT NULL,
    ""DecidedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Verifications"" PRIMARY KEY (""Id"")
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Verifications_Users_AssigneeUserId'
    ) THEN
        ALTER TABLE ""Verifications""
        ADD CONSTRAINT ""FK_Verifications_Users_AssigneeUserId""
        FOREIGN KEY (""AssigneeUserId"") REFERENCES ""Users"" (""Id"") ON DELETE SET NULL;
    END IF;
END $$;
";
}
