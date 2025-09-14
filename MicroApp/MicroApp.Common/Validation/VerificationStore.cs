using Common.Domain.Entities;
using Common.Infrastucture.Persistence;

namespace Common.Validation;

public sealed class VerificationStore(VerificationsDb db) : IVerificationStore
{
    private readonly Random rng = new();

    public async Task<VerificationResponse> Create(VerificationAction action, Guid targetId, Guid createdBy, Guid? assignee)
    {
        var code = rng.Next(100000, 1000000).ToString();
        var verification = new Verification
        {
            Id = Guid.NewGuid(),
            Action = action,
            TargetId = targetId,
            Status = VerificationStatus.Pending,
            Code = code,
            CreatedBy = createdBy,
            AssigneeUserId = assignee,
            CreatedAt = DateTime.UtcNow
        };
        db.Verifications.Add(verification);
        await db.SaveChangesAsync();
        return ToDto(verification);
    }

    public async Task<VerificationResponse?> Get(Guid id)
    {
        var verification = await db.Verifications.FindAsync(id);
        return verification is null ? null : ToDto(verification);
    }

    public async Task<VerificationResponse?> Decide(Guid id, VerificationStatus status)
    {
        var verification = await db.Verifications.FindAsync(id);
        if (verification is null) return null;

        verification.Status = status;
        verification.DecidedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ToDto(verification);
    }

    private static VerificationResponse ToDto(Verification v) => new(v.Id, v.Action, v.TargetId, v.Status, v.Code, v.CreatedBy, v.AssigneeUserId, v.CreatedAt, v.DecidedAt);
}
