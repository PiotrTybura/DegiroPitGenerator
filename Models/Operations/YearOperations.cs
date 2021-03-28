using System.Collections.Generic;
using Models.Operations.Fees;

namespace Models.Operations
{
    public class YearOperations
    {
        public int Year { get; set; }
        public IEnumerable<Transactions.Transaction> AllTransactions { get; set; }
        public IEnumerable<Dividend> YearDividends { get; set; }
        public IEnumerable<Fee> YearFees { get; set; }
    }
}
