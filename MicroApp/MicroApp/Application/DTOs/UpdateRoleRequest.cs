using MicroApp.Domain.Enums;

namespace MicroApp.Application.DTOs;

public record UpdateRoleRequest(
    string? Name,
    UserPermissions? Permissions);
