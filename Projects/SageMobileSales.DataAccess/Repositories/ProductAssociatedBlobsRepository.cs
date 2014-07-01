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
    public class ProductAssociatedBlobsRepository : IProductAssociatedBlobsRepository
    {
        private SQLiteAsyncConnection _sageSalesDB;

        private readonly IDatabase _database;

        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private string _log = string.Empty;


        public ProductAssociatedBlobsRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
        }

        # region Public Methods
        /// <summary>
        /// Extracts data from sData(jsonObject) and then saves data into ProductAssociatedBlobs & LocalSyncDigest tables in LocalDB
        /// </summary>
        /// <param name="sDataProductAssociatedBlobs"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveProductAssociatedBlobsAsync(JsonObject sDataProductAssociatedBlobs, LocalSyncDigest localSyncDigest)
        {
            try
            {
                JsonArray sDataProductAssociatedBlobsArray = sDataProductAssociatedBlobs.GetNamedArray("$resources");
                DataAccessUtils.ProductAssociatedBlobsReturnedCount += sDataProductAssociatedBlobsArray.Count;

                List<ProductAssociatedBlob> lstProductAssociatedBlobs = new List<ProductAssociatedBlob>();
                for (int productAssociatedBlob = 0; productAssociatedBlob < sDataProductAssociatedBlobsArray.Count; productAssociatedBlob++)
                {
                    var sDataProductAssociatedBlob = sDataProductAssociatedBlobsArray[productAssociatedBlob].GetObject();
                    lstProductAssociatedBlobs.Add(GetProductAssociatedBlobDataFromJson(sDataProductAssociatedBlob));

                    if ((Convert.ToInt32(sDataProductAssociatedBlob.GetNamedNumber("SyncEndpointTick")) > localSyncDigest.localTick))
                        localSyncDigest.localTick = Convert.ToInt32(sDataProductAssociatedBlob.GetNamedNumber("SyncEndpointTick"));
                    if (productAssociatedBlob == sDataProductAssociatedBlobsArray.Count - 1)
                    {
                        if (DataAccessUtils.ProductAssociatedBlobsTotalCount == DataAccessUtils.ProductAssociatedBlobsReturnedCount)
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
        /// Updates Prodcut data into LocalDb
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns></returns>
        public async Task UpdatProductAssociatedBlobAsync(JsonObject sDataProductAssociatedBlob)
        {
            try
            {
                ProductAssociatedBlob latestProductAssociatedBlob = GetProductAssociatedBlobDataFromJson(sDataProductAssociatedBlob);
                ProductAssociatedBlob oldProductAssociatedBlob = await GetProductAssociatedBlobAsync(latestProductAssociatedBlob.ProductAssociatedBlobId);
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
        /// Gets ProductAssociatedBlobs data from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductAssociatedBlob>> GetProductAssociatedBlobsAsync(string productId)
        {
            List<ProductAssociatedBlob> productsAssociatedBlobsList=null;
            try
            {
               
                productsAssociatedBlobsList = await _sageSalesDB.QueryAsync<ProductAssociatedBlob>("select * from ProductAssociatedBlob where ProductId=? order by IsPrimary DESC", productId);

                if (productsAssociatedBlobsList.Count == 0)
                {
                    productsAssociatedBlobsList = new List<ProductAssociatedBlob>();
                    ProductAssociatedBlob productAssociatedBlob = new ProductAssociatedBlob();
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
        /// Extracts ProductAssociatedBlobs sData(JsonResponse) Response.
        /// </summary>
        /// <param name="sDataProductAssociatedBlob"></param>
        /// <returns>Product</returns>
        private ProductAssociatedBlob GetProductAssociatedBlobDataFromJson(JsonObject sDataProductAssociatedBlob)
        {
            try
            {
                ProductAssociatedBlob _productAssociatedBlob = new ProductAssociatedBlob();
                _productAssociatedBlob.ProductAssociatedBlobId = sDataProductAssociatedBlob.GetNamedString("$key");
                if (sDataProductAssociatedBlob.GetNamedValue("FileName").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.Name = sDataProductAssociatedBlob.GetNamedString("FileName");
                }
                _productAssociatedBlob.TenantId = sDataProductAssociatedBlob.GetNamedString("TenantId");
                if (sDataProductAssociatedBlob.GetNamedValue("Description").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.productAssociatedBlobDescription = sDataProductAssociatedBlob.GetNamedString("Description");
                }
                if (sDataProductAssociatedBlob.GetNamedValue("MimeType").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.MimeType = sDataProductAssociatedBlob.GetNamedString("MimeType");
                }
                if (sDataProductAssociatedBlob.GetNamedValue("Url").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.Url = sDataProductAssociatedBlob.GetNamedString("Url");
                }
                if (sDataProductAssociatedBlob.GetNamedValue("ThumbnailMimeType").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.ThumbnailMimeType = sDataProductAssociatedBlob.GetNamedString("ThumbnailMimeType");
                }
                if (sDataProductAssociatedBlob.GetNamedValue("ThumbnailUrl").ValueType.ToString() != DataAccessUtils.Null)
                {
                    _productAssociatedBlob.ThumbnailUrl = sDataProductAssociatedBlob.GetNamedString("ThumbnailUrl");
                }
                _productAssociatedBlob.ProductId = sDataProductAssociatedBlob.GetNamedString("ParentEntityId");
                _productAssociatedBlob.IsPrimary = sDataProductAssociatedBlob.GetNamedBoolean("IsPrimaryImage");
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
        /// Gets ProductAssociatedBlob data from LocalDB
        /// </summary>
        /// <returns></returns>
        private async Task<ProductAssociatedBlob> GetProductAssociatedBlobAsync(string productAssociatedBlobId)
        {
            try
            {
                var productsAssociatedBlob = await _sageSalesDB.QueryAsync<ProductAssociatedBlob>("select * from ProductAssociatedBlob where ProductAssociatedBlobId=?", productAssociatedBlobId);
                if (productsAssociatedBlob.Count > 0)
                {
                    return productsAssociatedBlob.FirstOrDefault();
                }
                else
                {
                    return null;
                }
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
