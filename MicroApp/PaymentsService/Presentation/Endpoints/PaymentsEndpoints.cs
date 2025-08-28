using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Security;

namespace PaymentsService.Presentation.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
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
