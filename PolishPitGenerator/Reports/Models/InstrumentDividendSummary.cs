namespace PolishPitGenerator.Reports.Models
{
    public class InstrumentDividendSummary : DividendSummary
    {
        public string FinancialInstrumentCommonName { get; internal set; }
        public string FinancialInstrumentReference { get; internal set; }
    }
}
