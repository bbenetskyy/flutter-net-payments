using Common.Security;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record UpdateRoleRequest(
    string? Name,
    UserPermissions? Permissions);
