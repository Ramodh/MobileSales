using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ITenantRepository
    {
        Task SaveTenantAsync(JsonArray sDataTenants, string repId);
        Task UpdateTenantAsync(JsonObject sDataTenant, string tenantGuid);
        Task<Tenant> GetTenantDtlsAsync(string tenantId);
        Task<string> GetTenantId();
    }
}