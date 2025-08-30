using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Application.DTOs;

public record CreateUserRequest(
    string Email,
    string DisplayName,
    Guid RoleId,
    string? Iban,            // plain з форми → сервер хешує
    DateOnly? DateOfBirth,   // з форми → "YYYY-MM-DD" → хеш
    UserPermissions? OverridePermissions);
