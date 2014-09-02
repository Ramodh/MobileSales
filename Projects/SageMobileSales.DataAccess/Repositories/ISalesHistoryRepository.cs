using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ISalesHistoryRepository
    {
        Task SaveSalesHistoryAsync(JsonObject sDataSalesHistory);
    }
}