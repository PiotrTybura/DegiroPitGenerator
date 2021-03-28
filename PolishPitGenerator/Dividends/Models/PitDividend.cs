using Models.Operations;

namespace PolishPitGenerator.Dividends.Models
{
    public class PitDividend : Dividend
    {
        public PitDividend(Dividend dividend)
        {
            FinancialInstrumentReference = dividend.FinancialInstrumentReference;
            FinancialInstrumentCommonName = dividend.FinancialInstrumentCommonName;
            Amount = dividend.Amount;
            Currency = dividend.Currency;
            Date = dividend.Date;
            PaidTaxAmount = dividend.PaidTaxAmount;
        }

        public decimal PolishTaxAmount { get; internal set; }
        public decimal PitExchangeRate { get; internal set; }
        public decimal PitTaxAmount { get; internal set; }
    }
}
