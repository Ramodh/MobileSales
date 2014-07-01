using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IProductRepository
    {
        Task SaveProductsAsync(JsonObject sDataProduct, LocalSyncDigest localSyncDigest);
        Task UpdatProductsAsync(JsonObject sDataProduct);
        Task DeleteProductAsync(Product product);
        Task<List<ProductDetails>> GetCategoryProductsAsync(string categoryId);
        Task<Product> GetProductdetailsAsync(string productId);
        Task<List<ProductAssociatedBlob>> GetProductRelatedItems(string productId);
        Task<List<ProductDetails>> GetSearchSuggestionsAsync(string searchTerm);
        //Task<Product> AddOrUpdateProductFromOrderLineItem(JsonObject sDataProduct);
        Task<Product> AddOrUpdateProductJsonToDbAsync(JsonObject sDataProduct);
    }
}