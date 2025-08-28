using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record UpdateRoleRequest(
    string? Name,
    UserPermissions? Permissions);
