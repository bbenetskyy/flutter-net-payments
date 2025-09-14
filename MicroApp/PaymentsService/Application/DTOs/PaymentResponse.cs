using Common.Domain.Enums;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.DTOs;

public record PaymentResponse(
    Guid Id,
    Guid UserId,
    string BeneficiaryName,
    string BeneficiaryAccount,
    Guid? BeneficiaryId,
    Guid? BeneficiaryAccountId,
    string FromAccount,
    decimal Amount,
    Currency Currency,
    Currency FromCurrency,
    string? Details,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
