using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record UpdateUserRequest(
    string? DisplayName,
    Guid? RoleId,
    string? Iban,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
