using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IProductAssociatedBlobsRepository
    {
        Task SaveProductAssociatedBlobsAsync(JsonObject sDataProductAssociatedBlobs, LocalSyncDigest localSyncDigest);
        Task UpdatProductAssociatedBlobAsync(JsonObject productAssociatedBlobJsonObject);
        Task<List<ProductAssociatedBlob>> GetProductAssociatedBlobsAsync(string productId);
    }
}