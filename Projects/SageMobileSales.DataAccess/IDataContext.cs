using System.Threading.Tasks;
using SQLite;

namespace SageMobileSales.DataAccess
{
    public interface IDataContext
    {
        Task<SQLiteAsyncConnection> InitializeDatabase();
        Task DeleteDatabase();
    }
}