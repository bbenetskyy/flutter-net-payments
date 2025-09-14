namespace PaymentsService.Infrastructure.Persistence;

public class SqlCreateScript
{
    public const string Script = @"
CREATE TABLE IF NOT EXISTS ""Payments"" (
    ""Id"" uuid NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""BeneficiaryName"" character varying(200) NOT NULL,
    ""BeneficiaryAccount"" character varying(64) NOT NULL,
    ""FromAccount"" character varying(64) NOT NULL,
    ""Amount"" numeric(18,2) NOT NULL,
    ""Currency"" character varying(3) NOT NULL,
    ""FromCurrency"" character varying(3) NOT NULL DEFAULT 'EUR',
    ""BeneficiaryId"" uuid NULL,
    ""BeneficiaryAccountId"" uuid NULL,
    ""Details"" text NULL,
    ""Status"" integer NOT NULL,
    ""CreatedAt"" timestamptz NOT NULL,
    ""UpdatedAt"" timestamptz NULL,
    CONSTRAINT ""PK_Payments"" PRIMARY KEY (""Id"")
);
-- Ensure required columns exist for existing databases
ALTER TABLE ""Payments"" ADD COLUMN IF NOT EXISTS ""FromCurrency"" character varying(3) NOT NULL DEFAULT 'EUR';
ALTER TABLE ""Payments"" ADD COLUMN IF NOT EXISTS ""BeneficiaryId"" uuid NULL;
ALTER TABLE ""Payments"" ADD COLUMN IF NOT EXISTS ""BeneficiaryAccountId"" uuid NULL;

-- Create indexes (after ensuring columns exist)
CREATE INDEX IF NOT EXISTS ""IX_Payments_UserId"" ON ""Payments"" (""UserId"");
CREATE INDEX IF NOT EXISTS ""IX_Payments_BeneficiaryId"" ON ""Payments"" (""BeneficiaryId"");
CREATE INDEX IF NOT EXISTS ""IX_Payments_BeneficiaryAccountId"" ON ""Payments"" (""BeneficiaryAccountId"");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Payments_Users_UserId'
    ) THEN
        ALTER TABLE ""Payments""
        ADD CONSTRAINT ""FK_Payments_Users_UserId""
        FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Payments_Users_BeneficiaryId'
    ) THEN
        ALTER TABLE ""Payments""
        ADD CONSTRAINT ""FK_Payments_Users_BeneficiaryId""
        FOREIGN KEY (""BeneficiaryId"") REFERENCES ""Users"" (""Id"") ON DELETE SET NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Payments_Accounts_BeneficiaryAccountId'
    ) THEN
        ALTER TABLE ""Payments""
        ADD CONSTRAINT ""FK_Payments_Accounts_BeneficiaryAccountId""
        FOREIGN KEY (""BeneficiaryAccountId"") REFERENCES ""Accounts"" (""Id"") ON DELETE SET NULL;
    END IF;
END $$;
";
}
