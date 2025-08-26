using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string DisplayName { get; set; } = default!;

    // Хешовані значення з сіллю/перцем
    public string? IbanHash { get; set; }          // hex/base64 хеш IBAN
    public string? DobHash { get; set; }           // YYYY-MM-DD -> хеш
    public string? HashSalt { get; set; }          // унікальна сіль на користувача

    // Ролі/права
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
    public UserPermissions? OverridePermissions { get; set; } // опціонально: явне перекриття

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
