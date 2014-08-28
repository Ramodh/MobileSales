using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ISalesHistoryRepository
    {
        Task SaveSalesHistoryAsync(JsonObject sDataSalesHistory);
        Task<List<SalesHistory>> GetCustomerProductSalesHistory(string CustomerId, string ProductId);
    }
}
