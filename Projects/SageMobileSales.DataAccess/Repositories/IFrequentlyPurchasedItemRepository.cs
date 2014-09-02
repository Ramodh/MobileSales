using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IFrequentlyPurchasedItemRepository
    {
        Task SaveFrequentlyPurchasedItemsAsync(JsonObject sDataFrequentlyPurchasedItem);
        Task<List<FrequentlyPurchasedItem>> GetFrequentlyPurchasedItems(string customerId);
    }
}