namespace Common.Validation;

public enum VerificationStatus
{
    Pending,
    Rejected,
    Completed
}

public enum VerificationAction
{
    NewUserCreated,
    UserAssignedToCard,
    CardPrinting,
    CardTermination,
    PaymentCreated,
    PaymentReverted
}

public record CreateVerificationRequest(
    VerificationAction Action,
    Guid TargetId
);

public record VerificationDecisionRequest(
    string Code,
    bool Accept
);

public record VerificationDto(
    Guid Id,
    VerificationAction Action,
    Guid TargetId,
    VerificationStatus Status,
    string Code,
    Guid CreatedBy,
    Guid? AssigneeUserId,
    DateTime CreatedAt,
    DateTime? DecidedAt
);
