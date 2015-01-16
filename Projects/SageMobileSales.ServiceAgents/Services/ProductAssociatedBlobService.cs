using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class ProductAssociatedBlobService : IProductAssociatedBlobService
    {
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public ProductAssociatedBlobService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent,
            ILocalSyncDigestRepository localSyncDigestRepository,
            IProductAssociatedBlobsRepository productAssociatedBlobsRepository)
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
                Constants.IsSyncAvailable =
                    await
                        _localSyncDigestService.SyncLocalDigest(Constants.BlobsEntity, Constants.syncDigestQueryEntity);
                if (Constants.IsSyncAvailable)
                {
                    await
                        _localSyncDigestService.SyncLocalSource(Constants.BlobsEntity, Constants.syncSourceQueryEntity);
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
        ///     makes call to BuildAndSendRequest method to make service call to get ProductAssociatedBlobs data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent ProductAssociatedBlobs batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of ProductAssociatedBlobs data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncProductAssociatedBlobs()
        {
            try
            {
                var digest =
                    await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.BlobsEntity);
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
                ErrorLog("Product Associated Blobs local tick : " + digest.localTick);
                parameters.Add("Count", "100");
                HttpResponseMessage productAssociatedBlobsResponse = null;
                productAssociatedBlobsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.BlobsEntity,
                            Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (productAssociatedBlobsResponse != null && productAssociatedBlobsResponse.IsSuccessStatusCode)
                {
                    var sDataProductAssociatedBlobs =
                        await _serviceAgent.ConvertTosDataObject(productAssociatedBlobsResponse);
                    if (Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults")) >
                        DataAccessUtils.ProductAssociatedBlobsTotalCount)
                        DataAccessUtils.ProductAssociatedBlobsTotalCount =
                            Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults"));

                    var _totalCount = Convert.ToInt32(sDataProductAssociatedBlobs.GetNamedNumber("$totalResults"));
                    ErrorLog("Product Associated Blobs total count : " + _totalCount);
                    var categoriesObject = sDataProductAssociatedBlobs.GetNamedArray("$resources");
                    var _returnedCount = categoriesObject.Count;
                    ErrorLog("Product Associated Blobs returned count : " + _returnedCount);
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0)
                    {
                        var lastProductAssociatedBlobObject =
                            categoriesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastProductAssociatedBlobObject.GetNamedString("$key");
                        var _syncEndpointTick =
                            Convert.ToInt32(lastProductAssociatedBlobObject.GetNamedNumber("SyncTick"));
                        ErrorLog("Product Associated Blobs sync tick : " + _syncEndpointTick);
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves ProductAssociatedBlobs data into LocalDB
                        await
                            _productAssociatedBlobsRepository.SaveProductAssociatedBlobsAsync(
                                sDataProductAssociatedBlobs, digest);
                        // Looping this method again to make request for next batch of records(ProductAssociatedBlobs).
                        await SyncProductAssociatedBlobs();
                    }
                    else
                    {
                        DataAccessUtils.ProductAssociatedBlobsTotalCount = 0;
                        DataAccessUtils.ProductAssociatedBlobsReturnedCount = 0;
                    }
                }
            }
            catch (SQLiteException ex)
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

        /// <summary>
        ///     Error log
        /// </summary>
        /// <param name="message"></param>
        private void ErrorLog(string message)
        {
            AppEventSource.Log.Info(message);
        }
    }
}