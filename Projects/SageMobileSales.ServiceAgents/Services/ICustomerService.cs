using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ICustomerService
    {
        Task StartCustomersSyncProcess();
    }
}