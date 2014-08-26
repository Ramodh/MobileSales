using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IOrderRepository
    {
        Task SaveOrdersAsync(JsonObject sDataOrders, LocalSyncDigest localSyncDigest);
        Task<Orders> SaveOrderAsync(JsonObject sDataOrder);
        Task<List<OrderDetails>> GetOrdersForCustomerAsync(string customerId, bool isCameFrom);
        Task<OrderDetails> GetOrderDetailsAsync(string orderId);
        Task<List<OrderDetails>> GetOrdersListAsync(string salesRepId);
    }
}