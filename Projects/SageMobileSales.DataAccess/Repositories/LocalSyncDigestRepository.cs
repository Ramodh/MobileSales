using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.DataAccess.Repositories
{
    public class LocalSyncDigestRepository : ILocalSyncDigestRepository
    {
        private SQLiteAsyncConnection _sageSalesDB;
        private readonly IDatabase _database;
        private string _log = string.Empty;

        public LocalSyncDigestRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        /// <summary>
        /// Extracts required data from SData(JsonObject) and then save's that data into LocalDB
        /// </summary>
        /// <param name="sDataLocalDigest"></param>
        /// <returns></returns>
        public async Task<LocalSyncDigest> SaveLocalSyncDigestDtlsAsync(JsonObject sDataLocalDigest)
        {
            LocalSyncDigest _localSyncDigest = new LocalSyncDigest();
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
        /// Update LocalSyncDigest data into LocalDB
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
        /// Delete LocalSyncDigest data from LocalDB
        /// </summary>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task DeleteLocalSyncDigestDtlsAsync(LocalSyncDigest localSyncDigest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets LocalSyncDigest data from LocalDB
        /// </summary>
        /// <param name="sDataEntity"></param>
        /// <returns></returns>
        public async Task<LocalSyncDigest> GetLocalSyncDigestDtlsAsync(string sDataEntity)
        {
            try
            {
                    var result = await _sageSalesDB.QueryAsync<LocalSyncDigest>(string.Format("select * from LocalSyncDigest where SDataEntity='{0}'", sDataEntity));
                    return result.FirstOrDefault();

            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }
    }
}
