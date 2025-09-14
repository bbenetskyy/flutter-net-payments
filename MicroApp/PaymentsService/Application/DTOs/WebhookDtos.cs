using Common.Domain.Enums;

namespace PaymentsService.Application.DTOs;

public class WalletEvent
{
    public Guid IntentId { get; set; }
    public Guid UserId { get; set; } // Payer (source)
    public Guid BeneficiaryId { get; set; } // Payee (destination); Guid.Empty if not applicable
    public long AmountMinor { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public PaymentEventType EventType { get; set; } = PaymentEventType.PaymentCaptured;
    public string? Description { get; set; }
}
