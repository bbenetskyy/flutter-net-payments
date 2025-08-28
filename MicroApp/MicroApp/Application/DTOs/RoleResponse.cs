using MicroApp.Domain.Enums;

namespace MicroApp.Application.DTOs;

public record RoleResponse(
    Guid Id,
    string Name,
    UserPermissions Permissions,
    DateTime CreatedAt,
    int UsersCount);
