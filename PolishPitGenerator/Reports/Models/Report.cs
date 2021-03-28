using System;
using System.Collections.Generic;
using PolishPitGenerator.Dividends.Models;
using PolishPitGenerator.Fees.Models;
using PolishPitGenerator.Transactions.Models;

namespace PolishPitGenerator.Reports.Models
{
    public class Report
    {
        public string GeneratorVersion { get; internal set; }
        public DateTime GenerationDateTime { get; internal set; }
        public Pit38Report Pit38Report { get; internal set; }
        public ConsolidatedReport ConsolidatedReport { get; internal set; }
        public List<PitFee> Fees { get; internal set; }
        public List<PitDividend> Dividends { get; internal set; }
        public List<FinancialInstrumentBalance> FinancialInstrumentBalances { get; internal set; }
    }
}
