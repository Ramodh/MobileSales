using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class ProductService : IProductService
    {
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IProductRepository _productRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters = null;

        public ProductService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent,
            ILocalSyncDigestRepository localSyncDigestRepository, IProductRepository productRepository)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _productRepository = productRepository;
        }


        public async Task StartProductsSyncProcess()
        {
            Constants.IsSyncAvailable =
                await _localSyncDigestService.SyncLocalDigest(Constants.ItemsEntity, Constants.syncDigestQueryEntity);
            if (Constants.IsSyncAvailable)
            {
                await _localSyncDigestService.SyncLocalSource(Constants.ItemsEntity, Constants.syncSourceQueryEntity);
                await SyncProducts();
            }
        }

        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get Products data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent Products batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of Products data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncProducts()
        {
            try
            {
                LocalSyncDigest digest =
                    await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.ItemsEntity);
                parameters = new Dictionary<string, string>();
                if (digest != null)
                {
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", digest.LastRecordId);
                }
                else
                {
                    digest = new LocalSyncDigest();
                    digest.SDataEntity = Constants.ItemsEntity;
                    digest.localTick = 0;
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", null);
                }
                parameters.Add("Count", "100");
                HttpResponseMessage productsResponse = null;
                productsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.ItemsEntity, Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (productsResponse != null && productsResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataProducts = await _serviceAgent.ConvertTosDataObject(productsResponse);
                    if (Convert.ToInt32(sDataProducts.GetNamedString("$totalResults")) >
                        DataAccessUtils.ProductTotalCount)
                        DataAccessUtils.ProductTotalCount =
                            Convert.ToInt32(sDataProducts.GetNamedString("$totalResults"));

                    int _totalCount = Convert.ToInt32(sDataProducts.GetNamedString("$totalResults"));
                    JsonArray categoriesObject = sDataProducts.GetNamedArray("$resources");
                    int _returnedCount = categoriesObject.Count;
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsProductSyncCompleted))
                    {
                        JsonObject lastProductObject = categoriesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastProductObject.GetNamedString("Id");
                        int _syncEndpointTick = Convert.ToInt32(lastProductObject.GetNamedNumber("SyncTick"));
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves Products data into LocalDB
                        await _productRepository.SaveProductsAsync(sDataProducts, digest);
                        // Looping this method again to make request for next batch of records(Products).
                        await SyncProducts();
                    }
                    else
                    {
                        DataAccessUtils.ProductTotalCount = 0;
                        DataAccessUtils.ProductReturnedCount = 0;
                        DataAccessUtils.IsProductSyncCompleted = false;
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
    }
}