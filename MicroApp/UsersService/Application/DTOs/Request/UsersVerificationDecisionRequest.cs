namespace MicroApp.UsersService.Application.DTOs;

public record UsersVerificationDecisionRequest(
    Guid TargetId,
    string Code,
    bool Accept,
    string? NewPassword
);