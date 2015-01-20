using System.Threading.Tasks;
using SQLite;

namespace SageMobileSales.DataAccess
{
    public interface IDatabase
    {
        Task Initialize();
        SQLiteAsyncConnection GetAsyncConnection();
        Task Delete();
        //Task<bool> isDBFilePresent();
    }
}