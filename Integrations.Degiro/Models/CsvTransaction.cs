using CsvHelper.Configuration.Attributes;

namespace Integrations.Degiro.Models
{
    public class CsvTransaction
    {
        [Index(0)]
        public string Date { get; set; }

        [Index(1)]
        public string Time { get; set; }

        [Index(2)]
        public string InstrumentName { get; set; }

        [Index(3)]
        public string Isin { get; set; }

        [Index(4)]
        public string StockExchangeName { get; set; }

        [Index(5)]
        public string StockLocation { get; set; }

        [Index(6)]
        public int? Quantity { get; set; }

        [Index(7)]
        public decimal? UnitPrice { get; set; }

        [Index(8)]
        public string UnitPriceCurrency { get; set; }

        [Index(9)]
        public decimal? LocalValue { get; set; }

        [Index(10)]
        public string LocalCurrency { get; set; }

        [Index(11)]
        public decimal? DegiroAmount { get; set; }

        [Index(12)]
        public string DegiroCurrency { get; set; }

        [Index(13)]
        public decimal? ExchangeRate { get; set; }

        [Index(14)]
        public decimal? FeeAmount { get; set; }

        [Index(15)]
        public string FeeCurrency { get; set; }

        [Index(16)]
        public decimal? TotalAmount { get; set; }

        [Index(17)]
        public string TotalAmountCurrency { get; set; }

        [Index(18)]
        public string TransactionId { get; set; }

    }
}
