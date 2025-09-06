using System.Security.Claims;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Infrastructure.Persistence;
using PaymentsService.Presentation.Security;
using Common.Validation;
using Common.Domain.Enums;
using Common.Infrastucture.Persistence;
using Common.Security;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PaymentsService.Presentation.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        // Get current user's payments
          app.MapGet("/payments/verifications", async (
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
                    VerificationAction.PaymentCreated , VerificationAction.PaymentReverted
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
            .RequirePermission(UserPermissions.ConfirmPayments);
        
        // Create verification for a payment action (PaymentCreated or PaymentReverted)
        app.MapPost("/payments/verifications", async (HttpContext http, CreateVerificationRequest req, PaymentsDb db, IVerificationStore store) =>
        {
            if (req.Action is not (VerificationAction.PaymentCreated or VerificationAction.PaymentReverted))
                return Results.BadRequest("Unsupported action for PaymentsService");

            var p = await db.Payments.FindAsync(req.TargetId);
            if (p is null) return Results.NotFound("Payment not found");

            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            Guid? assignee = req.Action == VerificationAction.PaymentCreated ? p.UserId : null;
            var v = await store.Create(req.Action, req.TargetId, uid.Value, assignee);
            return Results.Created($"/payments/verifications/{v.Id}", v);
        }).RequirePermission(UserPermissions.CreatePayments);

        // Decide on a verification (accept/reject) with code
        app.MapPost("/payments/verifications/{id:guid}/decision", async (HttpContext http, Guid id, VerificationDecisionRequest req, PaymentsDb db, IConfiguration cfg, IVerificationStore store) =>
        {
            var uid = GetUserId(http.User);
            if (uid is null) return Results.Unauthorized();

            var v = await store.Get(id);
            if (v is null) return Results.NotFound();
            if (v.Status != VerificationStatus.Pending) return Results.BadRequest("Already decided");
            if (!string.Equals(v.Code, req.Code, StringComparison.Ordinal)) return Results.Unauthorized();

            // Load target payment and user permissions
            var p = await db.Payments.FindAsync(v.TargetId);
            if (p is null) return Results.NotFound("Payment not found");

            var me = await GetMeAsync(http);
            if (me is null) return Results.Unauthorized();
            var perms = (UserPermissions)me.EffectivePermissions;

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

            var intendedStatus = req.Accept ? VerificationStatus.Completed : VerificationStatus.Rejected;

            if (intendedStatus == VerificationStatus.Rejected)
            {
                v = await store.Decide(id, VerificationStatus.Rejected);
                return Results.Ok(v);
            }

            // intended Completed
            if (v.Action == VerificationAction.PaymentCreated)
            {
                try
                {
                    var client = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("wallet");
                    // Compute minor units with 2 decimals
                    var amountRounded = Math.Round(p.Amount, 2, MidpointRounding.AwayFromZero);
                    long amountMinor = (long)Math.Round(amountRounded * 100m, 0, MidpointRounding.AwayFromZero);
                    var evt = new WalletEvent
                    {
                        IntentId = p.Id,
                        UserId = p.UserId,                      // payer
                        BeneficiaryId = p.BeneficiaryId!.Value, // payee
                        AmountMinor = amountMinor,
                        Currency = p.Currency,
                        EventType = Common.Domain.Enums.PaymentEventType.PaymentCaptured,
                        Description = string.IsNullOrWhiteSpace(p.Details) ? $"Payment {p.Id} to {p.BeneficiaryName}" : p.Details
                    };
                    var res = await client.PostAsJsonAsync("/internal/events/payment", evt);
                    var body = await res.Content.ReadAsStringAsync();

                    v = await store.Decide(id, res.IsSuccessStatusCode ? VerificationStatus.Completed : VerificationStatus.Rejected);
                    p.Status = res.IsSuccessStatusCode ? PaymentStatus.Confirmed : PaymentStatus.Rejected;

                    p.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    return Results.Content(body, res.Content.Headers.ContentType?.ToString(), Encoding.Default, (int)res.StatusCode);
                }
                catch (Exception ex)
                {
                    v = await store.Decide(id, VerificationStatus.Rejected);
                    p.Status = PaymentStatus.Rejected;
                    p.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    return Results.BadRequest("Failed to publish wallet event: " + ex.Message);
                }
            }
            else if (v.Action == VerificationAction.PaymentReverted)
            {
                try
                {
                    var client = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("wallet");
                    var amountRounded = Math.Round(p.Amount, 2, MidpointRounding.AwayFromZero);
                    long amountMinor = (long)Math.Round(amountRounded * 100m, 0, MidpointRounding.AwayFromZero);
                    var evt = new WalletEvent
                    {
                        IntentId = p.Id,
                        UserId = p.UserId,                      // original payer
                        BeneficiaryId = p.BeneficiaryId!.Value, // original payee
                        AmountMinor = amountMinor,
                        Currency = p.Currency,
                        EventType = Common.Domain.Enums.PaymentEventType.RefundSucceeded,
                        Description = string.IsNullOrWhiteSpace(p.Details)
                            ? $"Reverted payment {p.Id} from {p.BeneficiaryName}"
                            : p.Details
                    };
                    var res = await client.PostAsJsonAsync("/internal/events/payment", evt);
                    var body = await res.Content.ReadAsStringAsync();

                    v = await store.Decide(id, res.IsSuccessStatusCode ? VerificationStatus.Completed : VerificationStatus.Rejected);
                    p.Status = res.IsSuccessStatusCode ? PaymentStatus.Confirmed : PaymentStatus.Rejected;

                    p.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();


                    return Results.Content(body, res.Content.Headers.ContentType?.ToString(), Encoding.Default, (int)res.StatusCode);
                }
                catch (Exception ex)
                {
                    v = await store.Decide(id, VerificationStatus.Rejected);
                    p.Status = PaymentStatus.Rejected;
                    p.UpdatedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    return Results.BadRequest("Failed to publish revert wallet event: " + ex.Message);
                }
            }
            return Results.Ok(v);
        }).RequirePermission(UserPermissions.ConfirmPayments);

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
        app.MapPost("/payments", async (HttpContext http, ClaimsPrincipal user, 
            CreatePaymentRequest req, PaymentsDb db, IVerificationStore store) =>
        {
            var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var uid))
                return Results.Unauthorized();

            var err = Validate(req);
            if (err is not null) return Results.BadRequest(err);

            var fromIban = req.FromAccount.Trim();
            var client = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("wallet");
            var authHeader = http.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

            using var accountsRes = await client.GetAsync("/accounts/my");
            if (!accountsRes.IsSuccessStatusCode)
                return Results.BadRequest("Failed to fetch accounts");
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new JsonStringEnumConverter());
            var accounts = await accountsRes.Content.ReadFromJsonAsync<List<AccountSlim>>(options) ?? new();
            var acc = accounts.FirstOrDefault(a => a.Iban == fromIban);
            if (acc is null) return Results.BadRequest("From account not found for current user");

            // Resolve beneficiary user by DisplayName via UsersService
            var usersClient = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("users");
            var authHeader2 = http.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader2))
                usersClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader2);

            using var usersRes = await usersClient.GetAsync("/users");
            if (!usersRes.IsSuccessStatusCode)
                return Results.BadRequest("Failed to fetch users");
            var users = await usersRes.Content.ReadFromJsonAsync<List<UserListItem>>(options) ?? new();
            var beneficiaryName = req.BeneficiaryName.Trim();
            var matches = users.Where(u => string.Equals(u.DisplayName, beneficiaryName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0) return Results.BadRequest("Beneficiary user not found");
            if (matches.Count > 1) return Results.BadRequest("Multiple users found with the same name; please use a unique beneficiary");
            var beneficiary = matches[0];

            // Fetch beneficiary accounts via WalletService internal endpoint
            var walletInternal = http.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("wallet");
            var bearer = http.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(bearer))
                walletInternal.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(bearer);
            using var benAccRes = await walletInternal.GetAsync($"/accounts/{beneficiary.Id}");
            if (!benAccRes.IsSuccessStatusCode)
                return Results.BadRequest("Failed to fetch beneficiary accounts");
            var benAccounts = await benAccRes.Content.ReadFromJsonAsync<List<AccountSlim>>(options) ?? new();
            var beneficiaryIban = req.BeneficiaryAccount.Trim();
            var benAcc = benAccounts.FirstOrDefault(a => a.Iban == beneficiaryIban);
            if (benAcc is null) return Results.BadRequest("Beneficiary account not found for the specified user");

            var p = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = uid,
                BeneficiaryName = beneficiaryName,
                BeneficiaryAccount = beneficiaryIban,
                BeneficiaryId = beneficiary.Id,
                BeneficiaryAccountId = benAcc.Id,
                FromAccount = fromIban,
                Amount = req.Amount,
                Currency = req.Currency ?? Currency.EUR,
                FromCurrency = acc.Currency,
                Details = string.IsNullOrWhiteSpace(req.Details) ? null : req.Details.Trim(),
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Auto-create verification for the new payment and decide response based on its status
            try
            {
                var v = await store.Create(VerificationAction.PaymentCreated, p.Id, uid, p.UserId);

                db.Payments.Add(p);
                await db.SaveChangesAsync();
                return Results.Created($"/payments/{p.Id}", new { payment = ToDto(p), verification = v });
            }
            catch (Exception ex)
            {
                // Do not fail the payment creation if verification store has a transient issue
                Console.WriteLine($"[PaymentsService] Failed to create verification for payment {p.Id}: {ex.Message}");
                return Results.BadRequest("Failed to create verification for payment: " + ex.Message);
            }
        }).RequirePermission(UserPermissions.CreatePayments);
    }

    private static string? Validate(CreatePaymentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.BeneficiaryName)) return "Beneficiary name is required";
        if (string.IsNullOrWhiteSpace(req.BeneficiaryAccount)) return "Beneficiary account (IBAN) is required";
        if (string.IsNullOrWhiteSpace(req.FromAccount)) return "From account (IBAN) is required";
        if (req.Amount <= 0) return "Amount must be greater than 0";
        if (req.BeneficiaryName.Length > 200) return "Beneficiary name too long";
        if (req.BeneficiaryAccount.Length > 64) return "Beneficiary account too long";
        if (req.FromAccount.Length > 64) return "From account too long";
        // IBAN format validation (normalized)
        var benIban = Common.Security.Hashing.NormalizeIban(req.BeneficiaryAccount);
        var fromIban = Common.Security.Hashing.NormalizeIban(req.FromAccount);
        if (!Validation.IsValidIban(benIban)) return "Invalid beneficiary IBAN format";
        if (!Validation.IsValidIban(fromIban)) return "Invalid from-account IBAN format";
        return null;
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id,
        p.UserId,
        p.BeneficiaryName,
        p.BeneficiaryAccount,
        p.BeneficiaryId,
        p.BeneficiaryAccountId,
        p.FromAccount,
        p.Amount,
        p.Currency,
        p.FromCurrency,
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

internal sealed class AccountSlim
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Iban { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Common.Domain.Enums.Currency Currency { get; set; } = Common.Domain.Enums.Currency.EUR;
    public DateTime CreatedAt { get; set; }
}

internal sealed class MeResponse
{
    public Guid Id { get; set; }
    public long EffectivePermissions { get; set; }
}

internal sealed class UserListItem
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public record CreatePaymentRequest(
    string BeneficiaryName,
    string BeneficiaryAccount,
    string FromAccount,
    decimal Amount,
    Currency? Currency,
    string? Details
);

public record PaymentDto(
    Guid Id,
    Guid UserId,
    string BeneficiaryName,
    string BeneficiaryAccount,
    Guid? BeneficiaryId,
    Guid? BeneficiaryAccountId,
    string FromAccount,
    decimal Amount,
    Currency Currency,
    Currency FromCurrency,
    string? Details,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);