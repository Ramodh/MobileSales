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
    public class FrequentlyPurchasedItemRepository : IFrequentlyPurchasedItemRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public FrequentlyPurchasedItemRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        # region public metods

        public async Task SaveFrequentlyPurchasedItemsAsync(JsonObject sDataFrequentlyPurchasedItem)
        {
            try
            {
                IJsonValue value;
                if (sDataFrequentlyPurchasedItem.TryGetValue("$resources", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonArray sDataFrequentlyPurchasedItemArray = sDataFrequentlyPurchasedItem.GetNamedArray("$resources");
                        if (sDataFrequentlyPurchasedItemArray.Count > 0)
                        {
                            await SaveFrequentlyPurchasedItemDetailsAsync(sDataFrequentlyPurchasedItemArray);
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

        public async Task<List<FrequentlyPurchasedItem>> GetFrequentlyPurchasedItems(string customerId)
        {
            List<FrequentlyPurchasedItem> FrequentlyPurchasedItemList = null;
            try
            {
                FrequentlyPurchasedItemList =
                    await _sageSalesDB.QueryAsync<FrequentlyPurchasedItem>("Select * from FrequentlyPurchasedItem where CustomerId=?", customerId);           
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
            return FrequentlyPurchasedItemList;
        }

        #endregion

        #region private methods

        private async Task SaveFrequentlyPurchasedItemDetailsAsync(JsonArray sDataFrequentlyPurchasedItemArray)
        {
            //Need to confirm whether deletion need to be handled


            foreach (IJsonValue frequentlyPurchasedItem in sDataFrequentlyPurchasedItemArray)
            {
                JsonObject sDataFrequentlyPurchasedItem = frequentlyPurchasedItem.GetObject();
                await AddOrUpdateFrequentlyPurchasedItemJsonToDbAsync(sDataFrequentlyPurchasedItem);
            }

        }

        private async Task AddOrUpdateFrequentlyPurchasedItemJsonToDbAsync(JsonObject sDataFrequentlyPurchasedItem)
        {
            try
            {
                IJsonValue value;
                if (sDataFrequentlyPurchasedItem.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<FrequentlyPurchasedItem> frequentlyPurchasedItemList;
                        frequentlyPurchasedItemList =
                            await
                                _sageSalesDB.QueryAsync<FrequentlyPurchasedItem>("SELECT * FROM FrequentlyPurchasedItem where FrequentlyPurchasedItemId=?",
                                    sDataFrequentlyPurchasedItem.GetNamedString("$key"));

                        if (frequentlyPurchasedItemList.FirstOrDefault() != null)
                        {
                            await UpdateFrequentlyPurchasedItemJsonToDbAsync(sDataFrequentlyPurchasedItem, frequentlyPurchasedItemList.FirstOrDefault());
                        }
                        else
                        {
                            await AddFrequentlyPurchasedItemJsonToDbAsync(sDataFrequentlyPurchasedItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async Task UpdateFrequentlyPurchasedItemJsonToDbAsync(JsonObject sDataFrequentlyPurchasedItem, FrequentlyPurchasedItem frequentlyPurchasedItemDbObj)
        {
            try
            {
                frequentlyPurchasedItemDbObj = ExtractFrequentlyPurchasedItemFromJsonAsync(sDataFrequentlyPurchasedItem, frequentlyPurchasedItemDbObj);
                await _sageSalesDB.UpdateAsync(frequentlyPurchasedItemDbObj);
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

        private async Task AddFrequentlyPurchasedItemJsonToDbAsync(JsonObject sDataFrequentlyPurchasedItem)
        {
            var frequentlyPurchasedItemObj = new FrequentlyPurchasedItem();
            try
            {
                //frequentlyPurchasedItemObj.CustomerId = customerId;
                frequentlyPurchasedItemObj.FrequentlyPurchasedItemId = sDataFrequentlyPurchasedItem.GetNamedString("$key");

                frequentlyPurchasedItemObj = ExtractFrequentlyPurchasedItemFromJsonAsync(sDataFrequentlyPurchasedItem, frequentlyPurchasedItemObj);
                await _sageSalesDB.InsertAsync(frequentlyPurchasedItemObj);
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

        private FrequentlyPurchasedItem ExtractFrequentlyPurchasedItemFromJsonAsync(JsonObject sDataFrequentlyPurchasedItem, FrequentlyPurchasedItem frequentlyPurchasedItem)
        {
            try
            {
                IJsonValue value;

                if (sDataFrequentlyPurchasedItem.TryGetValue("NumberOfInvoices", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.NumberOfInvoices = Convert.ToInt16(sDataFrequentlyPurchasedItem.GetNamedNumber("NumberOfInvoices"));
                    }
                }

                if (sDataFrequentlyPurchasedItem.TryGetValue("ItemId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.ItemId = sDataFrequentlyPurchasedItem.GetNamedString("ItemId");
                    }
                }

                if (sDataFrequentlyPurchasedItem.TryGetValue("CustomerId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.CustomerId = sDataFrequentlyPurchasedItem.GetNamedString("CustomerId");
                    }
                }
                if (sDataFrequentlyPurchasedItem.TryGetValue("QuantityYtd", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.QuantityYtd = Convert.ToInt16(sDataFrequentlyPurchasedItem.GetNamedNumber("QuantityYtd"));
                    }
                }

                if (sDataFrequentlyPurchasedItem.TryGetValue("QuantityPriorYtd", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.QuantityPriorYtd = Convert.ToInt16(sDataFrequentlyPurchasedItem.GetNamedNumber("QuantityPriorYtd"));
                    }
                }
                if (sDataFrequentlyPurchasedItem.TryGetValue("ItemNumber", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.ItemNumber = sDataFrequentlyPurchasedItem.GetNamedString("ItemNumber");
                    }
                }
                if (sDataFrequentlyPurchasedItem.TryGetValue("ItemDescription", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.ItemDescription = sDataFrequentlyPurchasedItem.GetNamedString("ItemDescription");
                    }
                }
                if (sDataFrequentlyPurchasedItem.TryGetValue("EntityStatus", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.EntityStatus = sDataFrequentlyPurchasedItem.GetNamedString("EntityStatus");
                    }
                }
                if (sDataFrequentlyPurchasedItem.TryGetValue("IsDeleted", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        frequentlyPurchasedItem.IsDeleted = sDataFrequentlyPurchasedItem.GetNamedBoolean("IsDeleted");
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
            return frequentlyPurchasedItem;
        }

        #endregion
    }
}
