using Extensions;
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

        public decimal GetProfit() => (GetIncome() - GetTotalCost());

        public decimal GetIncome()
        {
            var incomeEvent = GetIncomeEvent();

            return incomeEvent.UnitPrice * incomeEvent.UnitPitExchangeRate;
        }

        public decimal GetTotalCost()
        {
            var costEvent = GetCostEvent();
            
            return costEvent.UnitPrice * costEvent.UnitPitExchangeRate + GetFeesCost();
        }

        public decimal GetFeesCost()
        {
            var incomeEvent = GetIncomeEvent();
            var costEvent = GetCostEvent();

            return incomeEvent.Fee * incomeEvent.FeePitExchangeRate + costEvent.Fee * costEvent.FeePitExchangeRate;
        }

    }
}
