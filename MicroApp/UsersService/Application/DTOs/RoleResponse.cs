using Common.Security;

namespace MicroApp.UsersService.Application.DTOs;

public record RoleResponse(
    Guid Id,
    string Name,
    UserPermissions Permissions,
    DateTime CreatedAt,
    int UsersCount);
