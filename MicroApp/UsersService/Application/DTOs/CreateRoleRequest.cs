using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record CreateRoleRequest(
    string Name,
    UserPermissions Permissions);
