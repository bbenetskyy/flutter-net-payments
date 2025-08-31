using Common.Domain.Enums;

namespace PaymentsService.Domain.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string BeneficiaryName { get; set; } = string.Empty;
    public string BeneficiaryAccount { get; set; } = string.Empty; // IBAN
    public Guid? BeneficiaryId { get; set; }
    public Guid? BeneficiaryAccountId { get; set; }
    public string FromAccount { get; set; } = string.Empty; // IBAN

    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.EUR; // destination/payment currency
    public Currency FromCurrency { get; set; } = Currency.EUR; // source account currency

    public string? Details { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
