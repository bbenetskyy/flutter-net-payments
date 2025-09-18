using Common.Domain.Enums;

namespace WalletService.Application.DTOs.Response;

public sealed record TopUpIdempotentResponse(
    string Status,
    Guid CorrelationId
);

public sealed record TopUpAppliedResponse(
    string Status,
    Guid CorrelationId,
    Guid WalletId,
    Guid UserId,
    Currency Currency,
    long BalanceMinor
);

public sealed record ErrorResponse(
    string Error
);
