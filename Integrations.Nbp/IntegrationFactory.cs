using System.Threading.Tasks;

namespace Integrations.Nbp
{
    public class IntegrationFactory
    {
        public static Task<IIntegration> Create()
        {
            return Task.FromResult((IIntegration) new Integration());
        }
    }
}
