using AuthService.Domain.Enums;
using Common.Validation;

namespace AuthService.Application.DTOs;

public record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    Guid RoleId,
    string RoleName,
    UserPermissions EffectivePermissions,
    VerificationStatus VerificationStatus,
    string? Iban,
    string? DobHash,
    DateTime CreatedAt);