using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Extensions;
using Integrations.Exante.Models;
using Models;
using Models.Operations;
using Models.Operations.Transactions;

namespace Integrations.Exante.Adapters
{
    public interface ITransactionAdapter
    {
        IEnumerable<Transaction> Adapt(List<CsvTransaction> exanteTransactions, List<CsvCashOperation> exanteCashOperations);
    }

    public class TransactionAdapter : ITransactionAdapter
    {
        //private readonly DegiroConfiguration _configuration;
        //private readonly CultureInfo _datesCultureInfo;

        public TransactionAdapter(/*DegiroConfiguration configuration*/)
        {
            //_configuration = configuration;
            //_datesCultureInfo = CultureInfo.GetCultureInfo(configuration.Domain.ReportsIsoLanguageCode);
        }

        public IEnumerable<Transaction> Adapt(List<CsvTransaction> exanteTransactions, List<CsvCashOperation> exanteCashOperations)
        {
            var adaptedTransactions = AdaptTransactions(exanteTransactions).ToList();
            return CorrectTransactions(adaptedTransactions, exanteCashOperations).OrderBy(_ => _.Date);
        }

        private IEnumerable<Transaction> CorrectTransactions(IList<Transaction> adaptedTransactions, List<CsvCashOperation> exanteCashOperations)
        {
            adaptedTransactions = adaptedTransactions.Where(_ => !_.FinancialInstrumentCommonName.EndsWith("EXANTE") && !_.FinancialInstrumentCommonName.EndsWith("E.FX")).ToList();
            adaptedTransactions = CorrectSplits(adaptedTransactions, exanteCashOperations);
            adaptedTransactions = CorrectOverSubscription(adaptedTransactions, exanteCashOperations);

            return adaptedTransactions;
        }

        private IList<Transaction> CorrectOverSubscription(IList<Transaction> adaptedTransactions, List<CsvCashOperation> exanteCashOperations)
        {
            //This is a manual Exante operations, so in my case sometimes they added some entities without balance change
            var overSubscriptionOperations = exanteCashOperations.Where(_ => _.OperationType == "CORPORATE ACTION" && _.EurEquivalent != 0).GroupBy(_ => DateTime.Parse(_.When));

            foreach(var exanteOperation in overSubscriptionOperations)
            {
                if (exanteOperation.Count() != 2)
                    throw new NotImplementedException("In the oversubscription operation the algorithm always expect two rows, but it was different in the csv");

                var stockOperation = exanteOperation.Single(_ => _.Isin != "None");
                var cashOperation = exanteOperation.Single(_ => _.Isin == "None");

                var symbolId = exanteOperation.SelectSingle(_ => _.SymbolId);

                adaptedTransactions.Add(new Transaction
                {
                    Id = "oversubscription." + stockOperation.TransactionId,
                    FinancialInstrumentCommonName = stockOperation.SymbolId,
                    FinancialInstrumentReference = stockOperation.Isin,
                    Date = Convert.ToDateTime(stockOperation.When),
                    TransactionType = stockOperation.Sum > 0 ? TransactionType.BUY : TransactionType.SELL,
                    Quantity = decimal.ToInt32(stockOperation.Sum.Value),
                    TransactionPrice = cashOperation.Sum.Value * (-1),
                    TransactionCurrency = Enum.Parse<Currency>(cashOperation.Asset),
                    Fee = 0m,
                    FeeCurrency = default,
                    StockExchangeCountry = SelectCountry(symbolId.Substring(symbolId.IndexOf('.') + 1))
                });
            }

            return adaptedTransactions;
        }

