### What “credit” and “debit” mean in your wallet
- The wallet uses double‑entry bookkeeping. Every operation is recorded as two ledger entries that balance each other.
- Accounts involved for wallet cash are represented by the `LedgerAccount` enum. For user‑visible funds, the important account is `Cash`.

Interpretation relative to the `Cash` account:
- Credit on Cash → increases your spendable wallet balance (money in).
- Debit on Cash → decreases your spendable wallet balance (money out).

In the top‑up endpoint you saw both a credit and a debit because every transaction must balance:
- `POST /wallets/{userId}/topup` writes two entries with the same amount and currency:
    - Credit to `Cash` (this is the part that increases the user’s balance)
    - Debit to `Clearing` (the internal counter‑account so books stay balanced)

So, for user understanding:
- “I’ve got money” (received) = Credit on Cash
- “I sent/spent money” (paid/withdrawn/transfer out) = Debit on Cash

### How balance is computed
All balances are computed per currency in minor units (integers; e.g., cents). The code does exactly this:

- Overall wallet balance by currency (see `/wallets/{userId}`):
  ```csharp
  .Where(x => x.WalletId == w.Id && x.Account == LedgerAccount.Cash)
  .GroupBy(x => x.Currency)
  .Select(g => new WalletBalanceItem(
      g.Key,
      g.Sum(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor)
  ))
  ```
  Meaning: for each currency, sum credits as `+amount` and debits as `-amount` on the `Cash` account.

- Balance for a specific currency after a top‑up (see the end of the top‑up handler):
  ```csharp
  .Where(x => x.WalletId == wallet.Id && x.Currency == req.Currency && x.Account == cash)
  .SumAsync(x => x.Type == LedgerEntryType.Credit ? x.AmountMinor : -x.AmountMinor);
  ```

Practical notes:
- Show balances per currency; do not add different currencies together.
- Convert from minor units to display (e.g., cents → 2 decimal places for USD) on the UI.

### How to display the ledger from `/wallets/{userId}/ledger`
That endpoint already filters to user‑visible cash movements:
```csharp
var q = db.Ledger.AsNoTracking()
  .Where(x => x.WalletId == w.Id && x.Account == LedgerAccount.Cash);
```
It returns a list of `LedgerEntryResponse` ordered by `CreatedAt`:
- `Id`
- `WalletId`
- `AmountMinor`
- `Currency`
- `Type` (Credit/Debit)
- `Account` (Cash — already filtered to this)
- `CounterpartyAccount` (e.g., Clearing; internal info but useful as a hint)
- `Description`
- `CorrelationId` (used for idempotency/grouping)
- `CreatedAt`

Recommended UI presentation for each row:
- Date/time: `CreatedAt`
- Description: `Description` (fallback based on `Type`, e.g., “Top-up”, “Payment”, etc.)
- Amount:
    - If `Type == Credit` → show `+` and green (money in)
    - If `Type == Debit` → show `-` and red (money out)
    - Always formatted from minor units with the right currency symbol
- Optional reference: short form of `CorrelationId`
- Optional running balance (client‑side): compute sequentially in UI by accumulating `+amount` for credits and `-amount` for debits in the order returned.

What NOT to show to the user:
- The internal balancing entry on non‑Cash accounts. The endpoint already hides those because it filters to `Account == Cash`. If other features later add non‑Cash entries, keep the UI focused on `Cash` movements for clarity.

### Why top‑up has both credit and debit
- Double‑entry means assets in Cash must be offset somewhere.
- For top‑up, the offset is `Clearing` (an internal liability/clearing account). The pair is:
    - Credit Cash (user funds go up)
    - Debit Clearing (internal account goes down for the same correlation)
- The operation is idempotent per wallet and `CorrelationId`. If the same `CorrelationId` is retried, you’ll get an `idempotent` response instead of duplicating funds.

### FAQ
- Does Credit always mean “money in” for the user? Yes, as long as you look at entries where `Account == Cash` (which both the balance and ledger endpoints do).
- How do I compute totals? Sum credits minus debits per currency on Cash.
- What about multiple currencies? Keep separate balances and ledgers; format each with the correct currency.

### Example display logic (pseudo‑code)
```pseudo
for entry in ledgerEntriesOrderedByCreatedAt:
  sign = entry.Type == Credit ? +1 : -1
  amount = sign * toDisplayUnits(entry.AmountMinor, entry.Currency)
  runningBalance += amount
  showRow(
    date = entry.CreatedAt,
    description = entry.Description ?? defaultDescription(entry),
    amountText = formatSigned(amount, entry.Currency),
    balanceText = format(runningBalance, entry.Currency),
    reference = short(entry.CorrelationId)
  )
```

### TL;DR for the UI
- Money received (top‑ups, incoming transfers): show as Credit on Cash → “+ amount”.
- Money sent (payments, withdrawals, outgoing transfers): show as Debit on Cash → “− amount”.
- Balance = sum of Cash credits minus debits, per currency.
- Ledger endpoint already gives you only user‑visible Cash movements; render them with +/− sign, date, description, and optionally a running balance and reference.