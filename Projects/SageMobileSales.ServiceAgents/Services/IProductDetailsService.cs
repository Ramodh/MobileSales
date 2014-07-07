using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IProductDetailsService
    {
        Task SyncProductDetails(string productId);
    }
}