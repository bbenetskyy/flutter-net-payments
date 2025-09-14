using CardsService.Application.DTOs;
using CardsService.Presentation.Endpoints;
using Common.Validation;

namespace CardsService.Application.Validation;

public sealed class AssignCardRequestValidator : IValidator<AssignCardRequest>
{
    public ValidationResult Validate(AssignCardRequest input)
    {
        if (input.UserId == Guid.Empty)
            return ValidationResult.Fail("UserId is required");
        return ValidationResult.Ok();
    }
}
