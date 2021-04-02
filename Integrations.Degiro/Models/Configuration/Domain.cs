using System.Collections.Generic;
using System.Globalization;
using Models.Operations;

namespace Integrations.Degiro.Models.Configuration
{
    public class Domain
    {
        public Translations Translations { get; set; }
        public Dictionary<string, string> CurrenciesMapping { get; set; }
        public Dictionary<string, Country> StockCountriesMapping { get; set; }
        public string ReportsIsoLanguageCode { get; set; }
    }
}