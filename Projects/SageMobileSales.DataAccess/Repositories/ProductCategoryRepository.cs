using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SQLite;
using Microsoft.Practices.Prism.PubSubEvents;

namespace SageMobileSales.DataAccess.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly IDatabase _database;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public ProductCategoryRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository,
            IEventAggregator eventAggregator)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _eventAggregator = eventAggregator;
        }

        #region public methods

        /// <summary>
        ///     Extract data from json, saves data into ProductCategory, ProductCategoryLinks &
        ///     LocalSyncDigest tables in LocalDB
        /// </summary>
        /// <param name="sDataProductCategory"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveProductCategoryDtlsAsync(JsonObject sDataProductCategory, LocalSyncDigest localSyncDigest)
        {
            try
            {
                var sDataCategoriesArray = sDataProductCategory.GetNamedArray("$resources");
                DataAccessUtils.ProductCategoryReturnedCount += sDataCategoriesArray.Count;

                for (var category = 0; category < sDataCategoriesArray.Count; category++)
                {
                    var sDataCategory = sDataCategoriesArray[category].GetObject();
                    await AddOrUpdateProductCategoryJsonToDbAsync(sDataCategory);

                    if (localSyncDigest != null)
                    {
                        if ((Convert.ToInt32(sDataCategory.GetNamedNumber("SyncTick")) >
                             localSyncDigest.localTick))
                            localSyncDigest.localTick = Convert.ToInt32(sDataCategory.GetNamedNumber("SyncTick"));
                    }

                    if (category == sDataCategoriesArray.Count - 1)
                    {
                        //Fires an event to update UI/publish
                        _eventAggregator.GetEvent<ProductDataChangedEvent>().Publish(true);

                        if (DataAccessUtils.ProductCategoryTotalCount == DataAccessUtils.ProductCategoryReturnedCount)
                        {
                            if (localSyncDigest == null)
                                localSyncDigest = new LocalSyncDigest();
                            localSyncDigest.localTick++;
                            localSyncDigest.LastRecordId = null;
                            localSyncDigest.LastSyncTime = DateTime.Now;
                            DataAccessUtils.IsProductCategorySyncCompleted = true;
                        }
                        await _localSyncDigestRepository.AddOrUpdateLocalSyncDigestDtlsAsync(localSyncDigest);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }


        /// <summary>
        ///     Update prodcutCategory data into local dB
        /// </summary>
        /// <param name="sDataProductCategory"></param>
        /// <returns></returns>
        public async Task<ProductCategory> UpdateProductCategoryJsonToDbAsync(JsonObject sDataProductCategory,
            ProductCategory productCategoryDbObj)
        {
            try
            {
                productCategoryDbObj =
                    await GetProductCategorydataFromJsonAsync(sDataProductCategory, productCategoryDbObj);
                await _sageSalesDB.UpdateAsync(productCategoryDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productCategoryDbObj;
        }

        ///// <summary>
        /////     Deletes ProductCategory data from LocalDB
        ///// </summary>
        ///// <param name="productCategory"></param>
        ///// <returns></returns>
        //public Task DeleteProductCategoryDtlsAsync(ProductCategory productCategory)
        //{
        //    throw new NotImplementedException();
        //}


        /// <summary>
        ///     Gets ProductCategory data from local dB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductCategory>> GetProductCategoryListDtlsAsync(string parentId)
        {
            List<ProductCategory> productCategoriesList = null;
            try
            {
                if (parentId != null)
                {
                    productCategoriesList =
                        await
                            _sageSalesDB.QueryAsync<ProductCategory>(
                                "select * from ProductCategory where ParentId=? order by CategoryName",
                                parentId);
                }
                else
                {
                    productCategoriesList =
                        await
                            _sageSalesDB.QueryAsync<ProductCategory>(
                                "select * from ProductCategory where ParentId is null order by CategoryName");
                }
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productCategoriesList;
        }

        /// <summary>
        ///     Check to navigate to other levels of "Categories" or "Products"
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetProductCategoryLevel(string parentId)
        {
            List<ProductCategory> productCategoryCount = null;
            try
            {
                productCategoryCount =
                    await
                        _sageSalesDB.QueryAsync<ProductCategory>("select * from ProductCategory where ParentId=?",
                            parentId);
                if (productCategoryCount.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return false;
        }

        //TODO Need to remove this method, Wrote just for temporary fix(Bug: hidding Spinner & displaying no categories text)
        /// <summary>
        ///     Gets ProductCategory data from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductCategory>> GetProductCategoryListAsync()
        {
            List<ProductCategory> productCategoriesList = null;
            try
            {
                productCategoriesList = await _sageSalesDB.QueryAsync<ProductCategory>("select * from ProductCategory");
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productCategoriesList;
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Add productCategory json to local dB
        /// </summary>
        /// <param name="sDataProductCategory"></param>
        /// <returns></returns>
        private async Task<ProductCategory> AddProductCategoryJsonToDbAsync(JsonObject sDataProductCategory)
        {
            var productCategoryObj = new ProductCategory();
            try
            {
                // productCategoryObj.CategoryId = sDataProductCategory.GetNamedString("$key");
                productCategoryObj = await GetProductCategorydataFromJsonAsync(sDataProductCategory, productCategoryObj);

                await _sageSalesDB.InsertAsync(productCategoryObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productCategoryObj;
        }

        /// <summary>
        ///     Add or update product json to local dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <returns></returns>
        private async Task<ProductCategory> AddOrUpdateProductCategoryJsonToDbAsync(JsonObject sDataProductCategory)
        {
            try
            {
                IJsonValue value;
                var entityStatusDeleted = false;
                if (sDataProductCategory.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<ProductCategory> productCategoryList;
                        productCategoryList =
                            await
                                _sageSalesDB.QueryAsync<ProductCategory>(
                                    "SELECT * FROM ProductCategory where CategoryId=?",
                                    sDataProductCategory.GetNamedString("$key"));

                        if (sDataProductCategory.TryGetValue("EntityStatus", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                if (sDataProductCategory.GetNamedString("EntityStatus").Contains("Deleted"))
                                    entityStatusDeleted = true;
                            }
                        }

                        if (productCategoryList.FirstOrDefault() != null)
                        {
                            if (entityStatusDeleted)
                                await
                                    _sageSalesDB.QueryAsync<ProductCategory>(
                                        "DELETE FROM ProductCategory where CategoryId=?",
                                        sDataProductCategory.GetNamedString("$key"));
                            else
                                return
                                    await
                                        UpdateProductCategoryJsonToDbAsync(sDataProductCategory,
                                            productCategoryList.FirstOrDefault());
                        }
                        else
                        {
                            if (!entityStatusDeleted)
                                return await AddProductCategoryJsonToDbAsync(sDataProductCategory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        ///     Extract productCategory from json
        /// </summary>
        /// <param name="sDataProductCategory"></param>
        /// <param name="productCategory"></param>
        /// <returns></returns>
        private async Task<ProductCategory> GetProductCategorydataFromJsonAsync(JsonObject sDataProductCategory,
            ProductCategory productCategory)
        {
            try
            {
                IJsonValue value;
                //ProductCategory productCategory = new ProductCategory();
                productCategory.CategoryId = sDataProductCategory.GetNamedString("$key");

                if (sDataProductCategory.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productCategory.CategoryName = sDataProductCategory.GetNamedString("Name");
                    }
                }
                //  productCategory.TenantId = sDataProductCategory.GetNamedString("TenantId");

                //if ((Convert.ToInt32(sDataCategory.GetNamedNumber("SyncEndpointTick")) > localSyncDigest.localTick))
                //    localSyncDigest.localTick = Convert.ToInt32(sDataCategory.GetNamedNumber("SyncEndpointTick"));

                if (sDataProductCategory.TryGetValue("Parent", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataParentIds = sDataProductCategory.GetNamedObject("Parent");
                        productCategory.ParentId = sDataParentIds.GetNamedString("$key");
                    }
                }
                //// JsonObject sDataAssociatedItems = sDataProductCategory.GetNamedObject("AssociatedItems");
                //var sDataAssociatedItemsArray = sDataProductCategory.GetNamedArray("AssociatedItems");

                //if (sDataAssociatedItemsArray.Count > 0)
                //{
                //    var lstProductCategoryLink = new List<ProductCategoryLink>();

                //    foreach (var associatedItem in sDataAssociatedItemsArray)
                //    {
                //        var sDataAssociatedItem = associatedItem.GetObject();
                //        var productCategoryLink = new ProductCategoryLink();
                //        productCategoryLink.CategoryId = productCategory.CategoryId;
                //        productCategoryLink.ProductId = sDataAssociatedItem.GetNamedString("$key");
                //        lstProductCategoryLink.Add(productCategoryLink);
                //    }
                //    await _sageSalesDB.InsertAllAsync(lstProductCategoryLink);
                //}
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productCategory;
        }

        #endregion
    }
}