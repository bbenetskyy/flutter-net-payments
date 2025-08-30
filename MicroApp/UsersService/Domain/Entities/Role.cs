using Common.Validation;
using MicroApp.UsersService.Domain.Enums;

namespace MicroApp.UsersService.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;       // наприклад: "CEO", "CFO", "CTO"
    public UserPermissions Permissions { get; set; }   // бітова маска прав
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
}


public sealed class Verification
{
    public Guid Id { get; set; }
    public VerificationAction Action { get; set; }
    public Guid TargetId { get; set; }
    public VerificationStatus Status { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
}

