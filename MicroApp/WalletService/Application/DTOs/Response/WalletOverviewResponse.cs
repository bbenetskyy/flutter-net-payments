using Common.Domain.Enums;

namespace WalletService.Application.DTOs.Response;

public sealed record WalletOverviewResponse(
    Guid WalletId,
    Guid UserId,
    IReadOnlyList<WalletBalanceItem> Balances
);

public sealed record WalletBalanceItem(
    Currency Currency,
    long BalanceMinor
);
