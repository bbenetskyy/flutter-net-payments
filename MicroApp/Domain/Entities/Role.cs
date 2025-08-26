using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;       // наприклад: "CEO", "Manager", "Viewer"
    public UserPermissions Permissions { get; set; }   // бітова маска прав
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
}
