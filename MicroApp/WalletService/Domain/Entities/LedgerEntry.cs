using System.ComponentModel.DataAnnotations;
using Common.Domain.Enums;

namespace WalletService.Domain.Entities;

public enum LedgerEntryType
{
    Credit = 1,
    Debit = 2
}

public enum LedgerAccount
{
    Cash = 1,
    Clearing = 2
}

public class LedgerEntry
{
    [Key]
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public long AmountMinor { get; set; } // store in minor units
    public Currency Currency { get; set; } = Currency.EUR; // limited set, default EUR
    public LedgerEntryType Type { get; set; }
    public LedgerAccount Account { get; set; } = LedgerAccount.Cash; // double-entry account
    public LedgerAccount CounterpartyAccount { get; set; } = LedgerAccount.Clearing;
    public string? Description { get; set; }
    public Guid CorrelationId { get; set; } // idempotency key (e.g., intentId)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
