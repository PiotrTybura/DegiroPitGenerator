using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Models.ExchangeRate;

namespace PolishPitGenerator
{
    internal interface IExchangeRateSolver
    {
        decimal GetNbpExchangeRate(Currency currency, DateTime date);
    }

    internal class ExchangeRateSolver : IExchangeRateSolver
    {
        private readonly IList<ExchangeRate> _exchangeRates;

        internal ExchangeRateSolver(IEnumerable<ExchangeRate> exchangeRates)
        {
            //Exchange Rates are ordered to find "previous working day" faster
            _exchangeRates = exchangeRates.OrderByDescending(_ => _.Date).ToList();
        }

        public decimal GetNbpExchangeRate(Currency currency, DateTime date) =>
            currency == Currency.PLN ? 1 : _exchangeRates.First(_ => _.BaseCurrency == currency && _.Date < date).Rate;
    }
}
