using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IOrderLineItemRepository
    {
        Task SaveOrderLineItemsAsync(JsonObject sDataOrder, string orderId);
        Task<List<LineItemDetails>> GetOrderLineItemDetailsAsync(string orderId);
        Task<List<OrderLineItem>> GetPreviouslyPurchasedProducts(string customerId);
        Task<List<OrderLineItem>> GetOrderLineItemForOrder(string orderId);
    }
}