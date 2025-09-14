using CardsService.Domain.Enums;

namespace CardsService.Application.DTOs;

public record UpdateCardRequest(
    CardType? Type,
    string? Name,
    decimal? SingleTransactionLimit,
    decimal? MonthlyLimit,
    CardOptions? Options,
    bool? Printed,
    Guid? AssignedUserId
)
{
    public bool AssignedUserIdSet => AssignedUserId.HasValue;
}