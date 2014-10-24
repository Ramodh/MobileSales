using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IProductService
    {
        Task StartProductsSyncProcess();
    }
}