using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using CardsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CardsService.Presentation.Security;

using System.Security.Claims;
using System.Net.Http.Headers;
using CardsService.Application.DTOs;
using CardsService.Application.Validation;
using Common.Domain.Entities;
using Common.Infrastucture.Persistence;
using Common.Security;
using Common.Validation;

namespace CardsService.Presentation.Endpoints;

public static class CardsEndpoints
{
    public static void MapCardsEndpoints(this IEndpointRouteBuilder app)
    {
        // Create a card
        app.MapPost("/cards", async (CreateCardRequest req, CardsDb db,
            IVerificationStore store,
            IValidator<CreateCardRequest> validator) =>
        {
            var vr = validator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var card = new Card
            {
                Id = Guid.NewGuid(),
                Type = req.Type,
                Name = req.Type == CardType.Shared ? req.Name.Trim() : string.Empty,
                SingleTransactionLimit = req.SingleTransactionLimit,
                MonthlyLimit = req.MonthlyLimit,
                Printed = false,
            };
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            return Results.Created($"/cards/{card.Id}", ToDto(card));
        }).RequireAuthorization();

        // Assign to user
        app.MapPost("/cards/{id:guid}/assign", async (Guid id, AssignCardRequest req, CardsDb db,
            HttpContext http, ICardVerificationService verSvc, 
            IValidator<AssignCardRequest> reqValidator, IValidator<AssignCardOperation> opValidator) =>
        {
            var vr = reqValidator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var card = await db.Cards.FindAsync(id);
            if (card is null) return Results.NotFound();

            var vop = opValidator.Validate(new(card, req));
            if (!vop.IsValid) return Results.BadRequest(vop.Error);

            card.AssignedUserId = req.UserId;
            card.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // 🔔 Create verification internally
            var v = await verSvc.CreateForAssignmentAsync(card.Id, http.User);

            return Results.Ok(new { card = ToDto(card), verification = v });
        }).RequireAuthorization();

        // Update/manage card
        app.MapPut("/cards/{id:guid}", async (Guid id, UpdateCardRequest req, CardsDb db, 
            HttpContext http, ICardVerificationService verSvc, 
            IValidator<UpdateCardRequest> reqValidator, IValidator<UpdateCardOperation> opValidator) =>
        {
            var card = await db.Cards.FindAsync(id);
            if (card is null) return Results.NotFound();

            var vr = reqValidator.Validate(req);
            if (!vr.IsValid) return Results.BadRequest(vr.Error);

            var op = new UpdateCardOperation(card, req);
            var vop = opValidator.Validate(op);
            if (!vop.IsValid) return Results.BadRequest(vop.Error);
            
            var wasPrinted = card.Printed;
            
            if (!string.IsNullOrWhiteSpace(req.Name)) card.Name = req.Name.Trim();
            if (req.Type is CardType ct) card.Type = ct; // Type can be changed unless other rules apply
            if (req.SingleTransactionLimit is decimal stl) card.SingleTransactionLimit = stl;
            
            if (req.MonthlyLimit is decimal ml) card.MonthlyLimit = ml;
            //will be updated via verification
            // if (req.Printed is bool printed) card.Printed = printed;
            if (req.Options is CardOptions opts) card.Options = opts;

            card.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            VerificationResponse? verification = null;

            // 🔔 Example trigger: when someone requests printing (false -> true)
            if (req.Printed is true && wasPrinted == false)
            {
                if(card.AssignedUserId is null)
                    return Results.BadRequest("Cannot request printing for unassigned card");
                verification = await verSvc.CreateForPrintingAsync(card.Id, http.User);
            }

            return Results.Ok(new { card = ToDto(card), verification });
        }).RequireAuthorization();
        
        
        app.MapGet("/cards/verifications", async (
                VerificationStatus? status,
                Guid? targetId,
                Guid? assigneeId,
                Guid? createdBy,
                string? q, // <-- free-text search
                int? skip,
                int? take,
                VerificationsDb db,
                CancellationToken ct) =>
            {
                var s = Math.Max(0, skip ?? 0);
                var t = take is int x ? Math.Clamp(x, 1, 500) : 25; // sane defaults + cap

                VerificationAction[] allowedActions = [
                    VerificationAction.UserAssignedToCard , VerificationAction.CardPrinting , VerificationAction.CardTermination
                ];
                // base query
                IQueryable<Verification> query = db.Verifications.AsNoTracking()
                    .Where(v=> allowedActions.Contains(v.Action));

                // filters
                if (status.HasValue) query = query.Where(v => v.Status == status.Value);
                if (targetId.HasValue) query = query.Where(v => v.TargetId == targetId.Value);
                if (assigneeId.HasValue) query = query.Where(v => v.AssigneeUserId == assigneeId.Value);
                if (createdBy.HasValue) query = query.Where(v => v.CreatedBy == createdBy.Value);

                // search:
                // - if q parses as Guid -> match Id/TargetId/Assignee/CreatedBy
                // - else -> LIKE search on Code (and optionally Action/Status names)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();

                    if (Guid.TryParse(term, out var g))
                    {
                        query = query.Where(v =>
                            v.Id == g ||
                            v.TargetId == g ||
                            v.AssigneeUserId == g ||
                            v.CreatedBy == g);
                    }
                    else
                    {
                        var like = $"%{term}%";
                        // Prefer provider-specific case-insensitive functions if available.
                        // This version uses LIKE on Code and also allows searching by enum names.
                        query = query.Where(v =>
                            (EF.Functions.Like(v.Code, like)) ||
                            EF.Functions.Like(v.Action.ToString(), like) ||
                            EF.Functions.Like(v.Status.ToString(), like));
                    }
                }

                // total BEFORE paging
                var total = await query.CountAsync(ct);

                // order newest first, then page
                var items = await query
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip(s)
                    .Take(t)
                    .Select(v => new
                    {
                        v.Id,
                        v.Action,
                        v.TargetId,
                        v.Status,
                        v.Code,
                        v.CreatedBy,
                        v.AssigneeUserId,
                        v.CreatedAt,
                        v.DecidedAt
                    })
                    .ToListAsync(ct);

                return Results.Ok(new
                {
                    total,
                    skip = s,
                    take = t,
                    items
                });
            })
            .RequirePermission(UserPermissions.ManageCompanyUsers);

