using MicroApp.Domain.Enums;

namespace MicroApp.Application.DTOs;

public record CreateRoleRequest(
    string Name,
    UserPermissions Permissions);
