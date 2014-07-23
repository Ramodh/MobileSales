using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IProductCategoryService
    {
        Task StartCategorySyncProcess();
    }
}