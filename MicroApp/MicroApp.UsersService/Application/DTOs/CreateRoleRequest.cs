using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record CreateRoleRequest(
    string Name,
    UserPermissions Permissions);
