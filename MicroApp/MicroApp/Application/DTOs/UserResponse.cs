using MicroApp.Domain.Enums;

namespace MicroApp.Application.DTOs;

public record UserResponse(
    Guid Id, string Email, string DisplayName, 
    Guid RoleId, string RoleName, UserPermissions EffectivePermissions,
    string? IbanHash, string? DobHash, DateTime CreatedAt);
