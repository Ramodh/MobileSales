using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ISalesHistoryRepository
    {
        Task SaveSalesHistoryAsync(JsonObject sDataSalesHistory);
        Task<List<SalesHistory>> GetCustomerProductSalesHistory(string CustomerId, string ProductId);
    }
}