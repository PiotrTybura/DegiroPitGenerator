using System;

namespace Models.ExchangeRate
{
    public class ExchangeRate
    {
        public DateTime Date { get; set; }
        public Currency BaseCurrency { get; set; }
        public Currency CounterCurrency { get; set; }
        public decimal Rate { get; set; }
    }
}