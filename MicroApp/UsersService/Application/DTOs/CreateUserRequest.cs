using Common.Security;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record CreateUserRequest(
    string Email,
    string DisplayName,
    Guid RoleId,
    DateOnly? DateOfBirth,
    UserPermissions? OverridePermissions);
