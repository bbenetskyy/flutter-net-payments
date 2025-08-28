using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using CardsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CardsService.Presentation.Endpoints;

public static class CardsEndpoints
{
    public static void MapCardsEndpoints(this IEndpointRouteBuilder app)
    {
        // Create a card
        app.MapPost("/cards", async (CreateCardRequest req, CardsDb db, Common.Validation.IValidator<CreateCardRequest> validator) =>
        {
            var vr = validator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Type = req.Type,
                Name = req.Name.Trim(),
                SingleTransactionLimit = req.SingleTransactionLimit,
                MonthlyLimit = req.MonthlyLimit,
                Options = req.Options,
                Printed = false,
            };
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            return Results.Created($"/cards/{card.Id}", ToDto(card));
        }).RequireAuthorization();

        // Assign to user
        app.MapPost("/cards/{id:guid}/assign", async (Guid id, AssignCardRequest req, CardsDb db, Common.Validation.IValidator<AssignCardRequest> reqValidator, Common.Validation.IValidator<CardsService.Application.Validation.AssignCardOperation> opValidator) =>
        {
            var vr = reqValidator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var card = await db.Cards.FindAsync(id);
            if (card is null) return Results.NotFound();

            var op = new CardsService.Application.Validation.AssignCardOperation(card, req);
            var vop = opValidator.Validate(op);
            if (!vop.IsValid) return Results.BadRequest(vop.Error);

            card.AssignedUserId = req.UserId;
            card.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(card));
        }).RequireAuthorization();

        // Update/manage card
        app.MapPut("/cards/{id:guid}", async (Guid id, UpdateCardRequest req, CardsDb db, Common.Validation.IValidator<UpdateCardRequest> reqValidator, Common.Validation.IValidator<CardsService.Application.Validation.UpdateCardOperation> opValidator) =>
        {
            var card = await db.Cards.FindAsync(id);
            if (card is null) return Results.NotFound();

            var vr = reqValidator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var op = new CardsService.Application.Validation.UpdateCardOperation(card, req);
            var vop = opValidator.Validate(op);
            if (!vop.IsValid) return Results.BadRequest(vop.Error);

            if (!string.IsNullOrWhiteSpace(req.Name)) card.Name = req.Name.Trim();
            if (req.Type is CardType ct) card.Type = ct; // Type can be changed unless other rules apply
            if (req.SingleTransactionLimit is decimal stl) card.SingleTransactionLimit = stl;
            if (req.MonthlyLimit is decimal ml) card.MonthlyLimit = ml;
            if (req.Printed is bool printed) card.Printed = printed;
            if (req.Options is CardOptions opts) card.Options = opts;

            card.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(card));
        }).RequireAuthorization();

        // Delete card
        app.MapDelete("/cards/{id:guid}", async (Guid id, CardsDb db) =>
        {
            var card = await db.Cards.FindAsync(id);
            if (card is null) return Results.NotFound();
            db.Cards.Remove(card);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // Optional: Get by id for convenience when testing
        app.MapGet("/cards/{id:guid}", async (Guid id, CardsDb db) =>
        {
            var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == id);
            return card is null ? Results.NotFound() : Results.Ok(ToDto(card));
        }).RequireAuthorization();
    }

    private static string? ValidateCreate(CreateCardRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return "Name is required";
        if (req.SingleTransactionLimit < 0) return "SingleTransactionLimit must be >= 0";
        if (req.MonthlyLimit < 0) return "MonthlyLimit must be >= 0";
        return null;
    }

    private static CardDto ToDto(Card c) => new(
        c.Id, c.Type, c.Name, c.SingleTransactionLimit, c.MonthlyLimit, c.AssignedUserId, c.Options,
        c.Printed, c.CreatedAt, c.UpdatedAt);
}

public record CreateCardRequest(
    CardType Type,
    string Name,
    decimal SingleTransactionLimit,
    decimal MonthlyLimit,
    CardOptions Options);

public record AssignCardRequest(Guid UserId);

public record UpdateCardRequest(
    CardType? Type,
    string? Name,
    decimal? SingleTransactionLimit,
    decimal? MonthlyLimit,
    CardOptions? Options,
    bool? Printed,
    bool AssignedUserIdSet,
    Guid? AssignedUserId
);

public record CardDto(
    Guid Id,
    CardType Type,
    string Name,
    decimal SingleTransactionLimit,
    decimal MonthlyLimit,
    Guid? AssignedUserId,
    CardOptions Options,
    bool Printed,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
