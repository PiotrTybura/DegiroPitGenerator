using CsvHelper.Configuration.Attributes;

namespace Integrations.Exante.Models
{
    public class CsvTransaction
    {
        [Index(0)]
        public string Time { get; set; }

        [Index(1)]
        public string AccountId { get; set; }

        [Index(2)]
        public string Side { get; set; }

        [Index(3)]
        public string SymbolId { get; set; }

        [Index(4)]
        public string Isin { get; set; }

        [Index(5)]
        public string Type { get; set; }

        [Index(6)]
        public decimal Price { get; set; }

        [Index(7)]
        public string Currency { get; set; }

        //CFD transactions may have a portion of unit
        [Index(8)]
        public decimal Quantity { get; set; }

        [Index(9)]
        public decimal Commission { get; set; }

        [Index(10)]
        public string CommissionCurrency { get; set; }

        [Index(11)]
        public decimal PnL { get; set; }

        [Index(12)]
        public decimal TradedVolume { get; set; }

        [Index(13)]
        public string OrderId { get; set; }

        [Index(14)]
        public uint OrderPos { get; set; }

        [Index(15)]
        public string ValueDate { get; set; }

        [Index(16)]
        public string UniqueTransactionIdentifier { get; set; }

        [Index(17)]
        public string TradeType { get; set; }
    }
}
