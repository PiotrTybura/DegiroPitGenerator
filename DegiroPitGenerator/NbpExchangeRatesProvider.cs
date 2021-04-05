using System.Collections.Generic;
using System.Threading.Tasks;
using Integrations.Nbp.Models;
using Models.ExchangeRate;

namespace DegiroPitGenerator
{
    internal class NbpExchangeRatesProvider
    {
        internal async Task<IEnumerable<ExchangeRate>> GetExchangeRates()
        {
            var nbpConfiguration = Configuration.GetSection<NbpConfiguration>();

            var nbpIntegration = await Integrations.Nbp.IntegrationFactory.Create(nbpConfiguration);
            return nbpIntegration.GetExchangeRates();
        }
    }
}
