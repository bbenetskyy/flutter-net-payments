using Common.Security;

namespace MicroApp.UsersService.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;       // наприклад: "CEO", "CFO", "CTO"
    public UserPermissions Permissions { get; set; }   // бітова маска прав
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
}


