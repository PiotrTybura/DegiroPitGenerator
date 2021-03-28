using System.Collections.Generic;

namespace PolishPitGenerator.Reports.Models
{
    public class Pit38Report
    {
        public decimal C22 { get; set; }
        public decimal C23 { get; set; }
        public decimal G45 { get; set; }
        public decimal G46 { get; set; }
        public decimal G47 { get; set; }
        public IEnumerable<PitZG> PitZGs { get; set; }
    }
}