        app.MapPost("/cards/{id:guid}/request-termination", async (
            HttpContext http,
            Guid id,
            CardsDb db,
            ICardVerificationService verSvc) =>
        {
            var card = await db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (card is null) return Results.NotFound();
            
            if(card.AssignedUserId is null)
                return Results.BadRequest("Cannot request termination for unassigned card");

            // 🔔 Create “termination” verification
            var v = await verSvc.CreateForTerminationAsync(id, http.User);

            // Client will use /cards/verifications/{id}/decision later
            return Results.Accepted($"/cards/verifications/{v.Id}", v);
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
        app.MapPost("/cards/verifications", async (HttpContext http, CreateVerificationRequest req,
            IVerificationStore store, CardsDb db) =>
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
        app.MapPost("/cards/verifications/{id:guid}/decision", async (HttpContext http, Guid id,
            VerificationDecisionRequest req,
            IVerificationStore store, CardsDb db) =>
        {
            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            var v = await store.Get(id);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();

            var card = await db.Cards.FindAsync(v.TargetId);
            if (card is null) return Results.NotFound("Card not found");

            // Permissions via /me
            var me = await GetMeAsync(http);
            if (me is null) return Results.Unauthorized();
            var perms = (UserPermissions)me.EffectivePermissions;

            bool allowed = false;
            if (v.Action == VerificationAction.UserAssignedToCard)
                allowed = card.AssignedUserId == uid;
            else if (v.Action == VerificationAction.CardPrinting)
                allowed = (card.AssignedUserId == uid) || ((perms & UserPermissions.ManageCompanyCards) == UserPermissions.ManageCompanyCards);
            else if (v.Action == VerificationAction.CardTermination)
                allowed = card.AssignedUserId == uid;

            if (!allowed) return Results.Forbid();

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = await store.Decide(id, newStatus);
            
            if(newStatus == VerificationStatus.Completed)
            {
                if (v.Action == VerificationAction.UserAssignedToCard && card.Type == CardType.Personal)
                {
                    card.Name = me.DisplayName;
                }
                else if (v.Action == VerificationAction.CardPrinting)
                {
                    card.Printed = true;
                }
                else if (v.Action == VerificationAction.CardTermination)
                {
                    card.Terminated = true;
                }
            }
            else
            {
                if(v.Action == VerificationAction.UserAssignedToCard)
                {
                    card.AssignedUserId = null;
                }
            }
            card.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            
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

    private static CardResponse ToDto(Card c) => new(
        c.Id, c.Type, c.Name, c.SingleTransactionLimit, c.MonthlyLimit, c.AssignedUserId, c.Options,
        c.Printed, c.Terminated ,c.CreatedAt, c.UpdatedAt);

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
