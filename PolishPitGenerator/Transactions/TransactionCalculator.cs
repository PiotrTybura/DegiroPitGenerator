using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Models.ExchangeRate;
using Models.Operations.Transactions;
using PolishPitGenerator.Transactions.Models;

namespace PolishPitGenerator.Transactions
{
    internal interface ITransactionCalculator
    {
        List<FinancialInstrumentBalance> Calculate(IEnumerable<Transaction> transactions, int pitYear);
    }

    internal class TransactionCalculator : ITransactionCalculator
    {
        private readonly IExchangeRateSolver _exchangeRateSolver;

        internal TransactionCalculator(IExchangeRateSolver exchangeRateSolver)
        {
            _exchangeRateSolver = exchangeRateSolver;
        }

        /// <param name="transactions">Transactions need to be provided in ascending order due to FIFO requirement</param>
        public List<FinancialInstrumentBalance> Calculate(IEnumerable<Transaction> transactions, int pitYear)
        {
            var financialInstrumentBalances = new List<FinancialInstrumentBalance>();

            foreach (var transaction in transactions)
            {
                var financialInstrumentBalance = financialInstrumentBalances
                    .SingleOrDefault(_ => _.FinancialInstrumentReference == transaction.FinancialInstrumentReference);
                if (financialInstrumentBalance == null)
                {
                    financialInstrumentBalance = new FinancialInstrumentBalance
                    {
                        StockExchangeCountry = transaction.StockExchangeCountry,
                        FinancialInstrumentCommonName = transaction.FinancialInstrumentCommonName,
                        FinancialInstrumentReference = transaction.FinancialInstrumentReference,
                        UnitBalance = 0,
                        TransactionUnits = new List<TransactionUnit>()
                    };
                    financialInstrumentBalances.Add(financialInstrumentBalance);
                }

                if (IfOpenTransaction(financialInstrumentBalance, transaction))
                {
                    financialInstrumentBalance.UnitBalance += transaction.Quantity;

                    financialInstrumentBalance.TransactionUnits.AddRange(Enumerable.Range(0, transaction.Quantity).Select(_ => new TransactionUnit
                    {
                        OpenEvent = GetTransactionEvent(transaction)
                    }));
                }
                else
                {
                    financialInstrumentBalance.UnitBalance -= transaction.Quantity;

                    financialInstrumentBalance.TransactionUnits
                        .Where(_ => _.CloseEvent == default).Take(transaction.Quantity)
                        .ToList().ForEach(_ => _.CloseEvent = GetTransactionEvent(transaction));
                }
            }

            foreach (var financialInstrumentBalance in financialInstrumentBalances)
                financialInstrumentBalance.AllUnitsClosedInYear =
                    financialInstrumentBalance.TransactionUnits.Where(_ => _.CloseEvent?.Date.Year == pitYear).ToList();

            return financialInstrumentBalances;
        }

        private bool IfOpenTransaction(FinancialInstrumentBalance financialInstrumentBalance, Transaction transaction)
        {
            return Math.Sign(financialInstrumentBalance.UnitBalance) == 0 ||
                Math.Sign(financialInstrumentBalance.UnitBalance) == 1 && transaction.TransactionType == TransactionType.BUY ||
                Math.Sign(financialInstrumentBalance.UnitBalance) == -1 && transaction.TransactionType == TransactionType.SELL;
        }

        private TransactionEvent GetTransactionEvent(Transaction transaction)
        {
            return new TransactionEvent
            {
                Date = transaction.Date,
                UnitPrice = transaction.TransactionPrice / transaction.Quantity,
                UnitCurrency = transaction.TransactionCurrency,
                UnitPitExchangeRate = _exchangeRateSolver.GetNbpExchangeRate(transaction.TransactionCurrency, transaction.Date),
                Fee = transaction.Fee / transaction.Quantity,
                FeeCurrency = transaction.FeeCurrency,
                FeePitExchangeRate = transaction.FeeCurrency.HasValue ?
                    _exchangeRateSolver.GetNbpExchangeRate(transaction.FeeCurrency.Value, transaction.Date) : default
            };
        }
    }
}
