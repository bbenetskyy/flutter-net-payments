namespace WalletService.Infrastructure.Persistence;

public static class SqlCreateScript
{
    public const string Script = @"
CREATE TABLE IF NOT EXISTS ""Wallets"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ""PK_Wallets"" PRIMARY KEY (""Id"")
);

CREATE UNIQUE INDEX IF NOT EXISTS ""UX_Wallets_UserId""
ON ""Wallets"" (""UserId"");

CREATE TABLE IF NOT EXISTS ""LedgerEntries"" (
    ""Id"" uuid NOT NULL,
    ""WalletId"" uuid NOT NULL,
    ""AmountMinor"" bigint NOT NULL,
    ""Currency"" character varying(3) NOT NULL DEFAULT 'EUR',
    ""Type"" integer NOT NULL,
    ""Account"" integer NOT NULL DEFAULT 1,
    ""CounterpartyAccount"" integer NOT NULL DEFAULT 2,
    ""Description"" text NULL,
    ""CorrelationId"" uuid NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ""PK_LedgerEntries"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_LedgerEntries_Wallets_WalletId""
        FOREIGN KEY (""WalletId"") REFERENCES ""Wallets"" (""Id"") ON DELETE CASCADE
);

-- Accounts table for multi-IBAN per user
CREATE TABLE IF NOT EXISTS ""Accounts"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""Iban"" character varying(64) NOT NULL,
    ""Currency"" character varying(3) NOT NULL DEFAULT 'EUR',
    ""CreatedAt"" timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ""PK_Accounts"" PRIMARY KEY (""Id"")
);
-- Enforce global uniqueness of IBAN across all users
CREATE UNIQUE INDEX IF NOT EXISTS ""UX_Accounts_Iban"" ON ""Accounts"" (""Iban"");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Wallets_Users_UserId'
    ) THEN
        ALTER TABLE ""Wallets""
        ADD CONSTRAINT ""FK_Wallets_Users_UserId""
        FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Accounts_Users_UserId'
    ) THEN
        ALTER TABLE ""Accounts""
        ADD CONSTRAINT ""FK_Accounts_Users_UserId""
        FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE;
    END IF;
END $$;
";
}
