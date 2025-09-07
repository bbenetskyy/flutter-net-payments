using CardsService.Domain.Enums;
using CardsService.Presentation.Endpoints;
using Common.Validation;

namespace CardsService.Application.Validation;

public sealed class CreateCardRequestValidator : IValidator<CreateCardRequest>
{
    public ValidationResult Validate(CreateCardRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) && input.Type == CardType.Shared)
            return ValidationResult.Fail("Name is required");
        if (input.SingleTransactionLimit < 0)
            return ValidationResult.Fail("SingleTransactionLimit must be >= 0");
        if (input.MonthlyLimit < 0)
            return ValidationResult.Fail("MonthlyLimit must be >= 0");
        return ValidationResult.Ok();
    }
}
