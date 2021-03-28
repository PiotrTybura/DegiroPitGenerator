using System.Collections.Generic;
using Models;
using Models.Operations.Fees;

namespace PolishPitGenerator.Reports.Models
{
    public class ConsolidatedReport
    {
        public Currency ReportCurrency { get; internal set; }
        public decimal FeesSum { get; internal set; }
        public Dictionary<FeeType, decimal> FeesSumByType { get; internal set; }
        public DividendSummary DividendsSum { get; internal set; }
        public IEnumerable<DividendSummary> DividendsSumByFinancialInstrument { get; internal set; }
        public TransactionSummary TransactionsSum { get; internal set; }
        public object TransactionsIncomeByFinancialInstrument { get; internal set; }
    }
}