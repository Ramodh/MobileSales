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
                    lstProductAssociatedBlobs.Add(GetProductAssociatedBlobDataFromJson(sDataProductAssociatedBlob));

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
                await _sageSalesDB.InsertAllAsync(lstProductAssociatedBlobs);
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
        ///     Update prodcut data into local dB
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns></returns>
        public async Task UpdatProductAssociatedBlobAsync(JsonObject sDataProductAssociatedBlob)
        {
            try
            {
                ProductAssociatedBlob latestProductAssociatedBlob =
                    GetProductAssociatedBlobDataFromJson(sDataProductAssociatedBlob);
                ProductAssociatedBlob oldProductAssociatedBlob =
                    await GetProductAssociatedBlobAsync(latestProductAssociatedBlob.ProductAssociatedBlobId);
                if (oldProductAssociatedBlob != null)
                {
                    await _sageSalesDB.DeleteAsync(oldProductAssociatedBlob);
                }
                await _sageSalesDB.InsertAsync(latestProductAssociatedBlob);
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
        ///     Extract ProductAssociatedBlobs json Response.
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <returns>Product</returns>
        private ProductAssociatedBlob GetProductAssociatedBlobDataFromJson(JsonObject sDataProductAssociatedBlob)
        {
            try
            {
                IJsonValue value;
                var _productAssociatedBlob = new ProductAssociatedBlob();

                if (sDataProductAssociatedBlob.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.ProductAssociatedBlobId =
                            sDataProductAssociatedBlob.GetNamedString("$key").ToLower();
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.Name = sDataProductAssociatedBlob.GetNamedString("Name");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Description", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.productAssociatedBlobDescription =
                            sDataProductAssociatedBlob.GetNamedString("Description");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("MimeType", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.MimeType = sDataProductAssociatedBlob.GetNamedString("MimeType");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("Url", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.Url = sDataProductAssociatedBlob.GetNamedString("Url");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ThumbnailMimeType", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.ThumbnailMimeType =
                            sDataProductAssociatedBlob.GetNamedString("ThumbnailMimeType");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ThumbnailUrl", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.ThumbnailUrl = sDataProductAssociatedBlob.GetNamedString("ThumbnailUrl");
                    }
                }
                if (sDataProductAssociatedBlob.TryGetValue("ParentEntityId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        _productAssociatedBlob.ProductId =
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
                            _productAssociatedBlob.IsPrimary = true;
                        }
                    }
                }
                return _productAssociatedBlob;
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

        # endregion
    }
}