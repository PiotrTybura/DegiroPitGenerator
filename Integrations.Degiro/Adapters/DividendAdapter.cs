using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Extensions;
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
        private readonly CultureInfo _datesCultureInfo;

        public DividendAdapter(DegiroConfiguration configuration)
        {
            _configuration = configuration;
            _datesCultureInfo = CultureInfo.GetCultureInfo(configuration.Domain.ReportsIsoLanguageCode);
        }

        public IEnumerable<Dividend> Adapt(List<CsvCashOperation> degiroCashOperations)
        {
            var dividends = degiroCashOperations.Where(_ => _.Description == _configuration.Domain.Translations.Dividend);
            var dividendsTaxes = degiroCashOperations.Where(_ => _.Description == _configuration.Domain.Translations.DividendTax).ToList();

            foreach (var dividendRow in dividends)
            {
                var dividendTax = dividendsTaxes.ZeroOrSingle(_ => TransactionEquals(_, dividendRow));
                dividendsTaxes.Remove(dividendTax);

                yield return new Dividend
                {
                    Amount = dividendRow.ChangeAmount.Value,
                    Currency = ParseCurrency(dividendRow.ChangeCurrency),
                    Date = DateTime.Parse(dividendRow.Date, _datesCultureInfo),
                    //There are no stock names in cashOperation reports, so reference must base only on Isin
                    FinancialInstrumentReference = dividendRow.Isin,
                    FinancialInstrumentCommonName = dividendRow.Product,
                    PaidTaxAmount = dividendTax != default ? Math.Abs(dividendTax.ChangeAmount.Value) : default
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

        private bool TransactionEquals(CsvCashOperation csvCashOperation1, CsvCashOperation csvCashOperation2) =>
            csvCashOperation1.Isin == csvCashOperation2.Isin
            && csvCashOperation1.Date == csvCashOperation2.Date
            && csvCashOperation1.ExecutionDate == csvCashOperation2.ExecutionDate
            && csvCashOperation1.ExecutionTime == csvCashOperation2.ExecutionTime
            && csvCashOperation1.ChangeCurrency == csvCashOperation2.ChangeCurrency;
    }
}
