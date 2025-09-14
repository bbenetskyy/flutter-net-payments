using Common.Security;

namespace MicroApp.UsersService.Application.DTOs;

public record UpdateRoleRequest(
    string? Name,
    UserPermissions? Permissions);
