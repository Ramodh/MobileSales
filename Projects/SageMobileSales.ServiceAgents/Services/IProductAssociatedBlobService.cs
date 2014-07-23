using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IProductAssociatedBlobService
    {
        Task StartProductAssoicatedBlobsSyncProcess();
    }
}