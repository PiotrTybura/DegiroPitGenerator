namespace Integrations.Degiro.Models.Configuration
{
    public class DegiroCsvOverride
    {
        public bool? UseLocalCsvs { get; set; }
        public string TransactionsCsvPath { get; set; }
        public string CashOperationsCsvPath { get; set; }
    }
}
