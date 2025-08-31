using Common.Security;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record RoleResponse(
    Guid Id,
    string Name,
    UserPermissions Permissions,
    DateTime CreatedAt,
    int UsersCount);
