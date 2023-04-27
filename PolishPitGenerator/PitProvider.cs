using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions;
using Models.ExchangeRate;
using Models.Operations;
using PolishPitGenerator.Dividends;
using PolishPitGenerator.Dividends.Models;
using PolishPitGenerator.Fees;
using PolishPitGenerator.Fees.Models;
using PolishPitGenerator.Reports.Models;
using PolishPitGenerator.Transactions;
using PolishPitGenerator.Transactions.Models;

namespace PolishPitGenerator
{
    public class PitProvider
    {
        private ITransactionCalculator _transactionCalculator;
        private IFeeCalculator _feeCalculator;
        private IDividendCalculator _dividendCalculator;

        public PitProvider(IEnumerable<ExchangeRate> exchangeRates)
        {
            var exchangeRateSolver = new ExchangeRateSolver(exchangeRates);
            _transactionCalculator = new TransactionCalculator(exchangeRateSolver);
            _dividendCalculator = new DividendCalculator(exchangeRateSolver);
            _feeCalculator = new FeeCalculator(exchangeRateSolver);
        }

        public Report GetReport(YearOperations yearOperations)
        {
            var financialInstrumentBalances =
                _transactionCalculator.Calculate(yearOperations.AllTransactions, yearOperations.Year);
            
            var dividends = _dividendCalculator.Calculate(yearOperations.YearDividends);

            var fees = _feeCalculator.Calculate(yearOperations.YearFees);

            return GenerateReport(financialInstrumentBalances, dividends, fees);
        }

        private Report GenerateReport(List<FinancialInstrumentBalance> financialInstrumentBalances, List<PitDividend> dividends, List<PitFee> fees)
        {
             return new Report
            {
                GeneratorVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                GenerationDateTime = DateTime.Now,
                Pit38Report = new Pit38Report
                {
                    C22 = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Sum(au => au.GetIncome())).Round2(),
                    C23 = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Sum(au =>  au.GetTotalCost())).Round2()
                          //All fees are taken as the "cost of income"
                          + fees.Sum(f => f.Amount * f.PitExchangeRate).Round2(),
                    G45 = dividends.Sum(d => d.PolishTaxAmount * d.PitExchangeRate).Round2(),
                    G46 = dividends.Sum(d => d.PaidTaxAmount * d.PitExchangeRate).Round2(),
                    G47 = dividends.Sum(d => d.PitTaxAmount * d.PitExchangeRate).Round2(),
                    PitZGs = financialInstrumentBalances.GroupBy(fib => fib.StockExchangeCountry)
                        .Where(fib => fib.Key != Country.Poland)
                        .Select(g => new PitZG
                    {
                        Country = g.Key,
                        C3_32 = g.Sum(fib => fib.AllUnitsClosedInYear.Sum(au => au.GetProfit())).Round2(),
                        //Assumging there was no Tax taken from the transaction profit
                        C3_33 = 0m
                    })
                },
                ConsolidatedReport = new ConsolidatedReport
                {
                    ReportCurrency = Models.Currency.PLN,
                    FeesSum = fees.Sum(_ => _.Amount * _.PitExchangeRate).Round2(),
                    FeesSumByType = fees.GroupBy(_ => _.FeeType).ToDictionary(_ => _.Key, _ => _.Sum(_ => _.Amount.Round2())),
                    DividendsSum = new DividendSummary
                    {
                        Income = dividends.Sum(_ => _.Amount * _.PitExchangeRate).Round2(),
                        TaxPaidViaBrooker = dividends.Sum(_ => _.PaidTaxAmount * _.PitExchangeRate).Round2(),
                        PolishTaxAmount = dividends.Sum(_ => _.PolishTaxAmount * _.PitExchangeRate).Round2(),
                        TaxToBePaidViaPit = dividends.Sum(_ => _.PitTaxAmount * _.PitExchangeRate).Round2()
                    },
                    DividendsSumByFinancialInstrument = dividends.GroupBy(_ => _.FinancialInstrumentReference)
                        .Select(d => new InstrumentDividendSummary
                        {
                            FinancialInstrumentCommonName = d.Select(_ => _.FinancialInstrumentCommonName).Distinct().Single(),
                            FinancialInstrumentReference = d.Key,
                            Income = d.Sum(_ => _.Amount * _.PitExchangeRate).Round2(),
                            TaxPaidViaBrooker = d.Sum(_ => _.PaidTaxAmount * _.PitExchangeRate).Round2(),
                            PolishTaxAmount = d.Sum(_ => _.PolishTaxAmount * _.PitExchangeRate).Round2(),
                            TaxToBePaidViaPit = d.Sum(_ => _.PitTaxAmount * _.PitExchangeRate).Round2()
                        }),
                    TransactionsSum =  new TransactionSummary
                    {
                        TransactionProfitExcludingFees = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Sum(au => au.GetProfit() + au.GetFeesCost())),
                        TransactionProfitIncludingFees = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Sum(au => au.GetProfit())),
                        FeesSum = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Sum(au => au.GetFeesCost()))
                    },
                    TransactionsProfitByFinancialInstrument = financialInstrumentBalances.Select(fib => new InstrumentTransactionSummary
                    {
                        TransactionProfitExcludingFees = fib.AllUnitsClosedInYear.Sum(au => au.GetProfit() + au.GetFeesCost()),
                        TransactionProfitIncludingFees = fib.AllUnitsClosedInYear.Sum(au => au.GetProfit()),
                        FeesSum = fib.AllUnitsClosedInYear.Sum(au => au.GetFeesCost()),
                        FinancialInstrumentCommonName = fib.FinancialInstrumentCommonName,
                        FinancialInstrumentReference = fib.FinancialInstrumentReference
                    })
                },
                Fees = fees,
                Dividends = dividends,
                FinancialInstrumentBalances = financialInstrumentBalances
            };
        }
    }
}
