using Common.Domain.Enums;

namespace WalletService.Domain.Events;

// Minimal event DTO propagated from PaymentsService webhooks (no PII)
public record PaymentEvent(Guid IntentId, Guid UserId, long AmountMinor, Currency Currency, PaymentEventType EventType, string? Description);
