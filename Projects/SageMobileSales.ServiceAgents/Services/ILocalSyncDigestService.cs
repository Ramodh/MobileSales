using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ILocalSyncDigestService
    {
        Task<bool> SyncLocalDigest(string entity, string queryEntity);
        Task<bool> SyncLocalSource(string entity, string queryEntity);
    }
}