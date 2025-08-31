using Common.Domain.Enums;

namespace PaymentsService.Presentation.Endpoints;

public record ProviderWebhook(
    Guid IntentId, 
    Guid UserId, 
    Guid BeneficiaryId,
    decimal Amount, 
    Currency Currency, 
    string Type, 
    string? Description);

public class WalletEvent
{
    public Guid IntentId { get; set; }
    public Guid UserId { get; set; } // Payer (source)
    public Guid BeneficiaryId { get; set; } // Payee (destination); Guid.Empty if not applicable
    public long AmountMinor { get; set; }
    public Common.Domain.Enums.Currency Currency { get; set; } = Common.Domain.Enums.Currency.EUR;
    public Common.Domain.Enums.PaymentEventType EventType { get; set; } = Common.Domain.Enums.PaymentEventType.PaymentCaptured;
    public string? Description { get; set; }
}
