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
    public class ProductAssociatedBlobsRepository : IProductAssociatedBlobsRepository
    {
        private readonly IDatabase _database;

        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;


        public ProductAssociatedBlobsRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
        }

        # region Public Methods

        /// <summary>
        ///     Extract data from json, saves data into ProductAssociatedBlobs & LocalSyncDigest tables in
        ///     LocalDB
        /// </summary>
        /// <param name="sDataProductAssociatedBlobs"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveProductAssociatedBlobsAsync(JsonObject sDataProductAssociatedBlobs,
            LocalSyncDigest localSyncDigest)
        {
            try
            {
                JsonArray sDataProductAssociatedBlobsArray = sDataProductAssociatedBlobs.GetNamedArray("$resources");
                DataAccessUtils.ProductAssociatedBlobsReturnedCount += sDataProductAssociatedBlobsArray.Count;

                var lstProductAssociatedBlobs = new List<ProductAssociatedBlob>();
                for (int productAssociatedBlob = 0;
                    productAssociatedBlob < sDataProductAssociatedBlobsArray.Count;
                    productAssociatedBlob++)
                {
                    JsonObject sDataProductAssociatedBlob =
                        sDataProductAssociatedBlobsArray[productAssociatedBlob].GetObject();
                    // Adds or Updated ProductAssociatedBlob data from json to LocalDB
                    await AddOrUpdateProductAssociatedBlobJsonToDbAsync(sDataProductAssociatedBlob);

                    if ((Convert.ToInt32(sDataProductAssociatedBlob.GetNamedNumber("SyncTick")) >
                         localSyncDigest.localTick))
                        localSyncDigest.localTick =
                            Convert.ToInt32(sDataProductAssociatedBlob.GetNamedNumber("SyncTick"));
                    if (productAssociatedBlob == sDataProductAssociatedBlobsArray.Count - 1)
                    {
                        if (DataAccessUtils.ProductAssociatedBlobsTotalCount ==
                            DataAccessUtils.ProductAssociatedBlobsReturnedCount)
                        {
                            if (localSyncDigest == null)
                                localSyncDigest = new LocalSyncDigest();
                            localSyncDigest.localTick++;
                            localSyncDigest.LastRecordId = null;
                            localSyncDigest.LastSyncTime = DateTime.Now;
                            DataAccessUtils.IsProductAssociatedBlobsSyncCompleted = true;
                        }
                        await _localSyncDigestRepository.UpdateLocalSyncDigestDtlsAsync(localSyncDigest);
                    }
                }                
            }
            catch (SQLiteException ex)
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
        ///     Adds or updates ProductAssociatedBlob json response to dB
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <returns></returns>
        public async Task<ProductAssociatedBlob> AddOrUpdateProductAssociatedBlobJsonToDbAsync(JsonObject sDataProductAssociatedBlob)
        {
            try
            {
                IJsonValue value;
                bool entityStatusDeleted = false;
                List<ProductAssociatedBlob> productAssociatedBlobsList = null;

                if (sDataProductAssociatedBlob.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobsList =
                            await
                                _sageSalesDB.QueryAsync<ProductAssociatedBlob>("SELECT * FROM ProductAssociatedBlob where ProductAssociatedBlobId=?",
                                    sDataProductAssociatedBlob.GetNamedString("$key"));
                        if (sDataProductAssociatedBlob.TryGetValue("EntityStatus", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                if (sDataProductAssociatedBlob.GetNamedString("EntityStatus").Contains("Deleted"))
                                    entityStatusDeleted = true;
                            }
                        }
                        if (productAssociatedBlobsList.FirstOrDefault() != null)
                        {
                            if (entityStatusDeleted)
                            {
                                await DeleteProductAssociatedBlob(sDataProductAssociatedBlob.GetNamedString("$key"));
                            }
                            else
                            {

                                return await UpdateProductAssociatedBlobJsonToDbAsync(sDataProductAssociatedBlob, productAssociatedBlobsList.FirstOrDefault());
                            }
                        }
                        return await AddProductAssociatedBlobJsonToDbAsync(sDataProductAssociatedBlob);
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
        ///     Gets productAssociatedBlobs data from local dB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductAssociatedBlob>> GetProductAssociatedBlobsAsync(string productId)
        {
            List<ProductAssociatedBlob> productsAssociatedBlobsList = null;
            try
            {
                productsAssociatedBlobsList =
                    await
                        _sageSalesDB.QueryAsync<ProductAssociatedBlob>(
                            "select * from ProductAssociatedBlob where ProductId=? order by IsPrimary DESC", productId);

                if (productsAssociatedBlobsList.Count == 0)
                {
                    productsAssociatedBlobsList = new List<ProductAssociatedBlob>();
                    var productAssociatedBlob = new ProductAssociatedBlob();
                    productAssociatedBlob.Url = string.Empty;
                    productsAssociatedBlobsList.Add(productAssociatedBlob);
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productsAssociatedBlobsList;
        }

        # endregion

        # region Private Methods

        /// <summary>
        ///     Add ProductAssociatedBlob json response to dB
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <returns></returns>
        private async Task<ProductAssociatedBlob> AddProductAssociatedBlobJsonToDbAsync(JsonObject sDataProductAssociatedBlob)
        {
            var productAssociatedBlobObj = new ProductAssociatedBlob();
            try
            {
                productAssociatedBlobObj.ProductAssociatedBlobId = sDataProductAssociatedBlob.GetNamedString("$key");
                productAssociatedBlobObj = ExtractProductAssociatedBlobFromJsonAsync(sDataProductAssociatedBlob, productAssociatedBlobObj);

                await _sageSalesDB.InsertAsync(productAssociatedBlobObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productAssociatedBlobObj;
        }

        /// <summary>
        ///     Update ProductAssociatedBlob json response to dB
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <param name="productAssociatedBlobDbObj"></param>
        /// <returns></returns>
        private async Task<ProductAssociatedBlob> UpdateProductAssociatedBlobJsonToDbAsync(JsonObject sDataProductAssociatedBlob, ProductAssociatedBlob productAssociatedBlobDbObj)
        {
            try
            {
                productAssociatedBlobDbObj = ExtractProductAssociatedBlobFromJsonAsync(sDataProductAssociatedBlob, productAssociatedBlobDbObj);
                await _sageSalesDB.UpdateAsync(productAssociatedBlobDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productAssociatedBlobDbObj;
        }

        /// <summary>
        ///     Extract ProductAssociatedBlobs json Response.
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <returns>Product</returns>
        private ProductAssociatedBlob ExtractProductAssociatedBlobFromJsonAsync(JsonObject sDataProductAssociatedBlob, ProductAssociatedBlob productAssociatedBlobDbObj)
        {
            try
            {
                IJsonValue value;

                if (sDataProductAssociatedBlob.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.ProductAssociatedBlobId =
                            sDataProductAssociatedBlob.GetNamedString("$key").ToLower();
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.Name = sDataProductAssociatedBlob.GetNamedString("Name");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Description", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.productAssociatedBlobDescription =
                            sDataProductAssociatedBlob.GetNamedString("Description");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("MimeType", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.MimeType = sDataProductAssociatedBlob.GetNamedString("MimeType");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Url", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.Url = sDataProductAssociatedBlob.GetNamedString("Url");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ThumbnailMimeType", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.ThumbnailMimeType =
                            sDataProductAssociatedBlob.GetNamedString("ThumbnailMimeType");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ThumbnailUrl", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.ThumbnailUrl = sDataProductAssociatedBlob.GetNamedString("ThumbnailUrl");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ParentEntityId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        productAssociatedBlobDbObj.ProductId =
                            sDataProductAssociatedBlob.GetNamedString("ParentEntityId").ToLower();
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Type", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        //_productAssociatedBlob.IsPrimary = sDataProductAssociatedBlob.GetNamedBoolean("IsPrimaryImage");
                        if (sDataProductAssociatedBlob.GetNamedString("Type") == "PrimaryImage")
                        {
                            productAssociatedBlobDbObj.IsPrimary = true;
                        }
                    }
                }
                return productAssociatedBlobDbObj;
            }


            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }


        /// <summary>
        ///     Gets ProductAssociatedBlob data from LocalDB
        /// </summary>
        /// <returns></returns>
        private async Task<ProductAssociatedBlob> GetProductAssociatedBlobAsync(string productAssociatedBlobId)
        {
            try
            {
                List<ProductAssociatedBlob> productsAssociatedBlob =
                    await
                        _sageSalesDB.QueryAsync<ProductAssociatedBlob>(
                            "select * from ProductAssociatedBlob where ProductAssociatedBlobId=?",
                            productAssociatedBlobId);
                if (productsAssociatedBlob.Count > 0)
                {
                    return productsAssociatedBlob.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        /// Delete ProductAssociatedBlob from LocalDB
        /// </summary>
        /// <param name="productAssociatedBlobId"></param>
        /// <returns></returns>
        private async Task DeleteProductAssociatedBlob(string productAssociatedBlobId)
        {
            try
            {
                await _sageSalesDB.QueryAsync<ProductAssociatedBlob>("Delete From ProductAssociatedBlob where ProductAssociatedBlobId=?", productAssociatedBlobId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }


        # endregion
    }
}