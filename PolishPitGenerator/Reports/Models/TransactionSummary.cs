namespace PolishPitGenerator.Reports.Models
{
    public class TransactionSummary
    {
        public decimal TransactionProfitExcludingFees { get; set; }
        public decimal FeesSum { get; set; }
        public decimal TransactionProfitIncludingFees { get; set; }
    }
}
