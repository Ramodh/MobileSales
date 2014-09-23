using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class LocalSyncDigestRepository : ILocalSyncDigestRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public LocalSyncDigestRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        # region public methods
        /// <summary>
        ///     Extracts localSyncDigest data from json, save in local dB
        /// </summary>
        /// <param name="sDataLocalDigest"></param>
        /// <returns></returns>
        public async Task<LocalSyncDigest> SaveLocalSyncDigestDtlsAsync(JsonObject sDataLocalDigest)
        {
            var _localSyncDigest = new LocalSyncDigest();
            try
            {
                _localSyncDigest.localTick = Convert.ToInt32(sDataLocalDigest.GetNamedNumber("Tick"));
                _localSyncDigest.SDataEntity = sDataLocalDigest.GetNamedString("ResourceKind");
                _localSyncDigest.TenantId = sDataLocalDigest.GetNamedString("TenantId");
                await _sageSalesDB.InsertAsync(_localSyncDigest);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _localSyncDigest;
        }

        /// <summary>
        ///     Update LocalSyncDigest data into local dB
        /// </summary>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task<LocalSyncDigest> UpdateLocalSyncDigestDtlsAsync(LocalSyncDigest localSyncDigest)
        {
            try
            {
                LocalSyncDigest digest = await GetLocalSyncDigestDtlsAsync(localSyncDigest.SDataEntity);
                if (digest != null)
                {
                    digest.LastRecordId = localSyncDigest.LastRecordId;
                    digest.localTick = localSyncDigest.localTick;
                    digest.LastSyncTime = localSyncDigest.LastSyncTime;
                    await _sageSalesDB.UpdateAsync(localSyncDigest);
                }
                else
                {
                    await _sageSalesDB.InsertAsync(localSyncDigest);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return localSyncDigest;
        }

        /// <summary>
        ///     Delete LocalSyncDigest data from LocalDB
        /// </summary>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        //public async Task DeleteLocalSyncDigestDtlsAsync(LocalSyncDigest localSyncDigest)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        ///     Gets LocalSyncDigest data from LocalDB
        /// </summary>
        /// <param name="sDataEntity"></param>
        /// <returns></returns>
        public async Task<LocalSyncDigest> GetLocalSyncDigestDtlsAsync(string sDataEntity)
        {
            try
            {
                List<LocalSyncDigest> result =
                    await
                        _sageSalesDB.QueryAsync<LocalSyncDigest>(
                            string.Format("select * from LocalSyncDigest where SDataEntity='{0}'", sDataEntity));
                return result.FirstOrDefault();
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        /// Delete localDigestSync for customer
        /// </summary>
        /// <returns></returns>
        public async Task DeleteLocalSyncDigestForCustomer()
        {
            try
            {
                List<LocalSyncDigest> result =
                    await
                        _sageSalesDB.QueryAsync<LocalSyncDigest>("DELETE FROM LocalSyncDigest WHERE SDataEntity LIKE 'MyCustomers'");

                //Change Customers to inActive
                List<Customer> customerList = await _sageSalesDB.QueryAsync<Customer>("UPDATE Customer SET IsActive='0'");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion
    }
}