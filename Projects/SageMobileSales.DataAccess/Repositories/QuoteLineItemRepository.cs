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
        ///     Extracts QuoteLineItem from json response for that quote
        /// </summary>
        /// <param name="sDataProduct"></param>
        /// <returns>Product</returns>
        public async Task SaveQuoteLineItemsAsync(JsonObject sDataQuote, string quoteId)
        {
            try
            {              
                 if (!string.IsNullOrEmpty(quoteId))
                 {                  
                     JsonArray sDataQuoteLineItemsArray = sDataQuote.GetNamedArray("Details");
                             if (sDataQuoteLineItemsArray.Count > 0)
                             {
                                 await SaveQuoteLineItemDetailsAsync(sDataQuoteLineItemsArray, quoteId);
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

        public async Task SavePostedQuoteLineItemsAsync(JsonObject sDataQuote, string quoteId,
            QuoteLineItem pendingQuoteLineItem)
        {
            try
            {
                JsonObject sDataQuoteLineItems = sDataQuote.GetNamedObject("Details");
                if (sDataQuoteLineItems.ContainsKey("$resources"))
                {
                    JsonArray sDataQuoteLineItemsArray = sDataQuoteLineItems.GetNamedArray("$resources");
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
                                            if (sDataQuoteLineItem.TryGetValue("InventoryItem", out value))
                                            {
                                                if (value.ValueType.ToString() != DataAccessUtils.Null)
                                                {
                                                    JsonObject sDataProduct =
                                                        sDataQuoteLineItem.GetNamedObject("InventoryItem");
                                                    if (sDataProduct.GetNamedValue("$key").ValueType.ToString() !=
                                                        DataAccessUtils.Null)
                                                    {
                                                        for (int i = 0; i < QuoteLineItemList.Count; i++)
                                                        {
                                                            if (sDataProduct.GetNamedString("$key") ==
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
                                                    }
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
        ///     Gets QuoteLineItems List along with Product(Product Details), ProductAssociatedBlob(Url)
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
                            "SELECT distinct Product.ProductId, QuoteLT.QuoteLineItemId as LineItemId, QuoteLT.QuoteId as LineId, QuoteLT.Price as LineItemPrice,QuoteLT.Quantity as LineItemQuantity, Product.ProductName, Product.Quantity as ProductQuantity, Product.Sku as ProductSku, (select PAB.url from  ProductAssociatedBlob as PAB where Product.ProductId=PAB.ProductId And PAB.IsPrimary=1) as Url  from QuoteLineItem as QuoteLT Join Product  on QuoteLT.ProductId=Product.ProductId where QuoteLT.QuoteId=? and QuoteLT.IsDeleted='0'",
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
        ///     Returns quoteLineItems for the quote
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
        ///     Updates quoteLineItem in local dB
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
        ///     Returns quoteLineItem if already exists for the quote
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
        ///     Returns pending quoteLineItems which are not synced
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
        ///     Returns deleted quoteLineItems which are not synced
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
        ///     Mark quoteLineItems for the as deleted to support offline capability
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
        ///     Deletes quoteLineItems for the quote from local dB
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
        ///     Compares list of quoteLineItems with the Json response and local Db to delete, add or update
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
                if (sDataQuoteLineItem.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<QuoteLineItem> quoteLineItemList;
                        quoteLineItemList =
                            await
                                _sageSalesDB.QueryAsync<QuoteLineItem>(
                                    "SELECT * FROM QuoteLineItem where QuoteLineItemId=?",
                                    sDataQuoteLineItem.GetNamedString("Id"));

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
                quoteLineItemObj.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("Id");

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
        ///     Extracts quoteLineItem json response
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
                        quoteLineItem.Price = Convert.ToDecimal(sDataQuoteLineItem.GetNamedString("Price"));
                    }
                }
                if (sDataQuoteLineItem.TryGetValue("Quantity", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.Quantity = Convert.ToInt32(Convert.ToDecimal(sDataQuoteLineItem.GetNamedString("Quantity")));
                    }
                }
                if (sDataQuoteLineItem.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("Id");
                    }
                }
             
                if (sDataQuoteLineItem.TryGetValue("InventoryItemId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quoteLineItem.ProductId = sDataQuoteLineItem.GetNamedString("InventoryItemId");
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
                    if (sDataQuoteLineItem.TryGetValue("Id", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            quoteLineItemJsonObj.QuoteLineItemId = sDataQuoteLineItem.GetNamedString("Id");
                        }
                    }
                    quoteLineItemIdJsonList.Add(quoteLineItemJsonObj);
                }

                //Retrieve list of addressId from dB
                quoteLineItemIdDbList = new List<QuoteLineItem>();
                quoteLineItemRemoveList = new List<QuoteLineItem>();
                quoteLineItemIdDbList =
                    await _sageSalesDB.QueryAsync<QuoteLineItem>("SELECT * FROM QuoteLineItem where quoteId=?", quoteId);

                // Requires enhancement
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

                //if (quoteLineItemIdDbList != null)
                //{
                //    var quoteLineItemRemoveList = quoteLineItemIdDbList.Except(quoteLineItemIdJsonList, new QuoteLineItemIdComparer()).ToList();
                if (quoteLineItemRemoveList.Count() > 0)
                {
                    foreach (QuoteLineItem quoteLineItemRemove in quoteLineItemRemoveList)
                    {
                        await _sageSalesDB.DeleteAsync(quoteLineItemRemove);
                    }
                }
                //}
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