using System;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite;

namespace SageMobileSales.DataAccess
{
    public class DataContext : IDataContext
    {
        private static readonly string _dbName = "SageMobileSales.db";
        private readonly IDatabase _database;

        public DataContext(IDatabase database)
        {
            _database = database;
        }

        /// <summary>
        ///     Initialize's database
        /// </summary>
        /// <returns>database</returns>
        public async Task<SQLiteAsyncConnection> InitializeDatabase()
        {
            await _database.Initialize();
            return _database.GetAsyncConnection();
        }

        /// <summary>
        ///     Deletes local database
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDatabase()
        {
            SQLiteConnectionPool.Shared.Reset();
            var sageSalesDBFile = await ApplicationData.Current.LocalFolder.GetFileAsync(_dbName);
            await sageSalesDBFile.DeleteAsync();
        }
    }
}