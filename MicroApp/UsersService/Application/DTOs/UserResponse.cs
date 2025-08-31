using Common.Security;
using Common.Validation;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    Guid RoleId,
    string RoleName,
    UserPermissions EffectivePermissions,
    VerificationStatus VerificationStatus,
    string? DobHash,
    DateTime CreatedAt);