using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IFrequentlyPurchasedItemService
    {
        Task SyncFrequentlyPurchasedItems(string customerId);
    }
}