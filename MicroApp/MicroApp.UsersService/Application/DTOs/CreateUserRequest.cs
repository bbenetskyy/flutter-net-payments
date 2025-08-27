using AuthService.Domain.Enums;

namespace AuthService.Application.DTOs;

public record CreateUserRequest(
    string Email,
    string DisplayName,
    Guid RoleId,
    string? Iban,            // plain з форми → сервер хешує
    DateOnly? DateOfBirth,   // з форми → "YYYY-MM-DD" → хеш
    UserPermissions? OverridePermissions);
