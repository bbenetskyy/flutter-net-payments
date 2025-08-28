using MicroApp.Domain.Enums;

namespace MicroApp.Application.DTOs;

public record UpdateUserRequest(
    string? DisplayName,
    Guid? RoleId,
    string? Iban,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
