using Common.Domain.Enums;

namespace PaymentsService.Application.DTOs;

public record CreatePaymentRequest(
    string BeneficiaryName,
    string BeneficiaryAccount,
    string FromAccount,
    decimal Amount,
    Currency? Currency,
    string? Details
);