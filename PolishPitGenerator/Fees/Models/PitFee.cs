using Models.Operations.Fees;

namespace PolishPitGenerator.Fees.Models
{
    public class PitFee : Fee
    {
        public PitFee(Fee fee)
        {
            Amount = fee.Amount;
            Currency = fee.Currency;
            Date = fee.Date;
            FeeType = fee.FeeType;
        }

        public decimal PitExchangeRate { get; internal set; }
    }
}
