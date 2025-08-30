using Common.Validation;
using MicroApp.UsersService.Application.DTOs;

namespace MicroApp.UsersService.Application.Validation;

public sealed class UpdateUserRequestValidator : IValidator<UpdateUserRequest>
{
    public ValidationResult Validate(UpdateUserRequest input)
    {
        if (input.DisplayName is not null && string.IsNullOrWhiteSpace(input.DisplayName))
            return ValidationResult.Fail("Display name cannot be blank");

        if (input.Iban is not null)
        {
            if (string.IsNullOrWhiteSpace(input.Iban))
                return ValidationResult.Fail("IBAN cannot be blank");
            if (!Common.Validation.Validation.IsValidIban(input.Iban))
                return ValidationResult.Fail("Invalid IBAN");
        }

        return ValidationResult.Ok();
    }
}
