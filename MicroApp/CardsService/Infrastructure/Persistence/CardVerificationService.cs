using System.Security.Claims;
using Common.Validation;
using Microsoft.EntityFrameworkCore;

namespace CardsService.Infrastructure.Persistence;

public sealed class CardVerificationService : ICardVerificationService
{
    private readonly CardsDb _db;
    private readonly IVerificationStore _store;

    public CardVerificationService(CardsDb db, IVerificationStore store)
    {
        _db = db;
        _store = store;
    }

    public async Task<VerificationDto> CreateForAssignmentAsync(Guid cardId, ClaimsPrincipal user)
        => await CreateAsync(cardId, user, VerificationAction.UserAssignedToCard, requireAssignee:true);

    public async Task<VerificationDto> CreateForPrintingAsync(Guid cardId, ClaimsPrincipal user)
        => await CreateAsync(cardId, user, VerificationAction.CardPrinting, requireAssignee:false);

    public async Task<VerificationDto> CreateForTerminationAsync(Guid cardId, ClaimsPrincipal user)
        => await CreateAsync(cardId, user, VerificationAction.CardTermination, requireAssignee:false);

    private async Task<VerificationDto> CreateAsync(Guid cardId, ClaimsPrincipal user,
        VerificationAction action, bool requireAssignee)
    {
        var card = await _db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cardId);
        if (card is null) throw new InvalidOperationException("Card not found");

        var createdBy = GetUserId(user) ?? throw new UnauthorizedAccessException("No user id");
        Guid? assignee = card.AssignedUserId;

        if (action == VerificationAction.UserAssignedToCard && requireAssignee && assignee is null)
            throw new InvalidOperationException("Card has no assigned user to accept");

        var v = await _store.Create(action, cardId, createdBy, assignee);
        return v;
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}