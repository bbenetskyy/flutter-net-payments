using Common.Domain.Enums;
using WalletService.Domain.Entities;

namespace WalletService.Application.DTOs.Response;

public sealed record LedgerEntryResponse(
    Guid Id,
    Guid WalletId,
    long AmountMinor,
    Currency Currency,
    LedgerEntryType Type,
    LedgerAccount Account,
    LedgerAccount CounterpartyAccount,
    string? Description,
    Guid CorrelationId,
    DateTime CreatedAt
);
