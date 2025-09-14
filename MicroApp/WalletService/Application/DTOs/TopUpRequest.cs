namespace WalletService.Application.DTOs;

public sealed record TopUpRequest(
    long AmountMinor, 
    Common.Domain.Enums.Currency Currency, 
    Guid? CorrelationId, 
    string? Description);

