using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.ServiceAgents.Services
{
   public class ProductAssociatedBlobService : IProductAssociatedBlobService
    {
       private readonly IServiceAgent _serviceAgent;
         private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private Dictionary<string, string> parameters = null;
        private string _log = string.Empty;

        public ProductAssociatedBlobService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent, ILocalSyncDigestRepository localSyncDigestRepository, IProductAssociatedBlobsRepository productAssociatedBlobsRepository)
        {

            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
        }


        public async Task StartProductAssoicatedBlobsSyncProcess()
        {
            try
            {
                Constants.IsSyncAvailable = await _localSyncDigestService.SyncLocalDigest(Constants.BlobsEntity, Constants.syncDigestQueryEntity);
                if (Constants.IsSyncAvailable)
                {
                    await _localSyncDigestService.SyncLocalSource(Constants.BlobsEntity, Constants.syncSourceQueryEntity);
                    await SyncProductAssociatedBlobs();
                }
            }
         
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// makes call to BuildAndSendRequest method to make service call to get ProductAssociatedBlobs data.
        /// Once we get the response converts it into JsonObject.
        /// This call is looped internally to get the subsequent ProductAssociatedBlobs batch's data
        /// by updating LastRecordId parameter everytime when we make request to get next batch data.
        /// This loop will run till the completion of ProductAssociatedBlobs data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncProductAssociatedBlobs()
        {
            try
            {
                LocalSyncDigest digest = await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.BlobsEntity);
                parameters = new Dictionary<string, string>();
                if (digest != null)
                {
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", digest.LastRecordId);
                }
                else
                {
                    digest = new LocalSyncDigest();
                    digest.SDataEntity = Constants.BlobsEntity;
                    digest.localTick = 0;
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", null);                    
                }
                parameters.Add("Count", "100");
                parameters.Add("where", "App eq 'Sales'");
                HttpResponseMessage productAssociatedBlobsResponse = null;
                productAssociatedBlobsResponse = await _serviceAgent.BuildAndSendRequest(Constants.BlobsEntity, Constants.syncQueryEntity, null, Constants.AccessToken, parameters);
                if (productAssociatedBlobsResponse!=null && productAssociatedBlobsResponse.IsSuccessStatusCode)
                {
                    var sDataProductAssociatedBlobs = await _serviceAgent.ConvertTosDataObject(productAssociatedBlobsResponse);
                    if (Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults")) > DataAccessUtils.ProductAssociatedBlobsTotalCount)
                        DataAccessUtils.ProductAssociatedBlobsTotalCount = Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults"));

                    int _totalCount = Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults"));
                    JsonArray categoriesObject = sDataProductAssociatedBlobs.GetNamedArray("$resources");
                    int _returnedCount = categoriesObject.Count;
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0)
                    {
                        var lastProductAssociatedBlobObject = categoriesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastProductAssociatedBlobObject.GetNamedString("$key");
                        int _syncEndpointTick = Convert.ToInt32(lastProductAssociatedBlobObject.GetNamedNumber("SyncEndpointTick"));
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves ProductAssociatedBlobs data into LocalDB
                        await _productAssociatedBlobsRepository.SaveProductAssociatedBlobsAsync(sDataProductAssociatedBlobs, digest);
                        // Looping this method again to make request for next batch of records(ProductAssociatedBlobs).
                        await SyncProductAssociatedBlobs();
                    }
                    else
                    {
                        DataAccessUtils.ProductAssociatedBlobsTotalCount = 0;
                        DataAccessUtils.ProductAssociatedBlobsReturnedCount = 0;
                        return;
                    }
                }

            }
            catch (SQLite.SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (TaskCanceledException ex)
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
