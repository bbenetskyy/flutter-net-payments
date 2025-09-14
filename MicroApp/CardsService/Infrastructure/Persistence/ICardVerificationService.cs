using System.Security.Claims;
using Common.Validation;

namespace CardsService.Infrastructure.Persistence;

public interface ICardVerificationService
{
    Task<VerificationResponse> CreateForAssignmentAsync(Guid cardId, ClaimsPrincipal user);
    Task<VerificationResponse> CreateForPrintingAsync(Guid cardId, ClaimsPrincipal user);
    Task<VerificationResponse> CreateForTerminationAsync(Guid cardId, ClaimsPrincipal user);
}
