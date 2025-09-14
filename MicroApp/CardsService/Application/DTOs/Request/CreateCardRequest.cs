using CardsService.Domain.Enums;

namespace CardsService.Application.DTOs;

public record CreateCardRequest(
    CardType Type,
    string? Name,
    decimal SingleTransactionLimit,
    decimal MonthlyLimit);

