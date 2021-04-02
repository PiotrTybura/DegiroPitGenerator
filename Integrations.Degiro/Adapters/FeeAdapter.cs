using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Models;
using Models.Operations.Fees;

namespace Integrations.Degiro.Adapters
{
    public interface IFeeAdapter
    {
        IEnumerable<Fee> Adapt(List<CsvCashOperation> degiroCashOperations);
    }

    public class FeeAdapter : IFeeAdapter
    {
        private readonly Translations _translations;
        private readonly CultureInfo _datesCultureInfo;

        public FeeAdapter(DegiroConfiguration configuration)
        {
            _translations = configuration.Domain.Translations;
            _datesCultureInfo = CultureInfo.GetCultureInfo(configuration.Domain.ReportsIsoLanguageCode);
        }

        public IEnumerable<Fee> Adapt(List<CsvCashOperation> degiroCashOperations)
        {
            var fees = new List<Fee>();

            //A yearly fee paid for every foreign stock exchange where there was at least one transaction in the year
            fees.AddRange(GetFeesByType(degiroCashOperations, _translations.ForeignExchangeFee, FeeType.ForeignStockConnection));

            //Fees for stocks bought using a loan
            fees.AddRange(GetFeesByType(degiroCashOperations, _translations.Interest, FeeType.Interest));

            //Fees for short-transactions
            fees.AddRange(GetFeesByType(degiroCashOperations, _translations.ShortInterest, FeeType.ShortInterest));

            return fees;
        }

        private IEnumerable<Fee> GetFeesByType(List<CsvCashOperation> cashOperations, string description, FeeType type)
        {
            var fees = cashOperations.Where(_ => _.Description.StartsWith(description));

            foreach (var fee in fees)
                yield return new Fee
                {
                    Amount = Math.Abs(fee.ChangeAmount.Value),
                    Currency = Enum.Parse<Currency>(fee.ChangeCurrency),
                    Date = DateTime.Parse(fee.Date, _datesCultureInfo),
                    FeeType = type
                };
        }
    }
}
