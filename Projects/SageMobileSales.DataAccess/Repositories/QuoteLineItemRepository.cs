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
    public class QuoteLineItemRepository : IQuoteLineItemRepository
    {
        private readonly IDatabase _database;
        private readonly IProductRepository _productRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public QuoteLineItemRepository(IDatabase database, IProductRepository productRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _productRepository = productRepository;
        }

        #region public methods

        /// <summary>
        ///     Extract QuoteLineItem from json for that quoteId
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns>Product</returns>
        public async Task SaveQuoteLineItemsAsync(JsonObject sDataQuote, string quoteId)
        {
            try
            {
                IJsonValue value;
                if (!string.IsNullOrEmpty(quoteId))
                {
                    if (sDataQuote.TryGetValue("Details", out value))
                    {
                        if (value.ValueType.ToString() != null)
                        {
                            JsonArray sDataQuoteLineItemsArray = sDataQuote.GetNamedArray("Details");
                            if (sDataQuoteLineItemsArray.Count > 0)
                            {
                                await SaveQuoteLineItemDetailsAsync(sDataQuoteLineItemsArray, quoteId);
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
        }

        /// <summary>
        /// Posted quote, line item is saved to local dB
        /// </summary>
        /// <param name="sDataQuoteLineItems"></param>
        /// <param name="quoteId"></param>
        /// <param name="pendingQuoteLineItem"></param>
        /// <returns></returns>
        public async Task SavePostedQuoteLineItemsAsync(JsonObject sDataQuoteLineItems, string quoteId,
            QuoteLineItem pendingQuoteLineItem)
        {
            try
            {
                //TO DO : Optimize
                //JsonObject sDataQuoteLineItems = sDataQuote.GetNamedObject("Details");
                if (sDataQuoteLineItems.ContainsKey("Details"))
                {
                    JsonArray sDataQuoteLineItemsArray = sDataQuoteLineItems.GetNamedArray("Details");
                    if (sDataQuoteLineItemsArray.Count > 0)
                    {
                        List<QuoteLineItem> QuoteLineItemList = await GetQuoteLineItemsForQuote(quoteId);
                        //await SaveQuoteLineItemDetailsAsync(sDataQuoteLineItemsArray, quoteId);

                        for (int quoteLineItem = 0; quoteLineItem < sDataQuoteLineItemsArray.Count; quoteLineItem++)
                        {
                            JsonObject sDataQuoteLineItem = sDataQuoteLineItemsArray[quoteLineItem].GetObject();

                            IJsonValue value;
                            if (sDataQuoteLineItem.TryGetValue("$key", out value))
                            {
                                if (value.ValueType.ToString() != DataAccessUtils.Null)
                                {
                                    List<QuoteLineItem> quoteLineItemList;
                                    quoteLineItemList =
                                        await
                                            _sageSalesDB.QueryAsync<QuoteLineItem>(
                                                "SELECT * FROM QuoteLineItem where QuoteLineItemId=?",
                                                sDataQuoteLineItem.GetNamedString("$key"));

                                    if (quoteLineItemList.FirstOrDefault() != null)
                                    {
                                        await
                                            UpdateQuoteLineItemJsonToDbAsync(sDataQuoteLineItem,
                                                quoteLineItemList.FirstOrDefault());
                                    }
                                    else if (pendingQuoteLineItem != null)
                                    {
                                        await
                                            _sageSalesDB.QueryAsync<QuoteLineItem>(
                                                "Update QuoteLineItem Set QuoteLineItemId=? where QuoteLineItemId=?",
                                                sDataQuoteLineItem.GetNamedString("$key"),
                                                pendingQuoteLineItem.QuoteLineItemId);
                                        pendingQuoteLineItem.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("$key");
                                        await UpdateQuoteLineItemJsonToDbAsync(sDataQuoteLineItem, pendingQuoteLineItem);
                                    }
                                    else if (sDataQuoteLineItemsArray.Count <= QuoteLineItemList.Count &&
                                             QuoteLineItemList[quoteLineItem] != null)
                                    {
                                        if (
                                            QuoteLineItemList[quoteLineItem].QuoteLineItemId.Contains(
                                                DataAccessUtils.Pending))
                                        {
                                            if (sDataQuoteLineItem.TryGetValue("$key", out value))
                                            {
                                                if (value.ValueType.ToString() != DataAccessUtils.Null)
                                                {
                                                    //JsonObject sDataProduct =
                                                    //    sDataQuoteLineItem.GetNamedObject("InventoryItem");
                                                    //if (sDataQuoteLineItem.GetNamedValue("$key").ValueType.ToString() !=
                                                    //    DataAccessUtils.Null)
                                                    //{
                                                    for (int i = 0; i < QuoteLineItemList.Count; i++)
                                                    {
                                                        if (sDataQuoteLineItem.GetNamedString("InventoryItemId") ==
                                                            QuoteLineItemList[i].ProductId)
                                                        {
                                                            await
                                                                _sageSalesDB.QueryAsync<QuoteLineItem>(
                                                                    "Update QuoteLineItem Set QuoteLineItemId=? where QuoteLineItemId=?",
                                                                    sDataQuoteLineItem.GetNamedString("$key"),
                                                                    QuoteLineItemList[i].QuoteLineItemId);
                                                            QuoteLineItemList[i].QuoteLineItemId =
                                                                sDataQuoteLineItem.GetNamedString("$key");
                                                            await
                                                                UpdateQuoteLineItemJsonToDbAsync(
                                                                    sDataQuoteLineItem, QuoteLineItemList[i]);
                                                            break;
                                                        }
                                                    }
                                                    //}
                                                }
                                            }
                                            //await _sageSalesDB.QueryAsync<QuoteLineItem>("Update QuoteLineItem Set QuoteLineItemId=? where QuoteLineItemId=?", sDataQuoteLineItem.GetNamedString("$key"), QuoteLineItemList[quoteLineItem].QuoteLineItemId);
                                            //QuoteLineItemList[quoteLineItem].QuoteLineItemId = sDataQuoteLineItem.GetNamedString("$key");
                                            //await UpdateQuoteLineItemJsonToDbAsync(sDataQuoteLineItem, QuoteLineItemList[quoteLineItem]);
                                        }
                                    }
                                    else
                                    {
                                        // worst case
                                        await AddQuoteLineItemJsonToDbAsync(sDataQuoteLineItem, quoteId);
                                    }
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
        }

        /// <summary>
        ///     Gets QuoteLineItem list along with Product(Product Details), ProductAssociatedBlob(Url)
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task<List<LineItemDetails>> GetQuoteLineItemDetailsAsync(string quoteId)
        {
            List<LineItemDetails> quoteLineItemList = null;
            try
            {
                quoteLineItemList =
                    await
                        _sageSalesDB.QueryAsync<LineItemDetails>(
                            "SELECT distinct Product.ProductId, QuoteLT.QuoteLineItemId as LineItemId, QuoteLT.ProductId as ProductId, QuoteLT.QuoteId as LineId, QuoteLT.Price as LineItemPrice,QuoteLT.Quantity as LineItemQuantity, Product.ProductName, Product.Quantity as ProductQuantity, Product.Sku as ProductSku, (select PAB.url from  ProductAssociatedBlob as PAB where Product.ProductId=PAB.ProductId And (PAB.IsPrimary=1 OR PAB.IsPrimary=0)) as Url  from QuoteLineItem as QuoteLT Join Product  on QuoteLT.ProductId=Product.ProductId where QuoteLT.QuoteId=? and QuoteLT.IsDeleted='0'",
                            quoteId);
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
            return quoteLineItemList;
        }

        /// <summary>
        ///     Gets QuoteLineItem details for specific Lineitem
        /// </summary>
        /// <param name="quoteLineItemId"></param>
        /// <returns></returns>
        public async Task<QuoteLineItem> GetQuoteLineAsync(string quoteLineItemId)
        {
            List<QuoteLineItem> quoteLineItemList = null;
            try
            {
                quoteLineItemList =
                    await
                        _sageSalesDB.QueryAsync<QuoteLineItem>(
                            "Select * from QuoteLineItem where QuoteLineItem.QuoteLineItemId=?", quoteLineItemId);
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
            return quoteLineItemList.FirstOrDefault();
        }

        /// <summary>
        ///     Gets quoteLineItems for the quote
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task<List<QuoteLineItem>> GetQuoteLineItemsForQuote(string quoteId)
        {
            List<QuoteLineItem> quoteLineItemList = null;
            try
            {
                quoteLineItemList =
                    await _sageSalesDB.QueryAsync<QuoteLineItem>("Select * from QuoteLineItem where QuoteId=?", quoteId);
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
            return quoteLineItemList;
        }

        /// <summary>
        ///     Add quoteLineItem to local dB
        /// </summary>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        public async Task AddQuoteLineItemToDbAsync(QuoteLineItem quoteLineItem)
        {
            try
            {
                await _sageSalesDB.InsertAsync(quoteLineItem);
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
        ///     Update quoteLineItem in local dB
        /// </summary>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        public async Task UpdateQuoteLineItemToDbAsync(QuoteLineItem quoteLineItem)
        {
            try
            {
                await _sageSalesDB.UpdateAsync(quoteLineItem);
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
        ///     Gets quoteLineItem if already exists for the quote
        /// </summary>
        /// <param name="quoteId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<QuoteLineItem> GetQuoteLineItemIfExistsForQuote(string quoteId, string productId)
        {
            try
            {
                List<QuoteLineItem> quoteLineItemList;
                quoteLineItemList =
                    await
                        _sageSalesDB.QueryAsync<QuoteLineItem>(
                            "Select * from QuoteLineItem where QuoteId=? and IsDeleted='0'", quoteId);

                foreach (QuoteLineItem quoteLineItem in quoteLineItemList)
                {
                    if (quoteLineItem.ProductId.Equals(productId))
                        return quoteLineItem;
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
        ///     Gets pending quoteLineItems, yet to sync
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuoteLineItem>> GetPendingQuoteLineItems()
        {
            List<QuoteLineItem> quoteLineItemPendingList = null;
            try
            {
                quoteLineItemPendingList =
                    await
                        _sageSalesDB.QueryAsync<QuoteLineItem>(
                            "Select * From QuoteLineItem Where QuoteLineItemId like ('Pending%' or IsPending='1') and IsDeleted='0' and Quantity>0");
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
            return quoteLineItemPendingList;
        }

        /// <summary>
        ///     Gets deleted quoteLineItems which are not synced
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuoteLineItem>> GetDeletedQuoteLineItems()
        {
            List<QuoteLineItem> quoteLineItemDeleteList = null;
            try
            {
                quoteLineItemDeleteList =
                    await _sageSalesDB.QueryAsync<QuoteLineItem>("Select * from QuoteLineItem where IsDeleted='1'");
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
            return quoteLineItemDeleteList;
        }

        /// <summary>
        ///     Mark quoteLineItems as deleted to support offline capability
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task MarkQuoteLineItemsAsDeleted(string quoteId)
        {
            try
            {
                await
                    _sageSalesDB.QueryAsync<QuoteLineItem>("Update QuoteLineItem Set isDeleted='1' where QuoteId=?",
                        quoteId);
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
        ///     Mark quoteLineItem as deleted to support offline capability
        /// </summary>
        /// <param name="quoteLineItemId"></param>
        /// <returns></returns>
        public async Task MarkQuoteLineItemAsDeleted(string quoteLineItemId)
        {
            try
            {
                await
                    _sageSalesDB.QueryAsync<QuoteLineItem>(
                        "Update QuoteLineItem Set isDeleted='1' where QuoteLineItemId=?", quoteLineItemId);
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
        ///     Deletes quoteLineItems for that quoteId from local dB
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task DeleteQuoteLineItemsFromDbAsync(string quoteId)
        {
            try
            {
                await _sageSalesDB.QueryAsync<QuoteLineItem>("Delete From QuoteLineItem Where QuoteId=?", quoteId);
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
        ///     Deletes quoteLineItem from local dB
        /// </summary>
        /// <param name="quoteLineItemId"></param>
        /// <returns></returns>
        public async Task DeleteQuoteLineItemFromDbAsync(string quoteLineItemId)
        {
            try
            {
                await
                    _sageSalesDB.QueryAsync<QuoteLineItem>("Delete From QuoteLineItem Where QuoteLineItemId=?",
                        quoteLineItemId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
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
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Compares list of quoteLineItems with the json and local dB to delete, add or update
        /// </summary>
        /// <param name="sDataAddressArray"></param>
        /// <param name="customerId"></param>
        private async Task SaveQuoteLineItemDetailsAsync(JsonArray sDataQuoteLineItemsArray, string quoteId)
        {
            await DeleteQuoteLineItemsJsonFromDbAsync(sDataQuoteLineItemsArray, quoteId);

            foreach (IJsonValue quoteLineItem in sDataQuoteLineItemsArray)
            {
                JsonObject sDataQuoteLineItem = quoteLineItem.GetObject();
                await AddOrUpdateQuoteLineItemJsonToDbAsync(sDataQuoteLineItem, quoteId);
            }
        }

        /// <summary>
        ///     Adds or updates quoteLineItem json response to dB
        /// </summary>
        /// <param name="sDataQuoteLineItem"></param>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        private async Task<QuoteLineItem> AddOrUpdateQuoteLineItemJsonToDbAsync(JsonObject sDataQuoteLineItem,
            string quoteId)
        {
            try
            {
                IJsonValue value;
                if (sDataQuoteLineItem.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<QuoteLineItem> quoteLineItemList;
                        quoteLineItemList =
                            await
                                _sageSalesDB.QueryAsync<QuoteLineItem>(
                                    "SELECT * FROM QuoteLineItem where QuoteLineItemId=?",
                                    sDataQuoteLineItem.GetNamedString("$key"));

                        if (quoteLineItemList.FirstOrDefault() != null)
                        {
                            return
                                await
                                    UpdateQuoteLineItemJsonToDbAsync(sDataQuoteLineItem,
                                        quoteLineItemList.FirstOrDefault());
                        }
                        return await AddQuoteLineItemJsonToDbAsync(sDataQuoteLineItem, quoteId);
                    }
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
        ///     Adds quoteLineItem json response to dB
        /// </summary>
        /// <param name="sDataQuoteLineItem"></param>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        private async Task<QuoteLineItem> AddQuoteLineItemJsonToDbAsync(JsonObject sDataQuoteLineItem, string quoteId)
        {
            var quoteLineItemObj = new QuoteLineItem();
            try
            {
                quoteLineItemObj.QuoteId = quoteId;
                quoteLineItemObj.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("$key");

                quoteLineItemObj = await ExtractQuoteLineItemFromJsonAsync(sDataQuoteLineItem, quoteLineItemObj);
                await _sageSalesDB.InsertAsync(quoteLineItemObj);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteLineItemObj;
        }

        /// <summary>
        ///     Updates quoteLineItem json response to dB
        /// </summary>
        /// <param name="sDataQuoteLineItem"></param>
        /// <param name="quoteLineItemDbObj"></param>
        /// <returns></returns>
        private async Task<QuoteLineItem> UpdateQuoteLineItemJsonToDbAsync(JsonObject sDataQuoteLineItem,
            QuoteLineItem quoteLineItemDbObj)
        {
            try
            {
                quoteLineItemDbObj = await ExtractQuoteLineItemFromJsonAsync(sDataQuoteLineItem, quoteLineItemDbObj);
                await _sageSalesDB.UpdateAsync(quoteLineItemDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteLineItemDbObj;
        }

        /// <summary>
        ///     Extract quoteLineItem json response
        /// </summary>
        /// <param name="sDataQuoteLineItem"></param>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        private async Task<QuoteLineItem> ExtractQuoteLineItemFromJsonAsync(JsonObject sDataQuoteLineItem,
            QuoteLineItem quoteLineItem)
        {
            try
            {
                IJsonValue value;

                if (sDataQuoteLineItem.TryGetValue("Price", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.Price = Convert.ToDecimal(sDataQuoteLineItem.GetNamedNumber("Price"));
                    }
                }
                if (sDataQuoteLineItem.TryGetValue("Quantity", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.Quantity =
                            Convert.ToInt32(Convert.ToDecimal(sDataQuoteLineItem.GetNamedNumber("Quantity")));
                    }
                }
                if (sDataQuoteLineItem.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("$key");
                    }
                }

                if (sDataQuoteLineItem.TryGetValue("InventoryItem", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataInventoryItem = sDataQuoteLineItem.GetNamedObject("InventoryItem");
                        await _productRepository.AddOrUpdateProductJsonToDbAsync(sDataInventoryItem);
                        //await _productRepository.AddOrUpdateProductJsonToDbAsync(sDataQuoteLineItem);
                    }
                }

                //Need to change based on the response
                if (sDataQuoteLineItem.TryGetValue("InventoryItemId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.ProductId = sDataQuoteLineItem.GetNamedString("InventoryItemId");

                        List<Product> productDb =
                            await
                                _sageSalesDB.QueryAsync<Product>("SELECT * FROM Product WHERE ProductId=?",
                                    sDataQuoteLineItem.GetNamedString("InventoryItemId"));

                        if (productDb.FirstOrDefault() != null)
                        {
                            quoteLineItem.ProductId = productDb.FirstOrDefault().ProductId;
                        }
                        else
                        {
                            var product = new Product();
                            product.ProductId = sDataQuoteLineItem.GetNamedString("InventoryItemId");
                            quoteLineItem.ProductId = product.ProductId;

                            await _sageSalesDB.InsertAsync(product);
                        }
                    }
                }

                quoteLineItem.IsPending = false;
            }


            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteLineItem;
        }


        /// <summary>
        ///     Deletes quoteLineItems from dB which exists in dB but not in updated json response
        /// </summary>
        /// <param name="sDataQuoteLineItemArray"></param>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        private async Task DeleteQuoteLineItemsJsonFromDbAsync(JsonArray sDataQuoteLineItemArray, string quoteId)
        {
            IJsonValue value;
            List<QuoteLineItem> quoteLineItemIdJsonList;
            List<QuoteLineItem> quoteLineItemIdDbList;
            List<QuoteLineItem> quoteLineItemRemoveList;
            bool idExists = false;


            try
            {
                // Retrieve list of addressId from Json
                quoteLineItemIdJsonList = new List<QuoteLineItem>();
                foreach (IJsonValue quoteLineItem in sDataQuoteLineItemArray)
                {
                    JsonObject sDataQuoteLineItem = quoteLineItem.GetObject();
                    var quoteLineItemJsonObj = new QuoteLineItem();
                    if (sDataQuoteLineItem.TryGetValue("$key", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            quoteLineItemJsonObj.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("$key");
                        }
                    }
                    quoteLineItemIdJsonList.Add(quoteLineItemJsonObj);
                }

                //Retrieve list of addressId from dB
                quoteLineItemIdDbList = new List<QuoteLineItem>();
                quoteLineItemRemoveList = new List<QuoteLineItem>();
                quoteLineItemIdDbList =
                    await _sageSalesDB.QueryAsync<QuoteLineItem>("SELECT * FROM QuoteLineItem where quoteId=?", quoteId);

                for (int i = 0; i < quoteLineItemIdDbList.Count; i++)
                {
                    idExists = false;
                    for (int j = 0; j < quoteLineItemIdJsonList.Count; j++)
                    {
                        if (quoteLineItemIdDbList[i].QuoteLineItemId.Contains(DataAccessUtils.Pending))
                        {
                            idExists = true;
                            break;
                        }
                        if (quoteLineItemIdDbList[i].QuoteLineItemId == quoteLineItemIdJsonList[j].QuoteLineItemId)
                        {
                            idExists = true;
                            break;
                        }
                    }
                    if (!idExists)
                        quoteLineItemRemoveList.Add(quoteLineItemIdDbList[i]);
                }

                if (quoteLineItemRemoveList.Count() > 0)
                {
                    foreach (QuoteLineItem quoteLineItemRemove in quoteLineItemRemoveList)
                    {
                        await _sageSalesDB.DeleteAsync(quoteLineItemRemove);
                    }
                }
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion
    }
}