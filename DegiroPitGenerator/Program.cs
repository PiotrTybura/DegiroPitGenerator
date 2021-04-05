using System;
using System.Threading.Tasks;
using Integrations.Degiro.Models.Configuration;
using Microsoft.Extensions.Configuration;
using PolishPitGenerator;

namespace DegiroPitGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pitYear = int.Parse(args[0]);

            var degiroOperationsProivder = new DegiroOperationsProvider();
            var nbpExchangeRatesProvider = new NbpExchangeRatesProvider();

            var yearOperations = await degiroOperationsProivder.GetYearOperations(pitYear);

            var exchangeRates = await nbpExchangeRatesProvider.GetExchangeRates();

            var pitProvider = new PitProvider(exchangeRates);
            var pitReport = pitProvider.GetReport(yearOperations);

            FileUtilities.SaveToJsonFile("PitReport_Detailed", pitReport);
            var summaryFileName = FileUtilities.SaveToJsonFile("PitReport_Summary", pitReport.Pit38Report);

            FileUtilities.StartProcess(summaryFileName);
        }
    }
}
