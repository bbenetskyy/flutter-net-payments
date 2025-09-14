using CardsService.Domain.Enums;

namespace CardsService.Application.DTOs;

public record CardResponse(
    Guid Id,
    CardType Type,
    string Name,
    decimal SingleTransactionLimit,
    decimal MonthlyLimit,
    Guid? AssignedUserId,
    CardOptions Options,
    bool Printed,
    bool Terminated,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
