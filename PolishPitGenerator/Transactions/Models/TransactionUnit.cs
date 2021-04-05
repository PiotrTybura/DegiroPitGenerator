using Models.Operations.Transactions;

namespace PolishPitGenerator.Transactions.Models
{
    public class TransactionUnit
    {
        public TransactionEvent OpenEvent { get; internal set; }
        public TransactionEvent CloseEvent { get; internal set; }
        public TransactionType TransactionType { get; internal set; }

        public TransactionEvent GetIncomeEvent() => TransactionType == TransactionType.BUY ? CloseEvent : OpenEvent;

        public TransactionEvent GetCostEvent() => TransactionType == TransactionType.BUY ? OpenEvent : CloseEvent;
    }
}
