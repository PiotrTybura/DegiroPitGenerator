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

        public decimal GetProfit() => (GetIncome() - GetTotalCost()).Round2();

        public decimal GetIncome()
        {
            var incomeEvent = GetIncomeEvent();
            return (incomeEvent.UnitPrice * incomeEvent.UnitPitExchangeRate).Round2();
        }

        public decimal GetTotalCost()
        {
            var costEvent = GetCostEvent();
            
            var totalCost = (costEvent.UnitPrice * costEvent.UnitPitExchangeRate).Round2() + GetFeesCost();

            return totalCost.Round2();
        }

        public decimal GetFeesCost()
        {
            var incomeEvent = GetIncomeEvent();
            var costEvent = GetCostEvent();

            var cost = (incomeEvent.Fee * incomeEvent.FeePitExchangeRate).Round2()
                       + (costEvent.Fee * costEvent.FeePitExchangeRate).Round2();

            return cost.Round2();
        }

    }
}
