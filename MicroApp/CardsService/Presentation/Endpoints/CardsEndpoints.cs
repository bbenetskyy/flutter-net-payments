using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using CardsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CardsService.Presentation.Security;

using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.Validation;

namespace CardsService.Presentation.Endpoints;

public static class CardsEndpoints
{
    public static void MapCardsEndpoints(this IEndpointRouteBuilder app)
    {
        var store = new VerificationStore();

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

        // Get all cards (requires ViewCards permission)
        app.MapGet("/cards", async (CardsDb db) =>
        {
            var items = await db.Cards.AsNoTracking().ToListAsync();
            return Results.Ok(items.Select(ToDto));
        }).RequirePermission(UserPermissions.ViewCards);

        // Create a verification for card actions
        app.MapPost("/cards/verifications", async (HttpContext http, CreateVerificationRequest req, CardsDb db) =>
        {
            if (req.Action is not (VerificationAction.UserAssignedToCard or VerificationAction.CardPrinting or VerificationAction.CardTermination))
                return Results.BadRequest("Unsupported action for CardsService");

            var card = await db.Cards.FindAsync(req.TargetId);
            if (card is null) return Results.NotFound("Card not found");

            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            Guid? assignee = card.AssignedUserId;
            if (req.Action == VerificationAction.UserAssignedToCard && assignee is null)
                return Results.BadRequest("Card has no assigned user to accept");

            var v = store.Create(req.Action, req.TargetId, uid.Value, assignee);
            return Results.Created($"/cards/verifications/{v.Id}", v);
        }).RequirePermission(UserPermissions.ViewCards);

        // Decide
        app.MapPost("/cards/verifications/{id:guid}/decision", async (HttpContext http, Guid id, VerificationDecisionRequest req, CardsDb db) =>
        {
            if (id != req.VerificationId) return Results.BadRequest("Mismatched verification id");
            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            var v = store.Get(id);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();

            var card = await db.Cards.FindAsync(v.TargetId);
            if (card is null) return Results.NotFound("Card not found");

            // Permissions via /me
            var me = await GetMeAsync(http);
            if (me is null) return Results.Unauthorized();
            var perms = (CardsService.Presentation.Security.UserPermissions)me.EffectivePermissions;

            bool allowed = false;
            if (v.Action == VerificationAction.UserAssignedToCard)
                allowed = card.AssignedUserId == uid;
            else if (v.Action == VerificationAction.CardPrinting)
                allowed = (card.AssignedUserId == uid) || ((perms & UserPermissions.ManageCompanyCards) == UserPermissions.ManageCompanyCards);
            else if (v.Action == VerificationAction.CardTermination)
                allowed = card.AssignedUserId == uid;

            if (!allowed) return Results.Forbid();

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = store.Decide(id, newStatus);
            return Results.Ok(v);
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

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var id)) return id;
        return null;
    }

    private static async Task<MeResponse?> GetMeAsync(HttpContext http)
    {
        var factory = http.RequestServices.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("users");
        var authHeader = http.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authHeader))
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

        using var res = await client.GetAsync("/me");
        if (!res.IsSuccessStatusCode) return null;
        var me = await res.Content.ReadFromJsonAsync<MeResponse>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return me;
    }
}

internal sealed class VerificationStore
{
    private readonly Dictionary<Guid, VerificationDto> _items = new();
    private readonly Random _rng = new();

    public VerificationDto Create(VerificationAction action, Guid targetId, Guid createdBy, Guid? assignee)
    {
        var id = Guid.NewGuid();
        var code = _rng.Next(100000, 1000000).ToString();
        var v = new VerificationDto(id, action, targetId, VerificationStatus.Pending, code, createdBy, assignee, DateTime.UtcNow, null);
        _items[id] = v;
        return v;
    }

    public VerificationDto? Get(Guid id) => _items.TryGetValue(id, out var v) ? v : null;

    public VerificationDto Decide(Guid id, VerificationStatus status)
    {
        var v = _items[id];
        var decided = v with { Status = status, DecidedAt = DateTime.UtcNow };
        _items[id] = decided;
        return decided;
    }
}

internal sealed class MeResponse
{
    public Guid Id { get; set; }
    public long EffectivePermissions { get; set; }
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
