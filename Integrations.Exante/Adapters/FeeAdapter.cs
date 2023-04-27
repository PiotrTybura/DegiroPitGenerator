using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Integrations.Exante.Models;
using Models;
using Models.Operations.Fees;

namespace Integrations.Exante.Adapters
{
    public interface IFeeAdapter
    {
        IEnumerable<Fee> Adapt(List<CsvCashOperation> degiroCashOperations);
    }

    public class FeeAdapter : IFeeAdapter
    {
        public IEnumerable<Fee> Adapt(List<CsvCashOperation> exanteCashOperations)
        {
            var fees = new List<Fee>();

            fees.AddRange(GetFeesByType(exanteCashOperations, "BANK CHARGE", FeeType.WithdrawCost));
            fees.AddRange(GetFeesByType(exanteCashOperations, "INTEREST", FeeType.Interest));
            fees.AddRange(GetFeesByType(exanteCashOperations, "ROLLOVER", FeeType.RolloverInterest));

            //Autoconversion
            fees.AddRange(GetAutoConversionFees(exanteCashOperations));

            //Spreads
            fees.AddRange(GetSpreads(exanteCashOperations));

            return fees;
        }

        private IEnumerable<Fee> GetSpreads(List<CsvCashOperation> exanteCashOperations)
        {
            var spreadsOperations = exanteCashOperations.Where(_ => _.SymbolId.EndsWith(".EXANTE") && _.OperationType == "TRADE").GroupBy(_ => _.Date);

            if (spreadsOperations.Any(_ => _.Count() != 2))
                throw new NotImplementedException("Spreads issue");

            foreach (var spreadsOperation in spreadsOperations)
                yield return new Fee
                {
                    Amount = Math.Abs(spreadsOperation.First().EurEquivalent.Value + spreadsOperation.Last().EurEquivalent.Value),
                    Currency = Currency.EUR,
                    Date = spreadsOperation.SelectSingle(_ => _.Date),
                    FeeType = FeeType.CurrencySpreads
                };
        }

        private IEnumerable<Fee> GetAutoConversionFees(List<CsvCashOperation> exanteCashOperations)
        {
            var conversionOperations = exanteCashOperations.Where(_ => _.OperationType == "AUTOCONVERSION" && _.Sum < 0);

            foreach (var conversionOperation in conversionOperations)
                yield return new Fee
                {
                    Amount = Math.Abs(conversionOperation.EurEquivalent.Value * 0.002m),
                    Currency = Currency.EUR,
                    Date = conversionOperation.Date,
                    FeeType = FeeType.CurrencyAutoConversion
                };
        }

        private IEnumerable<Fee> GetFeesByType(List<CsvCashOperation> cashOperations, string operationType, FeeType type)
        {
            var fees = cashOperations.Where(_ => _.OperationType == operationType);

            foreach (var fee in fees)
                yield return new Fee
                {
                    Amount = Math.Abs(fee.Sum.Value),
                    Currency = Enum.Parse<Currency>(fee.Asset),
                    Date = fee.Date,
                    FeeType = type
                };
        }
    }
}
