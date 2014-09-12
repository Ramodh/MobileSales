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
    public class SalesHistoryRepository : ISalesHistoryRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public SalesHistoryRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        #region public methods

        /// <summary>
        /// Extract salesHistory, save to local dB
        /// </summary>
        /// <param name="sDataSalesHistory"></param>
        /// <returns></returns>
        public async Task SaveSalesHistoryAsync(JsonObject sDataSalesHistory)
        {
            try
            {
                IJsonValue value;
                if (sDataSalesHistory.TryGetValue("$resources", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonArray sDataSalesHistoryArray = sDataSalesHistory.GetNamedArray("$resources");
                        if (sDataSalesHistoryArray.Count > 0)
                        {
                            await SaveSalesHistoryDetailsAsync(sDataSalesHistoryArray);
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
        /// Gets sales history for that customerId, productId
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <param name="ProductId"></param>
        /// <returns></returns>
        public async Task<List<SalesHistory>> GetCustomerProductSalesHistory(string CustomerId, string ProductId)
        {
            List<SalesHistory> salesHistoryList = null;
            try
            {
                salesHistoryList =
                    await
                        _sageSalesDB.QueryAsync<SalesHistory>(
                            "Select * from SalesHistory where SalesHistory.CustomerId=? and SalesHistory.ProductId=?",
                            CustomerId, ProductId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return salesHistoryList;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Save sDataSaleHistory json to salesHistory table
        /// </summary>
        /// <param name="sDataSalesHistoryArray"></param>
        /// <returns></returns>
        private async Task SaveSalesHistoryDetailsAsync(JsonArray sDataSalesHistoryArray)
        {
            // Delete - Need confirmation
            //await DeleteSalesHistoryFromDbAsync(sDataSalesHistoryArray, customerId);

            foreach (IJsonValue salesHistory in sDataSalesHistoryArray)
            {
                JsonObject sDataSalesHistory = salesHistory.GetObject();
                await AddOrUpdatesalesHistoryJsonToDbAsync(sDataSalesHistory);
            }
        }
        /// <summary>
        /// Add or update salesHistory to local dB
        /// </summary>
        /// <param name="sDataSalesHistory"></param>
        /// <returns></returns>
        public async Task<SalesHistory> AddOrUpdatesalesHistoryJsonToDbAsync(JsonObject sDataSalesHistory)
        {
            try
            {
                IJsonValue value;
                if (sDataSalesHistory.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<SalesHistory> salesHistoryList;
                        salesHistoryList =
                            await
                                _sageSalesDB.QueryAsync<SalesHistory>(
                                    "SELECT * FROM SalesHistory where SalesHistoryId=?",
                                    sDataSalesHistory.GetNamedString("$key"));

                        if (salesHistoryList.FirstOrDefault() != null)
                        {
                            return
                                await
                                    UpdateSalesHistoryJsonToDbAsync(sDataSalesHistory, salesHistoryList.FirstOrDefault());
                        }
                        return await AddSalesHistoryJsonToDbAsync(sDataSalesHistory);
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
        /// Update salesHistory to local dB
        /// </summary>
        /// <param name="sDataSalesHistory"></param>
        /// <param name="salesHistoryDbObj"></param>
        /// <returns></returns>
        private async Task<SalesHistory> UpdateSalesHistoryJsonToDbAsync(JsonObject sDataSalesHistory,
            SalesHistory salesHistoryDbObj)
        {
            try
            {
                salesHistoryDbObj = ExtractSalesHistoryFromJsonAsync(sDataSalesHistory, salesHistoryDbObj);
                await _sageSalesDB.UpdateAsync(salesHistoryDbObj);
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
            return salesHistoryDbObj;
        }

        /// <summary>
        /// Add salesHistory to local dB
        /// </summary>
        /// <param name="sDataSalesHistory"></param>
        /// <returns></returns>
        private async Task<SalesHistory> AddSalesHistoryJsonToDbAsync(JsonObject sDataSalesHistory)
        {
            var salesHistoryObj = new SalesHistory();
            try
            {
                //salesHistoryObj.CustomerId = customerId;
                salesHistoryObj.SalesHistoryId = sDataSalesHistory.GetNamedString("$key");

                salesHistoryObj = ExtractSalesHistoryFromJsonAsync(sDataSalesHistory, salesHistoryObj);
                await _sageSalesDB.InsertAsync(salesHistoryObj);
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
            return salesHistoryObj;
        }

        /// <summary>
        /// Extract salesHistory from json
        /// </summary>
        /// <param name="sDataSalesHistory"></param>
        /// <param name="salesHistory"></param>
        /// <returns></returns>
        private SalesHistory ExtractSalesHistoryFromJsonAsync(JsonObject sDataSalesHistory, SalesHistory salesHistory)
        {
            try
            {
                IJsonValue value;
                if (sDataSalesHistory.TryGetValue("CustomerId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.CustomerId = sDataSalesHistory.GetNamedString("CustomerId");
                    }
                }

                if (sDataSalesHistory.TryGetValue("InvoiceId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.InvoiceId = sDataSalesHistory.GetNamedString("InvoiceId");
                    }
                }

                if (sDataSalesHistory.TryGetValue("InvoiceNumber", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.InvoiceNumber = sDataSalesHistory.GetNamedString("InvoiceNumber");
                    }
                }
                if (sDataSalesHistory.TryGetValue("ItemId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.ProductId = sDataSalesHistory.GetNamedString("ItemId");
                    }
                }

                if (sDataSalesHistory.TryGetValue("ItemDescription", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.ProductDescription = sDataSalesHistory.GetNamedString("ItemDescription");
                    }
                }
                if (sDataSalesHistory.TryGetValue("Quantity", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.Quantity = Convert.ToDecimal(sDataSalesHistory.GetNamedNumber("Quantity"));
                    }
                }
                if (sDataSalesHistory.TryGetValue("Price", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.Price = Convert.ToDecimal(sDataSalesHistory.GetNamedNumber("Price"));
                    }
                }
                if (sDataSalesHistory.TryGetValue("Total", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.Total = Convert.ToDecimal(sDataSalesHistory.GetNamedNumber("Total"));
                    }
                }
                if (sDataSalesHistory.TryGetValue("UnitOfMeasure", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.UnitOfMeasure = sDataSalesHistory.GetNamedString("UnitOfMeasure");
                    }
                }
                if (sDataSalesHistory.TryGetValue("IsDeleted", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesHistory.IsDeleted = sDataSalesHistory.GetNamedBoolean("IsDeleted");
                    }
                }

                if (sDataSalesHistory.TryGetValue("InvoiceDate", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        //salesHistory.InvoiceDate =
                        //    ConvertJsonStringToDateTime(sDataSalesHistory.GetNamedString("InvoiceDate"));
                        salesHistory.InvoiceDate =
                            DateTime.Parse(sDataSalesHistory.GetNamedString("InvoiceDate"));
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
            return salesHistory;
        }

        //private DateTime ConvertJsonStringToDateTime(string jsonTime)
        //{
        //    if (!string.IsNullOrEmpty(jsonTime) && jsonTime.IndexOf("Date") > -1)
        //    {
        //        string milis = jsonTime.Substring(jsonTime.IndexOf("(") + 1);
        //        string sign = milis.IndexOf("+") > -1 ? "+" : "-";
        //        string hours = "";
        //        // Need to change based on GMT........ To be Confirmed
        //        if (milis.IndexOf(sign) > -1)
        //        {
        //            hours = milis.Substring(milis.IndexOf(sign));
        //            milis = milis.Substring(0, milis.IndexOf(sign));
        //            hours = hours.Substring(0, hours.IndexOf(")"));
        //            return
        //                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
        //                    .AddHours(Convert.ToInt64(hours)/100);
        //        }
        //        hours = "0";
        //        milis = milis.Substring(0, milis.IndexOf(")"));
        //        return
        //            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
        //                .AddHours(Convert.ToInt64(hours)/100);
        //    }

        //    return DateTime.Now;
        //}

        #endregion
    }
}