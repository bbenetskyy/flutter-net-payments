namespace PaymentsService.Application;

public interface IExchangeRateService
{
    // Returns multiplicative factor to convert an amount from 'from' currency to 'to' currency
    Task<decimal> GetRateAsync(Common.Domain.Enums.Currency from, Common.Domain.Enums.Currency to, CancellationToken ct = default);
}
