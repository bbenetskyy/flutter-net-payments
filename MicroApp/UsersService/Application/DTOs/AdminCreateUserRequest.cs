using System;

namespace AuthService.Application.DTOs;

public record AdminCreateUserRequest(
    string Email,
    string DisplayName,
    Guid? DesiredRoleId,
    string? Iban,
    DateOnly? DateOfBirth
);