        private IList<Transaction> CorrectSplits(IList<Transaction> adaptedTransactions, List<CsvCashOperation> exanteCashOperations)
        {
            //Transactions before split are corrected
            var splitOperations = exanteCashOperations.Where(_ => _.OperationType == "STOCK SPLIT").GroupBy(_ => DateTime.Parse(_.When).Date);

            foreach (var splitOperation in splitOperations)
            {
                //The split operation in Exante is very poorly provided. Therefore, for now the algorithm
                //expects that there are three consequtive split rows one after another.
                //1st - represents the cash balance change due to potential stock cut.
                //2nd - represents newly provided stocks
                //3rd - represents all previous stock taken as a part of split.

                var cashBalanceChangeEntity = splitOperation.ElementAt(0);
                var newStocksEntity = splitOperation.ElementAt(1);
                var oldStocksEntity = splitOperation.ElementAt(2);

                if (splitOperation.Count() != 3 ||
                    !Enum.IsDefined(typeof(Currency), cashBalanceChangeEntity.Asset) ||
                    newStocksEntity.Asset != oldStocksEntity.Asset ||
                    newStocksEntity.Sum < 0 ||
                    oldStocksEntity.Sum > 0)

                    throw new NotImplementedException("Unknown split operation noticed");

                var match = Regex.Match(cashBalanceChangeEntity.Comment, @"Stock split (\d+) for (\d+) fractional share payment");
                var splitNumerator = int.Parse(match.Groups[1].Value);
                var splitDenominator = int.Parse(match.Groups[2].Value);

                if (splitNumerator != 1)
                    throw new NotImplementedException("Unsupported split numerator noticed");

                var soldStocks = decimal.ToInt32(oldStocksEntity.Sum.Value + newStocksEntity.Sum.Value * splitDenominator);

                if (soldStocks != -1)
                    throw new NotImplementedException("Unsupported number of sold stocks as a part of split");

                var transactionsToCorrect = adaptedTransactions.Where(_ => _.FinancialInstrumentCommonName == oldStocksEntity.SymbolId && _.Date > DateTime.Parse(oldStocksEntity.When));
                transactionsToCorrect.ForEach(_ =>
                {
                    _.Quantity *= splitDenominator;
                    _.TransactionPrice /= splitDenominator;
                });

                adaptedTransactions.Add(new Transaction
                {
                    Id = "split." + oldStocksEntity.TransactionId,
                    FinancialInstrumentCommonName = oldStocksEntity.SymbolId,
                    FinancialInstrumentReference = oldStocksEntity.Isin,
                    Date = Convert.ToDateTime(oldStocksEntity.When),
                    TransactionType = TransactionType.SELL,
                    Quantity = soldStocks,
                    TransactionPrice = cashBalanceChangeEntity.Sum.Value,
                    TransactionCurrency = Enum.Parse<Currency>(cashBalanceChangeEntity.Asset),
                    Fee = 0m,
                    FeeCurrency = default,
                    StockExchangeCountry = SelectCountry(oldStocksEntity.SymbolId.Substring(oldStocksEntity.SymbolId.IndexOf('.') + 1))
                });
            }

            return adaptedTransactions;
        }

        private IEnumerable<Transaction> AdaptTransactions(List<CsvTransaction> exanteTransactions)
        {
            foreach (var exanteTransaction in exanteTransactions)
            {
                //CFDs or FUNDs can be traded with a portion of unit
                if (exanteTransaction.Type == "CFD" || exanteTransaction.Type == "FUND")
                    exanteTransaction.Quantity *= 100;

                if (exanteTransaction.Quantity % 1 != 0)
                    throw new NotImplementedException("The application is not ready for transactions with a portion of unit");

                yield return AdaptSingleTransaction(exanteTransaction);
            }
        }

        private Transaction AdaptSingleTransaction(CsvTransaction exanteTransaction)
        {
            return new Transaction
            {
                Id = exanteTransaction.OrderId,
                FinancialInstrumentCommonName = exanteTransaction.SymbolId,
                FinancialInstrumentReference = exanteTransaction.Isin == "None" ? exanteTransaction.SymbolId : exanteTransaction.Isin,
                Date = Convert.ToDateTime(exanteTransaction.Time),
                TransactionType = exanteTransaction.Side == "buy" ? TransactionType.BUY : TransactionType.SELL,
                Quantity = decimal.ToInt32(exanteTransaction.Side == "buy" ? exanteTransaction.Quantity : exanteTransaction.Quantity * (-1)),
                TransactionPrice = exanteTransaction.TradedVolume,
                TransactionCurrency = Enum.Parse<Currency>(exanteTransaction.Currency),
                Fee = exanteTransaction.Commission,
                //Sometimes there are no transaction Fees at all. Then FeeCurrency is left blank
                FeeCurrency = Enum.Parse<Currency>(exanteTransaction.CommissionCurrency),
                StockExchangeCountry = SelectCountry(exanteTransaction.SymbolId.Substring(exanteTransaction.SymbolId.IndexOf('.') + 1))
            };
        }

        private Country SelectCountry(string stockExchangeName)
        {
            switch(stockExchangeName)
            {
                case "ARCA":
                case "NASDAQ":
                case "NYSE":
                //Some CRYPTOS
                case "USD":
                    return Country.UnitedStatesOfAmerica;
                case "SGX":
                    return Country.Singapore;
                    //Exante dedicated products
                case "EXANTE":
                case "E.FX":
                    return Country.Cyprus;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
