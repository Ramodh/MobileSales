using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IFrequentlyPurchasedItemRepository
    {
        Task SaveFrequentlyPurchasedItemsAsync(JsonObject sDataFrequentlyPurchasedItem);
        Task<List<FrequentlyPurchasedItem>> GetFrequentlyPurchasedItems(string customerId);
    }
}
