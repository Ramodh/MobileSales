using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IOrderService
    {
        Task StartOrdersSyncProcess();
        Task<Orders> PostOrder(Quote quote);
    }
}