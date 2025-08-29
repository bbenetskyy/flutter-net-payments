namespace AuthService.Application.DTOs;

public record UsersVerificationDecisionRequest(
    Guid VerificationId,
    string Code,
    bool Accept,
    string? NewPassword
);