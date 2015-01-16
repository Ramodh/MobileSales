using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ILocalSyncDigestRepository
    {
        //Task<LocalSyncDigest> SaveLocalSyncDigestDtlsAsync(JsonObject localSyncDigestDtlsJson);
        Task<LocalSyncDigest> AddOrUpdateLocalSyncDigestDtlsAsync(LocalSyncDigest localSyncDigest);
        //Task DeleteLocalSyncDigestDtlsAsync(LocalSyncDigest localSyncDigest);
        Task<LocalSyncDigest> GetLocalSyncDigestDtlsAsync(string sDataEntity);
        Task DeleteLocalSyncDigestForCustomer();
    }
}