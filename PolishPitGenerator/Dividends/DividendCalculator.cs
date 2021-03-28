using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Models.ExchangeRate;
using Models.Operations;
using PolishPitGenerator.Dividends.Models;

namespace PolishPitGenerator.Dividends
{
    internal interface IDividendCalculator
    {
        List<PitDividend> Calculate(IEnumerable<Dividend> dividends);
    }

    internal class DividendCalculator : IDividendCalculator
    {
        private readonly IExchangeRateSolver _exchangeRateSolver;
        private readonly string[] LuxemburgIsinsOnWse =
        {
            "LU2237380790",
            "LU0646112838",
            "LU0122624777",
            "LU0607203980",
            "LU0327357389",
            "LU0611262873",
            "LU1642887738",
            "LU0564351582"
        };

        public DividendCalculator(IExchangeRateSolver exchangeRateSolver)
        {
            _exchangeRateSolver = exchangeRateSolver;
        }

        public List<PitDividend> Calculate(IEnumerable<Dividend> dividends)
        {
            var pitDividends = new List<PitDividend>();

            foreach (var dividend in dividends)
            {
                var polishTaxAmount = LuxemburgIsinsOnWse.Contains(dividend.FinancialInstrumentReference) ?
                        dividend.PaidTaxAmount : dividend.Amount * PolishConstants.PolishTaxRate;

                pitDividends.Add(
                    new PitDividend(dividend)
                    {
                        PitExchangeRate = _exchangeRateSolver.GetNbpExchangeRate(dividend.Currency, dividend.Date),
                        //The code assumes that W-8BEN has been completed by a user
                        PitTaxAmount = Math.Max(0, (polishTaxAmount - dividend.PaidTaxAmount).Round2()),
                        PolishTaxAmount = polishTaxAmount.Round2()
                    });
            }

            return pitDividends;
        }
    }
}
