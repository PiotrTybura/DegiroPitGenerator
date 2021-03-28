using System.Collections.Generic;
using Models.Operations;

namespace PolishPitGenerator.Transactions.Models
{
    public class FinancialInstrumentBalance
    {
        public string FinancialInstrumentReference { get; internal set; }
        public int UnitBalance { get; internal set; }
        public List<TransactionUnit> TransactionUnits { get; internal set; }
        public Country StockExchangeCountry { get; internal set; }
        public List<TransactionUnit> AllUnitsClosedInYear { get; internal set; }
        public string FinancialInstrumentCommonName { get; internal set; }
    }
}
