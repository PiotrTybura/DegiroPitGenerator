using System;
using System.Collections.Generic;
using Models.ExchangeRate;
using Models.Operations.Fees;
using PolishPitGenerator.Fees.Models;

namespace PolishPitGenerator.Fees
{
    internal interface IFeeCalculator
    {
        List<PitFee> Calculate(IEnumerable<Fee> fee);
    }

    internal class FeeCalculator : IFeeCalculator
    {
        private readonly IExchangeRateSolver _exchangeRateSolver;

        public FeeCalculator(IExchangeRateSolver exchangeRateSolver)
        {
            _exchangeRateSolver = exchangeRateSolver;
        }

        public List<PitFee> Calculate(IEnumerable<Fee> fees)
        {
            var pitFees = new List<PitFee>();

            foreach (var fee in fees)
            {
                pitFees.Add(
                    new PitFee(fee)
                    {
                        PitExchangeRate = _exchangeRateSolver.GetNbpExchangeRate(fee.Currency, fee.Date),
                    });
            }

            return pitFees;
        }
    }
}
