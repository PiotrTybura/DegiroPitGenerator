using System;
using Models;

namespace PolishPitGenerator.Transactions.Models
{
    public class TransactionEvent
    {
        public DateTime Date { get; set; }
        public decimal UnitPrice { get; set; }
        public Currency UnitCurrency { get; set; }
        public decimal UnitPitExchangeRate { get; set; }
        public decimal Fee { get; set; }
        public Currency? FeeCurrency { get; set; }
        public decimal FeePitExchangeRate { get; set; }
    }
}