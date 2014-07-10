﻿using System;
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
    public class OrderLineItemRepository : IOrderLineItemRepository
    {
        private readonly IDatabase _database;
        private readonly IProductRepository _productRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public OrderLineItemRepository(IDatabase database, IProductRepository productRepository)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _productRepository = productRepository;
        }

        # region public methods

        /// <summary>
        ///     Extracts orderLineItem from Json Response for that order
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task SaveOrderLineItemsAsync(JsonObject sDataOrder, string orderId)
        {
            try
            {
                if (!string.IsNullOrEmpty(orderId))
                {
                    JsonObject sDataOrdeLineItems = sDataOrder.GetNamedObject("Details");
                    if (sDataOrdeLineItems.ContainsKey("$resources"))
                    {
                        JsonArray sDataOrderLineItemArray = sDataOrdeLineItems.GetNamedArray("$resources");
                        if (sDataOrderLineItemArray.Count > 0)
                        {
                            await SaveOrderLineItemDetailsAsync(sDataOrderLineItemArray, orderId);
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

        /// <summary>
        ///     Gets OrderLineItems List along with Product(Product Details), ProductAssociatedBlob(Url)
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<List<LineItemDetails>> GetOrderLineItemDetailsAsync(string orderId)
        {
            List<LineItemDetails> LineItemList = null;
            try
            {
                LineItemList =
                    await
                        _sageSalesDB.QueryAsync<LineItemDetails>(
                            "SELECT distinct OrderLT.Price as LineItemPrice,OrderLT.Quantity as LineItemQuantity, Product.ProductName, Product.Sku as ProductSku,(select PAB.url from  ProductAssociatedBlob as PAB where Product.ProductId=PAB.ProductId And PAB.IsPrimary=1) as Url from OrderLineItem as OrderLT Join Product  on OrderLT.ProductId=Product.ProductId where OrderLT.OrderId=?",
                            orderId);
                //LineItemList = await _sageSalesDB.QueryAsync<LineItemDetails>("SELECT distinct OrderLT.Price as LineItemPrice,OrderLT.Quantity as LineItemQuantity, Product.ProductName, Product.Sku as ProductSku, PAB.Url  from OrderLineItem as OrderLT Join Product  on OrderLT.ProductId=Product.ProductId join ProductAssociatedBlob as PAB on (Product.ProductId=PAB.ProductId And PAB.IsPrimary=1) where OrderLT.OrderId=?", orderId);                                                                                 
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
            return LineItemList;
        }

        /// <summary>
        ///     Returns list of previously purchased products for the customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<OrderLineItem>> GetPreviouslyPurchasedProducts(string customerId)
        {
            List<OrderLineItem> previouslyPurchasedProductList = null;
            try
            {
                //SELECT ProductId, Price FROM OrderLineItem WHERE OrderId in (SELECT OrderId FROM Orders WHERE CustomerId=?
                previouslyPurchasedProductList =
                    await
                        _sageSalesDB.QueryAsync<OrderLineItem>(
                            "SELECT distinct OLT.ProductId, MAX(OLT.price) as Price FROM OrderLineItem as OLT WHERE OLT.OrderId in (SELECT OrderId FROM Orders WHERE CustomerId=?) Group by OLT.ProductId",
                            customerId);
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
            return previouslyPurchasedProductList;
        }

        /// <summary>
        ///     Returns list of orderlineitems for the order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<List<OrderLineItem>> GetOrderLineItemForOrder(string orderId)
        {
            List<OrderLineItem> orderLineItemList = null;
            try
            {
                orderLineItemList =
                    await _sageSalesDB.QueryAsync<OrderLineItem>("SELECT * FROM OrderLineItem WHERE OrderId=?", orderId);
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
            return orderLineItemList;
        }

        /// <summary>
        ///     Adds or updates OrderLineItem json response to dB
        /// </summary>
        /// <param name="sDataOrderLineItem"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<OrderLineItem> AddOrUpdateOrderLineItemJsonToDbAsync(JsonObject sDataOrderLineItem,
            string orderId)
        {
            try
            {
                IJsonValue value;
                if (sDataOrderLineItem.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<OrderLineItem> orderLineItemList;
                        orderLineItemList =
                            await
                                _sageSalesDB.QueryAsync<OrderLineItem>(
                                    "SELECT * FROM OrderLineItem where OrderLineItemId=?",
                                    sDataOrderLineItem.GetNamedString("$key"));

                        if (orderLineItemList.FirstOrDefault() != null)
                        {
                            return
                                await
                                    UpdateOrderLineItemJsonToDbAsync(sDataOrderLineItem,
                                        orderLineItemList.FirstOrDefault());
                        }
                        return await AddOrderLineItemJsonToDbAsync(sDataOrderLineItem, orderId);
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

        #endregion

        #region private methods

        /// <summary>
        ///     Compares list of orderLineItems with the Json response and local Db to delete, add or update
        /// </summary>
        /// <param name="sDataOrderLineItemArray"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private async Task SaveOrderLineItemDetailsAsync(JsonArray sDataOrderLineItemArray, string orderId)
        {
            // Delete if any address exists in dB but not in Json by comparing addressId
            await DeleteOrderLineItemsFromDbAsync(sDataOrderLineItemArray, orderId);

            foreach (IJsonValue orderLineItem in sDataOrderLineItemArray)
            {
                JsonObject sDataOrderLineItem = orderLineItem.GetObject();
                await AddOrUpdateOrderLineItemJsonToDbAsync(sDataOrderLineItem, orderId);
            }
        }

        /// <summary>
        ///     Adds orderLineItem json response to dB
        /// </summary>
        /// <param name="sDataOrderLineItem"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private async Task<OrderLineItem> AddOrderLineItemJsonToDbAsync(JsonObject sDataOrderLineItem, string orderId)
        {
            OrderLineItem orderLineItemObj = null;
            try
            {
                orderLineItemObj = new OrderLineItem();

                orderLineItemObj.OrderId = orderId;
                orderLineItemObj.OrderLineItemId = sDataOrderLineItem.GetNamedString("$key");

                orderLineItemObj = await ExtractOrderLineItemFromJsonAsync(sDataOrderLineItem, orderLineItemObj);
                await _sageSalesDB.InsertAsync(orderLineItemObj);
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
            return orderLineItemObj;
        }

        /// <summary>
        ///     Updates orerLineItem json response to dB
        /// </summary>
        /// <param name="sDataOrderLineItem"></param>
        /// <param name="orderLineItemDbObj"></param>
        /// <returns></returns>
        private async Task<OrderLineItem> UpdateOrderLineItemJsonToDbAsync(JsonObject sDataOrderLineItem,
            OrderLineItem orderLineItemDbObj)
        {
            try
            {
                orderLineItemDbObj = await ExtractOrderLineItemFromJsonAsync(sDataOrderLineItem, orderLineItemDbObj);
                await _sageSalesDB.UpdateAsync(orderLineItemDbObj);
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
            return orderLineItemDbObj;
        }

        /// <summary>
        ///     Extracts orderLineItem json response
        /// </summary>
        /// <param name="sDataOrderLineItem"></param>
        /// <param name="orderLineItem"></param>
        /// <returns></returns>
        private async Task<OrderLineItem> ExtractOrderLineItemFromJsonAsync(JsonObject sDataOrderLineItem,
            OrderLineItem orderLineItem)
        {
            try
            {
                IJsonValue value;

                if (sDataOrderLineItem.TryGetValue("Quantity", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        orderLineItem.Quantity = Convert.ToInt32(sDataOrderLineItem.GetNamedNumber("Quantity"));
                    }
                }

                if (sDataOrderLineItem.TryGetValue("Price", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        orderLineItem.Price = Convert.ToDecimal(sDataOrderLineItem.GetNamedNumber("Price"));
                    }
                }

                //yet to implement product.......
                if (sDataOrderLineItem.TryGetValue("InventoryItem", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonObject sDataProduct = sDataOrderLineItem.GetNamedObject("InventoryItem");

                        Product product = await _productRepository.AddOrUpdateProductJsonToDbAsync(sDataProduct);

                        orderLineItem.ProductId = product.ProductId;
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
            return orderLineItem;
        }

        /// <summary>
        ///     Deletes orderLineItems from dB which exists in dB but not in updated json response
        /// </summary>
        /// <param name="sDataOrderLineItemArray"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private async Task DeleteOrderLineItemsFromDbAsync(JsonArray sDataOrderLineItemArray, string orderId)
        {
            IJsonValue value;
            List<OrderLineItem> orderLineItemIdJsonList;
            List<OrderLineItem> orderLineItemIdDbList;
            List<OrderLineItem> orderLineItemRemoveList;
            bool idExists = false;

            try
            {
                // Retrieve list of addressId from Json
                orderLineItemIdJsonList = new List<OrderLineItem>();
                foreach (IJsonValue orderLineItem in sDataOrderLineItemArray)
                {
                    JsonObject sDataOrderLineItem = orderLineItem.GetObject();
                    var orderLineItemJsonObj = new OrderLineItem();
                    if (sDataOrderLineItem.TryGetValue("$key", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            orderLineItemJsonObj.OrderLineItemId = sDataOrderLineItem.GetNamedString("$key");
                        }
                    }
                    orderLineItemIdJsonList.Add(orderLineItemJsonObj);
                }

                //Retrieve list of orderLineItemId from dB
                orderLineItemIdDbList = new List<OrderLineItem>();
                orderLineItemRemoveList = new List<OrderLineItem>();
                orderLineItemIdDbList =
                    await _sageSalesDB.QueryAsync<OrderLineItem>("SELECT * FROM OrderLineItem where OrderId=?", orderId);

                // Requires enhancement
                for (int i = 0; i < orderLineItemIdDbList.Count; i++)
                {
                    idExists = false;
                    for (int j = 0; j < orderLineItemIdJsonList.Count; j++)
                    {
                        if (orderLineItemIdDbList[i].OrderLineItemId.Contains(DataAccessUtils.Pending))
                        {
                            idExists = true;
                            break;
                        }
                        if (orderLineItemIdDbList[i].OrderLineItemId == orderLineItemIdJsonList[j].OrderLineItemId)
                        {
                            idExists = true;
                            break;
                        }
                    }
                    if (!idExists)
                        orderLineItemRemoveList.Add(orderLineItemIdDbList[i]);
                }

                //if (orderLineItemIdDbList != null)
                //{
                //    var orderLineItemRemoveList = orderLineItemIdDbList.Except(orderLineItemIdJsonList, new OrderLineItemIdComparer()).ToList();
                if (orderLineItemRemoveList.Count() > 0)
                {
                    foreach (OrderLineItem orderLineItemRemove in orderLineItemRemoveList)
                    {
                        await _sageSalesDB.DeleteAsync(orderLineItemRemove);
                    }
                }
                //}
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

        #endregion
    }
}