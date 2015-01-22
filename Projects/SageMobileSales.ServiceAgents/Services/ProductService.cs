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
    public class ProductService : IProductService
    {
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IProductRepository _productRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

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
                var digest =
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
                ErrorLog("Product local tick : " + digest.localTick);
                parameters.Add("Count", "100");
                parameters.Add("include", "AssociatedCategories&select=*,AssociatedCategories/Id");
                HttpResponseMessage productsResponse = null;
                productsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.ItemsEntity,
                            Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (productsResponse != null && productsResponse.IsSuccessStatusCode)
                {
                    var sDataProducts = await _serviceAgent.ConvertTosDataObject(productsResponse);
                    if (Convert.ToInt32(sDataProducts.GetNamedNumber("$totalResults")) >
                        DataAccessUtils.ProductTotalCount)
                        DataAccessUtils.ProductTotalCount =
                            Convert.ToInt32(sDataProducts.GetNamedNumber("$totalResults"));

                    var _totalCount = Convert.ToInt32(sDataProducts.GetNamedNumber("$totalResults"));
                    ErrorLog("Product total count : " + _totalCount);
                    var categoriesObject = sDataProducts.GetNamedArray("$resources");
                    var _returnedCount = categoriesObject.Count;
                    ErrorLog("Product returned count : " + _returnedCount);
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsProductSyncCompleted))
                    {
                        var lastProductObject = categoriesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastProductObject.GetNamedString("$key");
                        var _syncEndpointTick = Convert.ToInt32(lastProductObject.GetNamedNumber("SyncTick"));
                        ErrorLog("Product sync tick : " + _syncEndpointTick);
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