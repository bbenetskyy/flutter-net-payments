using CardsService.Presentation.Endpoints;
using Common.Validation;

namespace CardsService.Application.Validation;

public sealed class UpdateCardRequestValidator : IValidator<UpdateCardRequest>
{
    public ValidationResult Validate(UpdateCardRequest input)
    {
        if (input.Name is not null && string.IsNullOrWhiteSpace(input.Name))
            return ValidationResult.Fail("Name cannot be blank");

        if (input.SingleTransactionLimit is decimal stl && stl < 0)
            return ValidationResult.Fail("SingleTransactionLimit must be >= 0");
        if (input.MonthlyLimit is decimal ml && ml < 0)
            return ValidationResult.Fail("MonthlyLimit must be >= 0");

        if (input.AssignedUserIdSet)
            return ValidationResult.Fail("Assigned user cannot be set or changed via update endpoint.");

        return ValidationResult.Ok();
    }
}
