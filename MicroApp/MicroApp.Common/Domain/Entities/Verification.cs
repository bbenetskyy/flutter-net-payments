using Common.Validation;

namespace Common.Domain.Entities;


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

