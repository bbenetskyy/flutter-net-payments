using Common.Security;

namespace MicroApp.UsersService.Application.DTOs;

public record UpdateUserRequest(
    string? DisplayName,
    Guid? RoleId,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
