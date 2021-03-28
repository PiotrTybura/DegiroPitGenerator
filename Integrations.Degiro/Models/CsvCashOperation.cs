using CsvHelper.Configuration.Attributes;

namespace Integrations.Degiro.Models
{
    public class CsvCashOperation
    {
        [Index(0)]
        public string ExecutionDate { get; set; }

        [Index(1)]
        public string ExecutionTime { get; set; }

        [Index(2)]
        public string Date { get; set; }

        [Index(3)]
        public string Product { get; set; }

        [Index(4)]
        public string Isin { get; set; }

        [Index(5)]
        public string Description { get; set; }

        [Index(6)]
        public decimal? ExchangeRate { get; set; }

        [Index(7)]
        public string ChangeCurrency { get; set; }

        [Index(8)]
        public decimal? ChangeAmount { get; set; }

        [Index(9)]
        public string BalanceCurrency { get; set; }

        [Index(10)]
        public decimal? BalanceAmount { get; set; }

        [Index(11)]
        public string TransactionId { get; set; }
    }
}
