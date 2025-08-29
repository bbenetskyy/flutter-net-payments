using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Security;
using Common.Validation;

namespace PaymentsService.Presentation.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        // In-memory verification store for demo/minimal implementation
        var store = new VerificationStore();

        // Get current user's payments

        // Create verification for a payment action (PaymentCreated or PaymentReverted)
        app.MapPost("/payments/verifications", async (HttpContext http, CreateVerificationRequest req, PaymentsDb db) =>
        {
            if (req.Action is not (VerificationAction.PaymentCreated or VerificationAction.PaymentReverted))
                return Results.BadRequest("Unsupported action for PaymentsService");

            var p = await db.Payments.FindAsync(req.TargetId);
            if (p is null) return Results.NotFound("Payment not found");

            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            Guid? assignee = req.Action == VerificationAction.PaymentCreated ? p.UserId : null;
            var v = store.Create(req.Action, req.TargetId, uid.Value, assignee);
            return Results.Created($"/payments/verifications/{v.Id}", v);
        }).RequirePermission(UserPermissions.ViewPayments);

        // Decide on a verification (accept/reject) with code
        app.MapPost("/payments/verifications/{id:guid}/decision", async (HttpContext http, Guid id, VerificationDecisionRequest req, PaymentsDb db, IConfiguration cfg) =>
        {
            if (id != req.VerificationId) return Results.BadRequest("Mismatched verification id");
            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            var v = store.Get(id);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();

            // Load target payment and user permissions
            var p = await db.Payments.FindAsync(v.TargetId);
            if (p is null) return Results.NotFound("Payment not found");

            var me = await GetMeAsync(http);
            if (me is null) return Results.Unauthorized();
            var perms = (PaymentsService.Presentation.Security.UserPermissions)me.EffectivePermissions;

            bool allowed = false;
            if (v.Action == VerificationAction.PaymentCreated)
            {
                allowed = (p.UserId == uid) || (perms & UserPermissions.ConfirmPayments) == UserPermissions.ConfirmPayments;
            }
            else if (v.Action == VerificationAction.PaymentReverted)
            {
                // Admin only (use ManageCompanyUsers as an admin-like permission)
                allowed = (perms & UserPermissions.ManageCompanyUsers) == UserPermissions.ManageCompanyUsers;
            }

            if (!allowed) return Results.Forbid();

            var newStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;
            v = store.Decide(id, newStatus);

            if (v.Action == VerificationAction.PaymentCreated)
            {
                p.Status = req.Accept ? PaymentStatus.Confirmed : PaymentStatus.Rejected;
                p.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            else if (v.Action == VerificationAction.PaymentReverted)
            {
                if (req.Accept)
                {
                    p.Status = PaymentStatus.Rejected; // minimal interpretation
                    p.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            return Results.Ok(v);
        }).RequireAuthorization();

        // Get current user's payments
        app.MapGet("/payments/my", async (ClaimsPrincipal user, PaymentsDb db) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var items = await db.Payments.AsNoTracking()
                .Where(p => p.UserId == uid)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Results.Ok(items.Select(ToDto));
        }).RequirePermission(UserPermissions.ViewPayments);

        // Create a new payment for current user
        app.MapPost("/payments", async (ClaimsPrincipal user, CreatePaymentRequest req, PaymentsDb db) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var err = Validate(req);
            if (err is not null) return Results.BadRequest(err);

            var p = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = uid,
                BeneficiaryName = req.BeneficiaryName.Trim(),
                BeneficiaryAccount = req.BeneficiaryAccount.Trim(),
                FromAccount = req.FromAccount.Trim(),
                Amount = req.Amount,
                Currency = (req.Currency ?? "EUR").Trim().ToUpperInvariant(),
                Details = string.IsNullOrWhiteSpace(req.Details) ? null : req.Details.Trim(),
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            db.Payments.Add(p);
            await db.SaveChangesAsync();
            return Results.Created($"/payments/{p.Id}", ToDto(p));
        }).RequirePermission(UserPermissions.CreatePayments);
    }

    private static string? Validate(CreatePaymentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.BeneficiaryName)) return "Beneficiary name is required";
        if (string.IsNullOrWhiteSpace(req.BeneficiaryAccount)) return "Beneficiary account (IBAN) is required";
        if (string.IsNullOrWhiteSpace(req.FromAccount)) return "From account (IBAN) is required";
        if (req.Amount <= 0) return "Amount must be greater than 0";
        if (!string.IsNullOrWhiteSpace(req.Currency) && req.Currency.Trim().Length != 3) return "Currency must be 3-letter ISO code";
        if (req.BeneficiaryName.Length > 200) return "Beneficiary name too long";
        if (req.BeneficiaryAccount.Length > 64) return "Beneficiary account too long";
        if (req.FromAccount.Length > 64) return "From account too long";
        return null;
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id,
        p.UserId,
        p.BeneficiaryName,
        p.BeneficiaryAccount,
        p.FromAccount,
        p.Amount,
        p.Currency,
        p.Details,
        p.Status,
        p.CreatedAt,
        p.UpdatedAt
    );

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
        var code = GenerateCode();
        var v = new VerificationDto(
            id,
            action,
            targetId,
            VerificationStatus.Pending,
            code,
            createdBy,
            assignee,
            DateTime.UtcNow,
            null
        );
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

    private string GenerateCode()
    {
        return _rng.Next(100000, 1000000).ToString(); // 6-digit
    }
}


internal sealed class MeResponse
{
    public Guid Id { get; set; }
    public long EffectivePermissions { get; set; }
}

public record CreatePaymentRequest(
    string BeneficiaryName,
    string BeneficiaryAccount,
    string FromAccount,
    decimal Amount,
    string? Currency,
    string? Details
);

public record PaymentDto(
    Guid Id,
    Guid UserId,
    string BeneficiaryName,
    string BeneficiaryAccount,
    string FromAccount,
    decimal Amount,
    string Currency,
    string? Details,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
