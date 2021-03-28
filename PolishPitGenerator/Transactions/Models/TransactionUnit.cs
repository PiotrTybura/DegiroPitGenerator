namespace PolishPitGenerator.Transactions.Models
{
    public class TransactionUnit
    {
        public TransactionEvent OpenEvent { get; internal set; }
        public TransactionEvent CloseEvent { get; internal set; }
    }
}
