using Common.Validation;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string DisplayName { get; set; } = default!;

    // Password (BCrypt hash)
    public string PasswordHash { get; set; } = default!;

    // Хешовані значення з сіллю/перцем
    public string? Iban { get; set; }          // hex/base64 хеш IBAN
    public string? DobHash { get; set; }           // YYYY-MM-DD -> хеш
    public string? HashSalt { get; set; }          // унікальна сіль на користувача

    // Ролі/права
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public UserPermissions? OverridePermissions { get; set; } // опціонально: явне перекриття

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public UserPermissions EffectivePermissions => OverridePermissions ?? Role?.Permissions ?? UserPermissions.None;
}
