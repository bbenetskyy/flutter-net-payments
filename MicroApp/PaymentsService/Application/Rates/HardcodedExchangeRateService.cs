using Common.Domain.Enums;

namespace PaymentsService.Application.Rates;

public sealed class HardcodedExchangeRateService : IExchangeRateService
{
    // Base rates with EUR as base
    // Example values; in real world this would query an external data source
    private static readonly Dictionary<Currency, decimal> EurTo = new()
    {
        { Currency.EUR, 1m },
        { Currency.USD, 1.10m },
        { Currency.PLN, 4.30m },
        { Currency.GBP, 0.85m }
    };

    public Task<decimal> GetRateAsync(Currency from, Currency to, CancellationToken ct = default)
    {
        if (from == to) return Task.FromResult(1m);

        // EUR -> X
        if (from == Currency.EUR)
            return Task.FromResult(EurTo[to]);

        // X -> EUR
        if (to == Currency.EUR)
        {
            var eurToFrom = EurTo[from];
            return Task.FromResult(1m / eurToFrom);
        }

        // X -> Y via EUR
        var rateToEur = 1m / EurTo[from];
        var rateEurToTarget = EurTo[to];
        var rate = rateToEur * rateEurToTarget;
        return Task.FromResult(rate);
    }
}
