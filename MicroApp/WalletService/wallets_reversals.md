Wallet payments: capture vs reversal (refund/chargeback)

Context
- The wallet uses double-entry bookkeeping with entries in minor units per currency.
- User-visible balance and ledger consider only Account == Cash.
- Payment workflow publishes internal events with the same IntentId for the payment; different event types are:
  - PaymentCaptured
  - RefundSucceeded
  - ChargebackReceived

What changed
- Previously, all payment events used CorrelationId = IntentId with a global idempotency check. This mistakenly blocked recording reversal events (refund/chargeback) if a capture already existed.
- Now, for idempotency and grouping we derive a deterministic correlation id from IntentId + EventType. This means:
  - Captures and reversals use different CorrelationId values, so they no longer collide.
  - Retries of the same event type remain idempotent (same derived CorrelationId).

How balances are computed (unchanged)
- Balance per currency = sum of Cash credits minus debits.
- A capture on the payer wallet is a Debit (money out). On the payee wallet it is a Credit (money in).
- A reversal (refund/chargeback) writes the opposite entries, netting the prior effect:
  - Payee wallet gets a Debit for the refund (money out).
  - Payer wallet gets a Credit for the refund (money in).
- Therefore balances naturally reflect the net effect without any special-case logic.

What the endpoints return during/after a reversal
- GET /wallets/{userId}
  - The balance includes all Cash entries; a reversal reduces/increases the user balance accordingly by the amount of the reversal.
- GET /wallets/{userId}/ledger[?correlationId]
  - The ledger lists individual entries on Cash ordered by CreatedAt.
  - A capture produces one Cash entry for the payer (Debit) and one for the payee (Credit), each with its derived correlation id for PaymentCaptured.
  - A reversal produces one Cash entry for the (original) payee (Debit) and one for the (original) payer (Credit), each with a distinct derived correlation id for Refund/Chargeback.
  - If you query by correlationId you will see the pair of cash/clearing entries for that specific event type only. Captures and reversals have different correlation ids by design.

UI/Display guidance
- Show each Cash entry as a row with sign based on Type:
  - Credit → + amount (green)
  - Debit → − amount (red)
- Optional: client-side running balance by accumulating +/− in CreatedAt order.
- Do not hide reversal rows. They are user-meaningful movements and ensure the running balance reconciles to the headline numbers.
- If you want to group logical payments, you can display rows grouped by payment IntentId and show sub-rows for each event type; use the Description and CreatedAt to clarify the sequence.

Notes and edge cases
- Idempotency: both capture and reversal are idempotent independently. Re-sending the same event type will not duplicate entries.
- Multiple reversals (partial refunds) are not currently modelled; every event is full-amount. If needed later, the same mechanism supports partial amounts — balances will still compute as credits minus debits.
- Multi-currency: all amounts are per-currency; do not aggregate across currencies.
