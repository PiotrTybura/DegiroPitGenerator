using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Models;
using Models.Operations;
using Models.Operations.Transactions;

namespace Integrations.Degiro.Adapters
{
    public interface ITransactionAdapter
    {
        IEnumerable<Transaction> Adapt(List<CsvTransaction> degiroTransactions);
    }

    public class TransactionAdapter : ITransactionAdapter
    {
        private readonly DegiroConfiguration _configuration;

        public TransactionAdapter(DegiroConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<Transaction> Adapt(List<CsvTransaction> degiroTransactions)
        {
            foreach (var degiroTransaction in degiroTransactions.OrderBy(_ => DateTime.Parse(_.Date)).ThenBy(_ => TimeSpan.Parse(_.Time)).GroupBy(_ => _.TransactionId))
            {
                var feeSum = degiroTransaction.Sum(_ => Math.Abs(_.FeeAmount ?? 0));

                //The Distinct().Single() confirms that all dates are the same within TransactionId
                yield return new Transaction
                {
                    Id = degiroTransaction.SelectSingle(_ => _.TransactionId),
                    FinancialInstrumentCommonName = degiroTransaction.SelectSingle(_ => _.InstrumentName),
                    FinancialInstrumentReference = degiroTransaction.SelectSingle(_ => $"{_.Isin}.{_.StockExchangeName}"),
                    Date = Convert.ToDateTime(degiroTransaction.SelectSingle(_ => _.Date)),
                    TransactionType = degiroTransaction
                        .SelectSingle(_ => _.Quantity > 0 ? TransactionType.BUY : TransactionType.SELL),
                    Quantity = degiroTransaction.Sum(_ => Math.Abs(_.Quantity.Value)),
                    TransactionPrice = Math.Abs(degiroTransaction.Sum(_ => _.LocalValue.Value)),
                    TransactionCurrency = degiroTransaction.SelectSingle(_ => Enum.Parse<Currency>(_.LocalCurrency)),
                    Fee = feeSum,
                    //Sometimes there are no transaction Fees at all. Then FeeCurrency is left blank
                    FeeCurrency = feeSum != 0
                        ? degiroTransaction.Where(_ => _.FeeCurrency != "")
                            .SelectSingle(_ => Enum.Parse<Currency>(_.FeeCurrency))
                        : default,
                    StockExchangeCountry = SelectCountry(degiroTransaction.SelectSingle(_ => _.StockExchangeName))
                };
            }
        }

        private Country SelectCountry(string stockExchangeName)
        {
            return _configuration.Domain.StockCountriesMapping[stockExchangeName];
        }
    }
}
