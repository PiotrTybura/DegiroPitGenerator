using System;
using System.Collections.Generic;
using System.Text;
using Integrations.Interfaces;

namespace Integrations.Exante
{
    class ExanteCsv<T> : Csv<T>
    {
        public ExanteCsv(string csv) : base(Fix(csv))
        {
        }

        private static string Fix(string csv)
        {
            csv = csv.Replace("\"\t\"", ",");
            csv = csv.Replace("\"\n\"", "\n");
            csv = csv.Trim('\"');
            csv = csv.TrimEnd('\"');

            return csv;
        }
    }
}
