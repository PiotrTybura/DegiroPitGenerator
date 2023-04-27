using System;

namespace Models.Operations
{
    public class Dividend
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public DateTime Date { get; set; }
        public string FinancialInstrumentReference { get; set; }
        public string FinancialInstrumentCommonName { get; set; }
        //Must be positive
        public decimal PaidTaxAmount { get; set; }
    }
}
