using System.Threading.Tasks;
using Integrations.Nbp.Models;

namespace Integrations.Nbp
{
    public class IntegrationFactory
    {
        public static Task<IIntegration> Create(NbpConfiguration configuration)
        {
            return Task.FromResult((IIntegration) new Integration(configuration));
        }
    }
}
