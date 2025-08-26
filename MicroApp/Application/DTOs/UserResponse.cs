using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record UserResponse(
    Guid Id, string Email, string DisplayName, 
    Guid RoleId, string RoleName, UserPermissions EffectivePermissions,
    bool HasIban, bool HasDateOfBirth, DateTime CreatedAt);
