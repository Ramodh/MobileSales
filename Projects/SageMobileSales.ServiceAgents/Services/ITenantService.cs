using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ITenantService
    {
        Task SyncTenant();
    }
}