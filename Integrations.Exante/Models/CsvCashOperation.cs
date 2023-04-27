using System;
using CsvHelper.Configuration.Attributes;

namespace Integrations.Exante.Models
{
    public class CsvCashOperation
    {
        [Index(0)]
        public string TransactionId { get; set; }

        [Index(1)]
        public string AccountId { get; set; }

        [Index(2)]
        public string SymbolId { get; set; }

        [Index(3)]
        public string Isin { get; set; }

        [Index(4)]
        public string OperationType { get; set; }

        [Index(5)]
        public string When { get; set; }

        [Index(6)]
        public decimal? Sum { get; set; }

        [Index(7)]
        public string Asset { get; set; }

        [Index(8)]
        public decimal? EurEquivalent { get; set; }

        [Index(9)]
        public string Comment { get; set; }

        [Index(10)]
        public string Uuid { get; set; }

        [Index(11)]
        public string ParentUuid { get; set; }

        public DateTime Date
        {
            get
            {
                return DateTime.Parse(When);
            }
        }
    }
}
