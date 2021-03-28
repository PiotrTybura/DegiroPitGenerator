namespace PolishPitGenerator.Reports.Models
{
    public class InstrumentTransactionSummary : TransactionSummary
    {
        public string FinancialInstrumentCommonName { get; set; }
        public string FinancialInstrumentReference { get; set; }
    }
}
