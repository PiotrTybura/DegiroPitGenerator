using System;

namespace Models.Operations.Fees
{
    public class Fee
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public DateTime Date { get; set; }
        public FeeType FeeType { get; set; }
    }
}
