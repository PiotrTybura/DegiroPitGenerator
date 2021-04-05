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
                    C22 = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.UnitPrice * ie.UnitPitExchangeRate)).Round2(),
                    C23 = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au =>  au.GetCostEvent()).Sum(ce => ce.UnitPrice * ce.UnitPitExchangeRate)).Round2()
                        + financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.Fee * ie.FeePitExchangeRate)).Round2()
                        + financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.Fee * ce.FeePitExchangeRate)).Round2()
                        //All fees are taken as the "cost of income"
                        + fees.Sum(f => f.Amount * f.PitExchangeRate).Round2(),
                    G45 = dividends.Sum(d => d.PolishTaxAmount * d.PitExchangeRate).Round2(),
                    G46 = dividends.Sum(d => d.PaidTaxAmount * d.PitExchangeRate).Round2(),
                    G47 = dividends.Sum(d => d.PitTaxAmount * d.PitExchangeRate).Round2(),
                    PitZGs = financialInstrumentBalances.GroupBy(fib => fib.StockExchangeCountry).Select(g => new PitZG
                    {
                        Country = g.Key,
                        C3_32 = g.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.UnitPrice * ie.UnitPitExchangeRate)).Round2()
                            - g.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.UnitPrice * ce.UnitPitExchangeRate)).Round2()
                            - g.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.Fee * ie.FeePitExchangeRate)).Round2()
                            - g.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.Fee * ce.FeePitExchangeRate)).Round2(),
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
                        Profit = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.UnitPrice * ie.UnitPitExchangeRate)).Round2()
                            - financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.UnitPrice * ce.UnitPitExchangeRate)).Round2(),
                        FeesSum = financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.Fee * ie.FeePitExchangeRate)).Round2()
                            + financialInstrumentBalances.Sum(fib => fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.Fee * ce.FeePitExchangeRate)).Round2(),
                    },
                    TransactionsProfitByFinancialInstrument = financialInstrumentBalances.Select(fib => new InstrumentTransactionSummary
                    {
                        Profit = fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.UnitPrice * ie.UnitPitExchangeRate).Round2()
                            - fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.UnitPrice * ce.UnitPitExchangeRate).Round2(),
                        FeesSum = fib.AllUnitsClosedInYear.Select(au => au.GetIncomeEvent()).Sum(ie => ie.Fee * ie.FeePitExchangeRate).Round2()
                            + fib.AllUnitsClosedInYear.Select(au => au.GetCostEvent()).Sum(ce => ce.Fee * ce.FeePitExchangeRate).Round2(),
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
