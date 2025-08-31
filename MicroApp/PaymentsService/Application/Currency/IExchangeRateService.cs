using System.Threading;
using Common.Domain.Enums;

namespace PaymentsService.Application.Rates;

public interface IExchangeRateService
{
    // Returns multiplicative factor to convert an amount from 'from' currency to 'to' currency
    Task<decimal> GetRateAsync(Currency from, Currency to, CancellationToken ct = default);
}
