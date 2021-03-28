using System;
using System.Collections.Generic;
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
        private readonly DegiroConfiguration _configuration;

        public FeeAdapter(DegiroConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<Fee> Adapt(List<CsvCashOperation> degiroCashOperations)
        {
            var fees = new List<Fee>();

            //A yearly fee paid for every foreign stock exchange where there was at least one transaction in the year
            fees.AddRange(GetFeesByType(degiroCashOperations, _configuration.Domain.Translations.ForeignExchangeFee, FeeType.ForeignStockConnection));

            //Fees for stocks bought using a loan
            fees.AddRange(GetFeesByType(degiroCashOperations, _configuration.Domain.Translations.Interest, FeeType.Interest));

            //Fees for short-transactions
            fees.AddRange(GetFeesByType(degiroCashOperations, _configuration.Domain.Translations.ShortInterest, FeeType.ShortInterest));

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
                    Date = DateTime.Parse(fee.Date),
                    FeeType = type
                };
        }
    }
}
