using System.Security.Claims;
using Common.Validation;

namespace CardsService.Infrastructure.Persistence;

public interface ICardVerificationService
{
    Task<VerificationDto> CreateForAssignmentAsync(Guid cardId, ClaimsPrincipal user);
    Task<VerificationDto> CreateForPrintingAsync(Guid cardId, ClaimsPrincipal user);
    Task<VerificationDto> CreateForTerminationAsync(Guid cardId, ClaimsPrincipal user);
}
