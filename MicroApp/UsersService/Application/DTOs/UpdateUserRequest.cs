using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record UpdateUserRequest(
    string? DisplayName,
    Guid? RoleId,
    string? Iban,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
