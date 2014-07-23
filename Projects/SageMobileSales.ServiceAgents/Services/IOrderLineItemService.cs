using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IOrderLineItemService
    {
        Task SyncOrderLineItems(string orderId);
    }
}