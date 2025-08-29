using System;

namespace AuthService.Application.DTOs;

public record AdminAssignRoleForVerificationRequest(
    Guid? DesiredRoleId
);
