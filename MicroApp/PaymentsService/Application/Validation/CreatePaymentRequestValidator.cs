using Common.Validation;
using Common.Security;
using PaymentsService.Application.DTOs;

namespace PaymentsService.Application.Validation;

public sealed class CreatePaymentRequestValidator : IValidator<CreatePaymentRequest>
{
    public ValidationResult Validate(CreatePaymentRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.BeneficiaryName))
            return ValidationResult.Fail("Beneficiary name is required");
        if (string.IsNullOrWhiteSpace(input.BeneficiaryAccount))
            return ValidationResult.Fail("Beneficiary account (IBAN) is required");
        if (string.IsNullOrWhiteSpace(input.FromAccount))
            return ValidationResult.Fail("From account (IBAN) is required");
        if (input.Amount <= 0)
            return ValidationResult.Fail("Amount must be greater than 0");
        if (input.BeneficiaryName.Length > 200)
            return ValidationResult.Fail("Beneficiary name too long");
        if (input.BeneficiaryAccount.Length > 64)
            return ValidationResult.Fail("Beneficiary account too long");
        if (input.FromAccount.Length > 64)
            return ValidationResult.Fail("From account too long");

        // IBAN format validation (normalized)
        var benIban = Hashing.NormalizeIban(input.BeneficiaryAccount);
        var fromIban = Hashing.NormalizeIban(input.FromAccount);
        if (!Common.Validation.Validation.IsValidIban(benIban))
            return ValidationResult.Fail("Invalid beneficiary IBAN format");
        if (!Common.Validation.Validation.IsValidIban(fromIban))
            return ValidationResult.Fail("Invalid from-account IBAN format");

        return ValidationResult.Ok();
    }
}
