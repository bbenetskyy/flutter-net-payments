using MicroApp.Application.DTOs;
using MicroApp.Validation;

namespace MicroApp.Application.Validation;

public sealed class CreateUserRequestValidator : IValidator<CreateUserRequest>
{
    public ValidationResult Validate(CreateUserRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
            return ValidationResult.Fail("Email is required");
        if (!Validation.IsValidEmail(input.Email))
            return ValidationResult.Fail("Invalid email format");
        if (string.IsNullOrWhiteSpace(input.DisplayName))
            return ValidationResult.Fail("Display name is required");

        if (!string.IsNullOrWhiteSpace(input.Iban) && !Validation.IsValidIban(input.Iban))
            return ValidationResult.Fail("Invalid IBAN");

        return ValidationResult.Ok();
    }
}
