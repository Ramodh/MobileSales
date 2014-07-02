﻿using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        private readonly IDatabase _database;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;


        public ProductRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository, IProductAssociatedBlobsRepository productAssociatedBlobsRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
        }

        # region Public Methods

        /// <summary>
        /// Extracts data from sData(jsonObject) and then saves data into Product, ProductRelatedItems & LocalSyncDigest tables in LocalDB
        /// </summary>
        /// <param name="sDataProducts"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveProductsAsync(JsonObject sDataProducts, LocalSyncDigest localSyncDigest)
        {
            try
            {
                JsonArray sDataProductsArray = sDataProducts.GetNamedArray("$resources");
                DataAccessUtils.ProductReturnedCount += sDataProductsArray.Count;
                
                for (int product = 0; product < sDataProductsArray.Count; product++)
                {
                    var sDataProduct = sDataProductsArray[product].GetObject();
                    await AddOrUpdateProductJsonToDbAsync(sDataProduct);

                    if (localSyncDigest != null)
                    {
                        if ((Convert.ToInt32(sDataProduct.GetNamedNumber("SyncEndpointTick")) > localSyncDigest.localTick))
                            localSyncDigest.localTick = Convert.ToInt32(sDataProduct.GetNamedNumber("SyncEndpointTick"));
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
        /// Updates Prodcut data into LocalDb
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns></returns>
        public async Task UpdatProductsAsync(JsonObject sDataProduct)
        {
            try
            {
                await AddOrUpdateProductJsonToDbAsync(sDataProduct);
                //Product latestProduct = await GetProductDataFromJson(sDataProduct);
                //Product oldProduct = await GetProductdetailsAsync(latestProduct.ProductId);
                //if (oldProduct != null)
                //{
                //    await _sageSalesDB.DeleteAsync(oldProduct);
                //}
                //await _sageSalesDB.InsertAsync(latestProduct);
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
        /// Deletes Product data from LocalDB
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public Task DeleteProductAsync(Product product)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Products data from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductDetails>> GetCategoryProductsAsync(string categoryId)
        {
            List<ProductDetails> productsList = null;
            try
            {


                productsList = await _sageSalesDB.QueryAsync<ProductDetails>("select distinct PRD.ProductId, PRD.ProductName, PRD.Sku, PRD.PriceStd, (select Url from ProductAssociatedBlob as PAB where PAB.ProductId = PRD.ProductId AND PAB.IsPrimary='1') as Url from Product as PRD join ProductCategoryLink as PCL on PRD.ProductId = PCL.ProductId and PRD.EntityStatus='Active' where PCL.CategoryId = ? order by Url desc", categoryId);


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
        /// Gets particular Product Details based on ProductId
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<Product> GetProductdetailsAsync(string productId)
        {
            try
            {
                var productDetails = await _sageSalesDB.QueryAsync<Product>("select * from Product where ProductId=?", productId);
                if (productDetails != null)
                {
                    return productDetails.FirstOrDefault();
                }
                else
                {
                    return null;
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

        public async Task<List<ProductAssociatedBlob>> GetProductRelatedItems(string productId)
        {
            try
            {
                var productDetails = await _sageSalesDB.QueryAsync<ProductAssociatedBlob>("SELECT distinct PAB.ProductId, PAB.Url,PAB.Name FROM ProductRelatedItem  as PRI Inner Join ProductAssociatedBlob  as PAB on  PRI.RelatedItemId =PAB.ProductId where PRI.ProductId=? AND PAB.IsPrimary=1", productId);
                if (productDetails != null)
                {
                    return productDetails;
                }
                else
                {
                    return null;
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
        /// Gets Products from LocalDB respective to search term
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProductDetails>> GetSearchSuggestionsAsync(string searchTerm)
        {
            try
            {
                // Retrieve the search suggestions from LocalDB
                var searchSuggestions = await _sageSalesDB.QueryAsync<ProductDetails>("SELECT distinct PAB.Url,PRD.ProductName,PRD.ProductId,PRD.Sku,PRD.PriceStd from ProductAssociatedBlob as PAB JOIN Product as PRD ON PRD.productid = PAB.productid where PAB.IsPrimary='1' AND ProductName like '%" + searchTerm + "%'");
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
        /// Extracts product from Json response and adds or updates to product table
        /// </summary>
        /// Edited by Ramodh Needs to change
        /// <param name="sDataProduct"></param>
        /// <returns></returns>
        public async Task<Product> AddOrUpdateProductFromOrderLineItem(JsonObject sDataProduct)
        {
            IJsonValue value;
            Product product = new Product();
            if (sDataProduct.TryGetValue("$key", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    product.ProductId = sDataProduct.GetNamedString("$key");
                }
            }
            if (sDataProduct.TryGetValue("Sku", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    product.Sku = sDataProduct.GetNamedString("Sku");
                }
            }
            if (sDataProduct.TryGetValue("Name", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    product.ProductName = sDataProduct.GetNamedString("Name");
                }
            }

            List<Product> productList = await _sageSalesDB.QueryAsync<Product>("Select * from Product where ProductId=?", product.ProductId);
            if (productList.FirstOrDefault() != null)
            {
                if (!string.IsNullOrEmpty(product.Sku))
                    productList.FirstOrDefault().Sku = product.Sku;

                if (!string.IsNullOrEmpty(product.ProductName))
                    productList.FirstOrDefault().ProductName = product.ProductName;

                await _sageSalesDB.UpdateAsync(productList.FirstOrDefault());
            }
            else
            {
                await _sageSalesDB.InsertAsync(product);
            }

            return product;
        }

        /// <summary>
        /// Adds or updates product json response to dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <returns></returns>
        public async Task<Product> AddOrUpdateProductJsonToDbAsync(JsonObject sDataProduct)
        {
            try
            {
                IJsonValue value;
                if (sDataProduct.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Product> productList;
                        productList = await _sageSalesDB.QueryAsync<Product>("SELECT * FROM Product where ProductId=?", sDataProduct.GetNamedString("$key"));

                        if (productList.FirstOrDefault() != null)
                        {
                            return await UpdateProductJsonToDbAsync(sDataProduct, productList.FirstOrDefault());
                        }
                        else
                        {
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

        # endregion

        # region Private Methods

        /// <summary>
        /// Extracts sData(JsonResponse) Response and inserts RelatedItems, AssociatedBlobs into ProductRelatedItems & ProductAssociatedBlobs Table.
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
                        product.ProductId = sDataProduct.GetNamedString("$key");
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
                        product.Quantity = (Int32)sDataProduct.GetNamedNumber("Quantity");
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

                if (sDataProduct.TryGetValue("RelatedItems", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonObject sDataRelatedItems = sDataProduct.GetNamedObject("RelatedItems");
                        if (sDataRelatedItems.ContainsKey("$resources"))
                        {
                            JsonArray sDataRelatedItemsArray = sDataRelatedItems.GetNamedArray("$resources");
                            if (sDataRelatedItemsArray.Count > 0)
                            {
                                List<ProductRelatedItem> lstProductRelatedItem = new List<ProductRelatedItem>();

                                foreach (var relatedItem in sDataRelatedItemsArray)
                                {
                                    var sDataRelatedItem = relatedItem.GetObject();
                                    ProductRelatedItem productRelatedItem = new ProductRelatedItem();
                                    productRelatedItem.ProductId = product.ProductId;
                                    productRelatedItem.RelatedItemId = sDataRelatedItem.GetNamedString("$key");
                                    lstProductRelatedItem.Add(productRelatedItem);
                                }
                                await UpdateProductRelatedItemsAsync(lstProductRelatedItem, product.ProductId);
                            }
                        }
                    }
                }


                if (sDataProduct.TryGetValue("AssociatedBlobs", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonObject sDataAssociatedBlobs = sDataProduct.GetNamedObject("AssociatedBlobs");
                        if (sDataAssociatedBlobs.ContainsKey("$resources"))
                        {
                            JsonArray sDataAssociatedBlobsArray = sDataAssociatedBlobs.GetNamedArray("$resources");
                            if (sDataAssociatedBlobsArray.Count > 0)
                            {
                                foreach (var associatedBlob in sDataAssociatedBlobsArray)
                                {
                                    var sDataAssociatedBlob = associatedBlob.GetObject();
                                    await _productAssociatedBlobsRepository.UpdatProductAssociatedBlobAsync(sDataAssociatedBlob);
                                }
                            }
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
        /// Updated ProductRelatedItems(You may also like)
        /// </summary>
        /// <param name="productRelatesItems"></param>
        /// <param name="ProductId"></param>
        /// <returns></returns>
        private async Task UpdateProductRelatedItemsAsync(List<ProductRelatedItem> productRelatedItems, string productId)
        {
            try
            {
                var oldProductRelatedItems = await _sageSalesDB.QueryAsync<ProductRelatedItem>("select * from ProductRelatedItem where ProductId=?", productId);

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
        /// Update quote json response to dB
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
        /// Add product json response to dB
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Product> AddProductJsonToDbAsync(JsonObject sDataProduct)
        {
            Product productObj = new Product();
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
