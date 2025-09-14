using CardsService.Application.DTOs;
using CardsService.Domain.Entities;
using CardsService.Presentation.Endpoints;
using Common.Validation;

namespace CardsService.Application.Validation;

public readonly record struct UpdateCardOperation(Card Card, UpdateCardRequest Request);
public sealed class UpdateCardOperationValidator : IValidator<UpdateCardOperation>
{
    public ValidationResult Validate(UpdateCardOperation input)
    {
        var card = input.Card;
        var req = input.Request;

        // Determine printed state after this update
        var effectivePrinted = req.Printed ?? card.Printed;

        // If options requested to change while printed, forbid
        if (req.Options is not null && effectivePrinted)
            return ValidationResult.Fail("Card options cannot be changed after the card is printed.");

        // Forbid any attempt to touch assignment via update
        if (req.AssignedUserIdSet)
            return ValidationResult.Fail("Assigned user cannot be set or changed via update endpoint.");

        return ValidationResult.Ok();
    }
}

public readonly record struct AssignCardOperation(Card Card, AssignCardRequest Request);
public sealed class AssignCardOperationValidator : IValidator<AssignCardOperation>
{
    public ValidationResult Validate(AssignCardOperation input)
    {
        if (input.Card.AssignedUserId is not null)
            return ValidationResult.Fail("Card is already assigned and cannot be reassigned.");
        return ValidationResult.Ok();
    }
}
