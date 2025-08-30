namespace MicroApp.UsersService.Application.DTOs;

public record AdminCreateUserRequest(
    string Email,
    string DisplayName,
    Guid? DesiredRoleId,
    string? Iban,
    DateOnly? DateOfBirth
);
