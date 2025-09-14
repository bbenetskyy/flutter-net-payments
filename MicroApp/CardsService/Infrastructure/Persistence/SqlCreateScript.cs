namespace CardsService.Infrastructure.Persistence;

public class SqlCreateScript
{
    public const string Script = @"
CREATE TABLE IF NOT EXISTS ""Cards"" (
    ""Id"" uuid NOT NULL,
    ""Type"" integer NOT NULL,
    ""Name"" character varying(200) NOT NULL,
    ""SingleTransactionLimit"" numeric(18,2) NOT NULL,
    ""MonthlyLimit"" numeric(18,2) NOT NULL,
    ""AssignedUserId"" uuid NULL,
    ""Options"" bigint NOT NULL,
    ""Printed"" boolean NOT NULL DEFAULT FALSE,
    ""Terminated"" boolean NOT NULL DEFAULT FALSE,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Cards"" PRIMARY KEY (""Id"")
);
CREATE INDEX IF NOT EXISTS ""IX_Cards_AssignedUserId"" ON ""Cards"" (""AssignedUserId"");

-- Add column if missing, ensure default + NOT NULL
ALTER TABLE IF EXISTS ""Cards""
    ADD COLUMN IF NOT EXISTS ""Terminated"" boolean DEFAULT FALSE;
UPDATE ""Cards"" SET ""Terminated"" = FALSE WHERE ""Terminated"" IS NULL;
ALTER TABLE IF EXISTS ""Cards"" ALTER COLUMN ""Terminated"" SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Cards_Users_AssignedUserId'
    ) THEN
        ALTER TABLE ""Cards""
        ADD CONSTRAINT ""FK_Cards_Users_AssignedUserId""
        FOREIGN KEY (""AssignedUserId"") REFERENCES ""Users"" (""Id"") ON DELETE SET NULL;
    END IF;
END $$;
";
}
