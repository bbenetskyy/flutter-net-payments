using Common.Security;

namespace MicroApp.UsersService.Application.DTOs;

public record CreateRoleRequest(
    string Name,
    UserPermissions Permissions);
