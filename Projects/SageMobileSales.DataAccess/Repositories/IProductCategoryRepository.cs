using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IProductCategoryRepository
    {
        Task SaveProductCategoryDtlsAsync(JsonObject sDataProductCategory, LocalSyncDigest localSyncDigest);

        Task<ProductCategory> UpdateProductCategoryJsonToDbAsync(JsonObject sDataProductCategory,
            ProductCategory productCategoryDbObj);

        //Task DeleteProductCategoryDtlsAsync(ProductCategory salesRep);
        Task<List<ProductCategory>> GetProductCategoryListDtlsAsync(string parentId);
        Task<bool> GetProductCategoryLevel(string parentId);
        Task<List<ProductCategory>> GetProductCategoryListAsync();
    }
}