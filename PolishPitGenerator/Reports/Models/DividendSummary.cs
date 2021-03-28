namespace PolishPitGenerator.Reports.Models
{
    public class DividendSummary
    {
        public decimal Income { get; set; }
        public decimal TaxPaidViaBrooker { get; set; }
        public decimal PolishTaxAmount { get; set; }
        public decimal TaxToBePaidViaPit { get; set; }
    }
}
