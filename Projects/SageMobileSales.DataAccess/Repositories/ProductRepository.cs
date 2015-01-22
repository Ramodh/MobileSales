using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDatabase _database;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public ProductRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository,
            IProductAssociatedBlobsRepository productAssociatedBlobsRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
        }

        # region Public Methods

        /// <summary>
        ///     Extract data from json, save data into Product, ProductRelatedItems & LocalSyncDigest tables
        ///     in local dB
        /// </summary>
        /// <param name="sDataProducts"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveProductsAsync(JsonObject sDataProducts, LocalSyncDigest localSyncDigest)
        {
            try
            {
                var sDataProductsArray = sDataProducts.GetNamedArray("$resources");
                DataAccessUtils.ProductReturnedCount += sDataProductsArray.Count;

                for (var product = 0; product < sDataProductsArray.Count; product++)
                {
                    var sDataProduct = sDataProductsArray[product].GetObject();
                    await AddOrUpdateProductJsonToDbAsync(sDataProduct);

                    if (localSyncDigest != null)
                    {
                        if ((Convert.ToInt32(sDataProduct.GetNamedNumber("SyncTick")) >
                             localSyncDigest.localTick))
                            localSyncDigest.localTick = Convert.ToInt32(sDataProduct.GetNamedNumber("SyncTick"));
                    }

                    if (product == (sDataProductsArray.Count - 1) && localSyncDigest != null)
                    {
                        if (DataAccessUtils.ProductTotalCount == DataAccessUtils.ProductReturnedCount)
                        {
                            if (localSyncDigest == null)
                                localSyncDigest = new LocalSyncDigest();
                            localSyncDigest.localTick++;
                            localSyncDigest.LastRecordId = null;
                            localSyncDigest.LastSyncTime = DateTime.Now;
                            DataAccessUtils.IsProductSyncCompleted = true;
                        }
                        await _localSyncDigestRepository.AddOrUpdateLocalSyncDigestDtlsAsync(localSyncDigest);
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
        ///     Update prodcut data into local dB
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns></returns>
        public async Task UpdatProductsAsync(JsonObject sDataProduct)
        {
            try
            {
                await AddOrUpdateProductJsonToDbAsync(sDataProduct);
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

        ///// <summary>
        /////     Deletes Product data from LocalDB
        ///// </summary>
        ///// <param name="product"></param>
        ///// <returns></returns>
        //public Task DeleteProductAsync(Product product)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        ///     Gets product list from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductDetails>> GetCategoryProductsAsync(string categoryId)
        {
            List<ProductDetails> productsList = null;
            try
            {
                productsList =
                    await
                        _sageSalesDB.QueryAsync<ProductDetails>(
                            "select distinct PRD.ProductId, PRD.ProductName, PRD.Sku, PRD.PriceStd, (select Url from ProductAssociatedBlob as PAB where PAB.ProductId = PRD.ProductId AND (PAB.IsPrimary='1' OR PAB.IsPrimary='0')) as Url from Product as PRD join ProductCategoryLink as PCL on PRD.ProductId = PCL.ProductId and PRD.EntityStatus='Active' where PCL.CategoryId = ? order by ProductName",
                            categoryId);
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
            return productsList;
        }

        /// <summary>
        ///     Gets product details for that ProductId
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<Product> GetProductdetailsAsync(string productId)
        {
            try
            {
                var productDetails =
                    await _sageSalesDB.QueryAsync<Product>("select * from Product where ProductId=?", productId);
                if (productDetails != null)
                {
                    return productDetails.FirstOrDefault();
                }
                return null;
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
            return null;
        }

        public async Task<List<ProductDetails>> GetProductRelatedItems(string productId)
        {
            // Need to get confirmation from ramodh before removing the below code.
            //try
            //{
            //    List<ProductAssociatedBlob> productDetails =
            //        await
            //            _sageSalesDB.QueryAsync<ProductAssociatedBlob>(
            //                "SELECT distinct PAB.ProductId, PAB.Url,PAB.Name FROM ProductRelatedItem  as PRI Inner Join ProductAssociatedBlob  as PAB on  PRI.RelatedItemId = PAB.ProductId where PRI.ProductId=? AND PAB.IsPrimary=1",
            //                productId);
            //    if (productDetails != null)
            //    {
            //        return productDetails;
            //    }
            //    return null;
            //}

            List<ProductDetails> productsList = null;
            try
            {
                productsList =
                    await
                        _sageSalesDB.QueryAsync<ProductDetails>(
                            "select distinct PRD.ProductId, PRD.ProductName, (select Url from ProductAssociatedBlob as PAB where PAB.ProductId = PRD.ProductId AND (PAB.IsPrimary=1 OR PAB.IsPrimary=0)) as Url from ProductRelatedItem as PRI join Product as PRD on PRD.ProductId = PRI.RelatedItemId and PRD.EntityStatus='Active' where PRI.ProductId = ?",
                            productId);
                if (productsList != null)
                {
                    return productsList;
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
            return null;
        }

        /// <summary>
        ///     Gets Product from local dB respective to search term
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductDetails>> GetSearchSuggestionsAsync(string searchTerm)
        {
            try
            {
                // Retrieve the search suggestions from LocalDB
                var searchSuggestions =
                    await
                        _sageSalesDB.QueryAsync<ProductDetails>(
                            "select distinct PRD.ProductId, PRD.ProductName, PRD.Sku, PRD.PriceStd, (select Url from ProductAssociatedBlob as PAB where PAB.ProductId = PRD.ProductId AND (PAB.IsPrimary='1' OR PAB.IsPrimary='0')) as Url from Product as PRD join ProductCategoryLink as PCL on PRD.ProductId = PCL.ProductId and PRD.EntityStatus='Active' AND (ProductName like '%" +
                            searchTerm + "%') OR (SKU like '%" +
                            searchTerm + "%') order by ProductName asc");
                //await
                //    _sageSalesDB.QueryAsync<ProductDetails>(
                //        "SELECT distinct PAB.Url,PRD.ProductName,PRD.ProductId,PRD.Sku,PRD.PriceStd from ProductAssociatedBlob as PAB JOIN Product as PRD ON PRD.productid = PAB.productid where PAB.IsPrimary='1' AND ProductName like '%" +
                //        searchTerm + "%'");
                return searchSuggestions;
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
            return null;
        }

        /// <summary>
        ///     Add or update product json to local dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <returns></returns>
        public async Task<Product> AddOrUpdateProductJsonToDbAsync(JsonObject sDataProduct)
        {
            try
            {
                IJsonValue value;
                var entityStatusDeleted = false;

                if (sDataProduct.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Product> productList;
                        productList =
                            await
                                _sageSalesDB.QueryAsync<Product>("SELECT * FROM Product where ProductId=?",
                                    sDataProduct.GetNamedString("$key"));

                        if (sDataProduct.TryGetValue("EntityStatus", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                if (sDataProduct.GetNamedString("EntityStatus").Contains("Deleted"))
                                    entityStatusDeleted = true;
                            }
                        }

                        if (productList.FirstOrDefault() != null)
                        {
                            if (entityStatusDeleted)
                                await
                                    _sageSalesDB.QueryAsync<ProductCategory>(
                                        "DELETE FROM ProductCategoryLink where ProductId=?",
                                        sDataProduct.GetNamedString("$key"));
                            else
                                return await UpdateProductJsonToDbAsync(sDataProduct, productList.FirstOrDefault());
                        }
                        else
                        {
                            if (!entityStatusDeleted)
                                return await AddProductJsonToDbAsync(sDataProduct);
                        }
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
            return null;
        }

        ///// <summary>
        /////     Extract product from Json, add or update to product table
        ///// </summary>
        ///// Edited by Ramodh Needs to change
        ///// <param name="sDataProduct"></param>
        ///// <returns></returns>
        //public async Task<Product> AddOrUpdateProductFromOrderLineItem(JsonObject sDataProduct)
        //{
        //    IJsonValue value;
        //    var product = new Product();
        //    if (sDataProduct.TryGetValue("$key", out value))
        //    {
        //        if (value.ValueType.ToString() != DataAccessUtils.Null)
        //        {
        //            product.ProductId = sDataProduct.GetNamedString("$key").ToLower();
        //        }
        //    }
        //    if (sDataProduct.TryGetValue("Sku", out value))
        //    {
        //        if (value.ValueType.ToString() != DataAccessUtils.Null)
        //        {
        //            product.Sku = sDataProduct.GetNamedString("Sku");
        //        }
        //    }
        //    if (sDataProduct.TryGetValue("Name", out value))
        //    {
        //        if (value.ValueType.ToString() != DataAccessUtils.Null)
        //        {
        //            product.ProductName = sDataProduct.GetNamedString("Name");
        //        }
        //    }

        //    List<Product> productList =
        //        await _sageSalesDB.QueryAsync<Product>("Select * from Product where ProductId=?", product.ProductId);
        //    if (productList.FirstOrDefault() != null)
        //    {
        //        if (!string.IsNullOrEmpty(product.Sku))
        //            productList.FirstOrDefault().Sku = product.Sku;

        //        if (!string.IsNullOrEmpty(product.ProductName))
        //            productList.FirstOrDefault().ProductName = product.ProductName;

        //        await _sageSalesDB.UpdateAsync(productList.FirstOrDefault());
        //    }
        //    else
        //    {
        //        await _sageSalesDB.InsertAsync(product);
        //    }

        //    return product;
        //}

        # endregion

        # region Private Methods

        /// <summary>
        ///     Extract product, product related items, blobs json
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns>Product</returns>
        private async Task<Product> ExtractProductFromJsonAsync(JsonObject sDataProduct, Product product)
        {
            try
            {
                IJsonValue value;

                if (sDataProduct.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.ProductId = sDataProduct.GetNamedString("$key").ToLower();
                    }
                }

                if (sDataProduct.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.ProductName = sDataProduct.GetNamedString("Name");
                    }
                }

                if (sDataProduct.TryGetValue("TenantId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.TenantId = sDataProduct.GetNamedString("TenantId");
                    }
                }

                if (sDataProduct.TryGetValue("Sku", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.Sku = sDataProduct.GetNamedString("Sku");
                    }
                }

                if (sDataProduct.TryGetValue("Description", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.ProductDescription = sDataProduct.GetNamedString("Description");
                    }
                }

                if (sDataProduct.TryGetValue("PriceStd", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.PriceStd = Convert.ToDecimal(sDataProduct.GetNamedNumber("PriceStd"));
                    }
                }

                if (sDataProduct.TryGetValue("CostStd", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.CostStd = Convert.ToDecimal(sDataProduct.GetNamedNumber("CostStd"));
                    }
                }

                if (sDataProduct.TryGetValue("Quantity", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.Quantity = Convert.ToInt32(Convert.ToDecimal(sDataProduct.GetNamedNumber("Quantity")));
                    }
                }

                if (sDataProduct.TryGetValue("UnitOfMeasure", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.UnitOfMeasure = sDataProduct.GetNamedString("UnitOfMeasure");
                    }
                }

                if (sDataProduct.TryGetValue("EntityStatus", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        product.EntityStatus = sDataProduct.GetNamedString("EntityStatus");
                    }
                }

                // TO DO : Change the way it is implementd not to delete and insert
                if (sDataProduct.TryGetValue("RelatedItems", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataRelatedItemsArray = sDataProduct.GetNamedArray("RelatedItems");
                        if (sDataRelatedItemsArray.Count > 0)
                        {
                            var lstProductRelatedItem = new List<ProductRelatedItem>();

                            foreach (var relatedItem in sDataRelatedItemsArray)
                            {
                                var sDataRelatedItem = relatedItem.GetObject();
                                var productRelatedItem = new ProductRelatedItem();
                                productRelatedItem.ProductId = product.ProductId;
                                productRelatedItem.RelatedItemId = sDataRelatedItem.GetNamedString("$key").ToLower();
                                lstProductRelatedItem.Add(productRelatedItem);
                            }
                            await UpdateProductRelatedItemsAsync(lstProductRelatedItem, product.ProductId);
                        }
                    }
                }

                // TO DO : Change the way it is implementd not to delete and insert
                if (sDataProduct.TryGetValue("Images", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataAssociatedBlobsArray = sDataProduct.GetNamedArray("Images");
                        if (sDataAssociatedBlobsArray.Count > 0)
                        {
                            foreach (var associatedBlob in sDataAssociatedBlobsArray)
                            {
                                var sDataAssociatedBlob = associatedBlob.GetObject();
                                await
                                    _productAssociatedBlobsRepository.AddOrUpdateProductAssociatedBlobJsonToDbAsync(
                                        sDataAssociatedBlob);
                            }
                        }
                    }
                }

                if (sDataProduct.TryGetValue("AssociatedCategories", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataAssociatedCategoriesArray = sDataProduct.GetNamedArray("AssociatedCategories");

                        if (sDataAssociatedCategoriesArray.Count > 0)
                        {
                            var lstProductCategoryLink = new List<ProductCategoryLink>();

                            foreach (var associatedCategory in sDataAssociatedCategoriesArray)
                            {
                                var sDataAssociatedItem = associatedCategory.GetObject();
                                var productCategoryLink = new ProductCategoryLink();
                                productCategoryLink.CategoryId = sDataAssociatedItem.GetNamedString("$key");
                                productCategoryLink.ProductId = product.ProductId;
                                lstProductCategoryLink.Add(productCategoryLink);
                            }
                            await _sageSalesDB.InsertAllAsync(lstProductCategoryLink);
                        }
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
            return product;
        }

        /// <summary>
        ///     Updat ProductRelatedItems
        /// </summary>
        /// <param name="productRelatesItems"></param>
        /// <param name="ProductId"></param>
        /// <returns></returns>
        private async Task UpdateProductRelatedItemsAsync(List<ProductRelatedItem> productRelatedItems, string productId)
        {
            try
            {
                var oldProductRelatedItems =
                    await
                        _sageSalesDB.QueryAsync<ProductRelatedItem>(
                            "select * from ProductRelatedItem where ProductId=?", productId);

                foreach (var productRelatedItem in oldProductRelatedItems)
                {
                    await _sageSalesDB.DeleteAsync(productRelatedItem);
                }

                await _sageSalesDB.InsertAllAsync(productRelatedItems);
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
        ///     Update quote json response to dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <param name="quoteDbObj"></param>
        /// <returns></returns>
        private async Task<Product> UpdateProductJsonToDbAsync(JsonObject sDataProduct, Product productDbObj)
        {
            try
            {
                productDbObj = await ExtractProductFromJsonAsync(sDataProduct, productDbObj);
                await _sageSalesDB.UpdateAsync(productDbObj);
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
            return productDbObj;
        }

        /// <summary>
        ///     Add product json response to dB
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Product> AddProductJsonToDbAsync(JsonObject sDataProduct)
        {
            var productObj = new Product();
            try
            {
                productObj.ProductId = sDataProduct.GetNamedString("$key");
                productObj = await ExtractProductFromJsonAsync(sDataProduct, productObj);

                await _sageSalesDB.InsertAsync(productObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return productObj;
        }

        # endregion
    }
}