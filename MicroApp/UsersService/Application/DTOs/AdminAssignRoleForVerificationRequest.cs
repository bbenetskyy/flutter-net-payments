namespace MicroApp.UsersService.Application.DTOs;

public record AdminAssignRoleForVerificationRequest(
    Guid? DesiredRoleId
);
