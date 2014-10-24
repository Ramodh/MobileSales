using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ISalesHistoryService
    {
        Task SyncSalesHistory(string customerId, string itemId);
    }
}