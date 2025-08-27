using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record RoleResponse(
    Guid Id,
    string Name,
    UserPermissions Permissions,
    DateTime CreatedAt,
    int UsersCount);
