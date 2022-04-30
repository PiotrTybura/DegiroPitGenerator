using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Integrations.Degiro.Comparers;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Models;
using Models.Operations;

namespace Integrations.Degiro.Adapters
{
    public interface IDividendAdapter
    {
        IEnumerable<Dividend> Adapt(List<CsvCashOperation> degiroCashOperations);
    }

    public class DividendAdapter : IDividendAdapter
    {
        private readonly DegiroConfiguration _configuration;
        private readonly ISameDividendRowComparer _sameDividendRowComparer;

        private readonly CultureInfo _datesCultureInfo;

        public DividendAdapter(DegiroConfiguration configuration)
        {
            _configuration = configuration;
            _sameDividendRowComparer = new SameDividendRowComparer();

            _datesCultureInfo = CultureInfo.GetCultureInfo(configuration.Domain.ReportsIsoLanguageCode);
        }

        public IEnumerable<Dividend> Adapt(List<CsvCashOperation> degiroCashOperations)
        {
            var dividends = degiroCashOperations.Where(_ => _.Description == _configuration.Domain.Translations.Dividend);
            var dividendsTaxes = degiroCashOperations.Where(_ => _.Description == _configuration.Domain.Translations.DividendTax).ToList();

            var dividendsInDay = dividends.GroupBy(_ => _, _sameDividendRowComparer);

            foreach (var sameDividendRows in dividendsInDay)
            {
                var dividend = sameDividendRows.Key;
                var amount = sameDividendRows.Sum(_ => _.ChangeAmount.Value);

                var sameDividendsTaxes = dividendsTaxes.Where(_ => _sameDividendRowComparer.Equals(_, sameDividendRows.Key)).ToList();
                var dividendTax = -1 * sameDividendsTaxes.Sum(_ => _.ChangeAmount.Value);

                dividendsTaxes = dividendsTaxes.Except(sameDividendsTaxes).ToList();

                yield return new Dividend
                {
                    Amount = amount,
                    Currency = ParseCurrency(dividend.ChangeCurrency),
                    Date = DateTime.Parse(dividend.Date, _datesCultureInfo),
                    //There are no stock names in cashOperation reports, so reference must base only on Isin
                    FinancialInstrumentReference = dividend.Isin,
                    FinancialInstrumentCommonName = dividend.Product,
                    PaidTaxAmount = dividendTax
                };
            }

            if (dividendsTaxes.Any())
                throw new ArgumentException("Some dividend taxes cannot be mapped to any dividend");
        }

        private Currency ParseCurrency(string currency)
        {
            var isMapping = _configuration.Domain.CurrenciesMapping.TryGetValue(currency, out var mappedCurrency);

            return isMapping ? Enum.Parse<Currency>(mappedCurrency) : Enum.Parse<Currency>(currency);
        }
    }
}
