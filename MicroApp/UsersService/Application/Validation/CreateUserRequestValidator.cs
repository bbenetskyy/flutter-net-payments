using Common.Validation;
using MicroApp.UsersService.Application.DTOs;

namespace MicroApp.UsersService.Application.Validation;

public sealed class CreateUserRequestValidator : IValidator<CreateUserRequest>
{
    public ValidationResult Validate(CreateUserRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
            return ValidationResult.Fail("Email is required");
        if (!Common.Validation.Validation.IsValidEmail(input.Email))
            return ValidationResult.Fail("Invalid email format");
        if (string.IsNullOrWhiteSpace(input.DisplayName))
            return ValidationResult.Fail("Display name is required");

        return ValidationResult.Ok();
    }
}
