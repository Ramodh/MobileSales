using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ITenantRepository
    {
        Task SaveTenantDtlsAsync(JsonObject sDataTenantDtls);
        Task<Tenant> GetTenantDtlsAsync(string tenantId);
    }
}