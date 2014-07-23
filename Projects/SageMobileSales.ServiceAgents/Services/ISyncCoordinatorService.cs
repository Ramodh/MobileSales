using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ISyncCoordinatorService
    {
        Task StartSync();
        Task StartProductsSync();
        Task StartQuotesSync();
        Task StartOrdersSync();
    }
}