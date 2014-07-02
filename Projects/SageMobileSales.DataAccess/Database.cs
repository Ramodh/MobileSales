using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SageMobileSales.DataAccess
{
    public class Database : IDatabase
    {

        private readonly SQLiteAsyncConnection _dbConnection;
        private string _dbName = "SageMobileSales.db";
        private string _log = string.Empty;
      
      

        public Database()
        {

            string datbasePath = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + _dbName;
            _dbConnection = new SQLiteAsyncConnection(datbasePath);

        }

        public async Task Initialize()
        {
            await _dbConnection.CreateTablesAsync<SalesRep, LocalSyncDigest, ProductCategory, ProductCategoryLink, ProductRelatedItem>();
            await _dbConnection.CreateTablesAsync<Product, ProductAssociatedBlob, Customer, Address, Contact>();
            await _dbConnection.CreateTablesAsync<Quote, QuoteLineItem, Orders, OrderLineItem, Tenant>();            
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return _dbConnection;
        }

        /// <summary>
        /// Deletes local database
        /// </summary>
        /// <returns></returns>
        public async Task Delete()
        {
            try
            {
                SQLite.SQLiteConnectionPool.Shared.Reset();
               
                StorageFile sageSalesDBFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(_dbName) as StorageFile;
               
                if (sageSalesDBFile != null)
                {
                    sageSalesDBFile= await ApplicationData.Current.LocalFolder.GetFileAsync(_dbName);
                    await sageSalesDBFile.DeleteAsync();
                }
                else
                {
                    return;
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (UnauthorizedAccessException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
           
        }
    }
}
