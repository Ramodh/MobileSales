using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public ProductCategoryService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent,
            ILocalSyncDigestRepository localSyncDigestRepository, IProductCategoryRepository productCategoryRepository,
            IEventAggregator eventAggregator)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _productCategoryRepository = productCategoryRepository;
            _eventAggregator = eventAggregator;
        }


        public async Task StartCategorySyncProcess()
        {
            Constants.IsSyncAvailable =
                await _localSyncDigestService.SyncLocalDigest(Constants.CategoryEntity, Constants.syncDigestQueryEntity);
            if (Constants.IsSyncAvailable)
            {
                await _localSyncDigestService.SyncLocalSource(Constants.CategoryEntity, Constants.syncSourceQueryEntity);
                await SyncProductCategory();
            }
            else
            {
                _eventAggregator.GetEvent<ProductDataChangedEvent>().Publish(true);
            }
        }


        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get ProductCategory(Catalog's,Categories) data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent ProductCategory batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of Productcategory data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncProductCategory()
        {
            try
            {
                LocalSyncDigest digest =
                    await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.CategoryEntity);
                parameters = new Dictionary<string, string>();
                if (digest != null)
                {
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", digest.LastRecordId);
                }
                else
                {
                    digest = new LocalSyncDigest();
                    digest.SDataEntity = Constants.CategoryEntity;
                    digest.localTick = 0;
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", null);
                }
                ErrorLog("Product Category local tick : " + digest.localTick);
                parameters.Add("Count", "100");
                parameters.Add("include", "AssociatedItems");
                //parameters.Add("Parent&select", "*");
                HttpResponseMessage productCategoryResponse = null;

                Constants.syncQueryEntity = Constants.syncSourceQueryEntity + "('" + Constants.TrackingId + "')";
                // Adding syncQueryEntity to Applicationdata Container as we are using this in every Servicerequest.
                // And this will be usefull when we are doing partial sync for particular Service.
                ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
                settingsLocal.Containers["SageSalesContainer"].Values["syncQueryEntity"] = Constants.syncQueryEntity;


                productCategoryResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.CategoryEntity,
                            Constants.syncQueryEntity,
                            Constants.AssociatedItems, Constants.AccessToken, parameters);
                if (productCategoryResponse != null && productCategoryResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataProductCategory = await _serviceAgent.ConvertTosDataObject(productCategoryResponse);
                    if (Convert.ToInt32(sDataProductCategory.GetNamedNumber("$totalResults")) >
                        DataAccessUtils.ProductCategoryTotalCount)
                        DataAccessUtils.ProductCategoryTotalCount =
                            Convert.ToInt32(sDataProductCategory.GetNamedNumber("$totalResults"));
                    if (DataAccessUtils.ProductCategoryTotalCount == 0)
                    {
                        _eventAggregator.GetEvent<ProductDataChangedEvent>().Publish(true);
                    }
                    int _totalCount = Convert.ToInt32(sDataProductCategory.GetNamedNumber("$totalResults"));
                    ErrorLog("Product Category total count : " + _totalCount);
                    JsonArray categoriesObject = sDataProductCategory.GetNamedArray("$resources");
                    int _returnedCount = categoriesObject.Count;
                    ErrorLog("Product Category returned count : " + _returnedCount);
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsProductCategorySyncCompleted))
                    {
                        JsonObject lastCategoryObject =
                            categoriesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastCategoryObject.GetNamedString("$key");
                        int _syncEndpointTick = Convert.ToInt32(lastCategoryObject.GetNamedNumber("SyncTick"));
                        ErrorLog("Product Category sync tick : " + _syncEndpointTick);
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }
                        // Saves ProductCategory data into LocalDB
                        await _productCategoryRepository.SaveProductCategoryDtlsAsync(sDataProductCategory, digest);
                        // Looping this method again to make request for next batch of records(ProductCategory).
                        await SyncProductCategory();
                    }
                    else
                    {
                        DataAccessUtils.ProductCategoryTotalCount = 0;
                        DataAccessUtils.ProductCategoryReturnedCount = 0;
                        DataAccessUtils.IsProductCategorySyncCompleted = false;
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

        /*
        private async Task<bool> SyncProductCategorynextbatch(JsonObject sDataProductCategory, LocalSyncDigest digest)
        {

            try
            {
                bool isSyncCompleted = false;
                HttpResponseMessage productCategoryResponse = null;
                int totalCount = Convert.ToInt32(sDataProductCategory.GetNamedNumber("$totalResults"));
                JsonArray categoriesObject = sDataProductCategory.GetNamedArray("$resources");
                int returnedCount = categoriesObject.Count;
                if (returnedCount > 0 && totalCount - returnedCount > 0)
                {
                    var lastCategoryObject = categoriesObject.GetObjectAt(Convert.ToUInt32(returnedCount - 1));
                    var lastCategoryId = lastCategoryObject.GetNamedString("$key");
                    var syncEndpointTick = Convert.ToInt32(lastCategoryObject.GetNamedNumber("SyncEndpointTick"));

                    parameters["LastRecordId"] = lastCategoryId;
                    parameters["LocalTick"] = syncEndpointTick.ToString();
                    productCategoryResponse = await _sendRequest.BuildAndSendRequest(Constants.CategoryEntity, _productCategoryQueryEntity, Constants.AssociatedItems, Constants.AccessToken, parameters);
                    if (productCategoryResponse.IsSuccessStatusCode)
                    {
                        digest.LastRecordId = lastCategoryId;
                        digest.SDataEntity = Constants.CategoryEntity;
                        if (Convert.ToInt32(syncEndpointTick) > digest.localTick)
                        {
                            digest.localTick = Convert.ToInt32(syncEndpointTick);
                        }
                        var sDataProductCategoryNextbatch = await _sendRequest.ConvertToJsonObject(productCategoryResponse);
                        await SaveProductCategoryDtls(sDataProductCategoryNextbatch, digest);
                        await SyncProductCategorynextbatch(sDataProductCategoryNextbatch, digest);
                    }
                    return isSyncCompleted;
                }
                else
                {
                    return isSyncCompleted = true;
                }

            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        */

        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="message"></param>
        private void ErrorLog(string message)
        {
            AppEventSource.Log.Info(message);
        }
    }
}