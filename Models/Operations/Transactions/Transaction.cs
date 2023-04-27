using System;

namespace Models.Operations.Transactions
{
    public class Transaction
    {
        public string Id { get; set; }
        public string FinancialInstrumentReference { get; set; }
        public DateTime Date { get; set; }
        public TransactionType TransactionType { get; set; }
        //Can be negative if transaction type is SELL
        public int Quantity { get; set; }
        public decimal TransactionPrice { get; set; }
        public Currency TransactionCurrency { get; set; }
        //Should be negative
        public decimal Fee { get; set; }
        public Currency? FeeCurrency { get; set; }
        public Country StockExchangeCountry { get; set; }
        public string FinancialInstrumentCommonName { get; set; }
    }
}
