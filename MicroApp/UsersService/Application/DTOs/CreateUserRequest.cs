using Common.Security;

namespace MicroApp.UsersService.Application.DTOs;

public record CreateUserRequest(
    string Email,
    string DisplayName,
    Guid RoleId,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
