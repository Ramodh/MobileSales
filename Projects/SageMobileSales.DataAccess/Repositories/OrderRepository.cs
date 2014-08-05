using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private readonly ISalesRepRepository _salesRepRepository;
        private IDatabase _database;
        private string _log = string.Empty;

        public OrderRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository,
            ICustomerRepository customerRepository, ISalesRepRepository salesRepRepository,
            IAddressRepository addressRepository, IOrderLineItemRepository orderLineItemRepository,
            IQuoteRepository quoteRepository, IEventAggregator eventAggregator)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
            _orderLineItemRepository = orderLineItemRepository;
            _salesRepRepository = salesRepRepository;
            _quoteRepository = quoteRepository;
            _eventAggregator = eventAggregator;
        }

        #region public methods

        /// <summary>
        ///     Extract json response(Order) and saves into local dB
        /// </summary>
        /// <param name="sDataOrders"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveOrdersAsync(JsonObject sDataOrders, LocalSyncDigest localSyncDigest)
        {
            JsonArray sDataOrdersArray = sDataOrders.GetNamedArray("$resources");
            DataAccessUtils.OrdersReturnedCount += sDataOrdersArray.Count;

            for (int order = 0; order < sDataOrdersArray.Count; order++)
            {
                JsonObject sDataOrder = sDataOrdersArray[order].GetObject();

                //Changed for pegasus as it returns only order list.
                //await
                //    _orderLineItemRepository.SaveOrderLineItemsAsync(sDataOrder,
                //        (await SaveOrderDetailsAsync(sDataOrder)).OrderId);

                await SaveOrderDetailsAsync(sDataOrder);

                if (localSyncDigest != null)
                {
                    if ((Convert.ToInt32(sDataOrder.GetNamedNumber("SyncTick")) > localSyncDigest.localTick))
                        localSyncDigest.localTick = Convert.ToInt32(sDataOrder.GetNamedNumber("SyncTick"));
                }

                if (order == (sDataOrdersArray.Count - 1) && localSyncDigest != null)
                {
                    // Update UI / Publish
                    _eventAggregator.GetEvent<OrderDataChangedEvent>().Publish(true);

                    if (DataAccessUtils.OrdersTotalCount == DataAccessUtils.OrdersReturnedCount)
                    {
                        if (localSyncDigest == null)
                            localSyncDigest = new LocalSyncDigest();
                        localSyncDigest.localTick++;
                        localSyncDigest.LastRecordId = null;
                        localSyncDigest.LastSyncTime = DateTime.Now;
                        DataAccessUtils.IsQuotesSyncCompleted = true;
                    }
                    await _localSyncDigestRepository.UpdateLocalSyncDigestDtlsAsync(localSyncDigest);
                }
            }
        }

        /// <summary>
        ///     Extracts data from response and updates orders, addresses and orderLineItems
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <returns></returns>
        public async Task<Orders> SaveOrderAsync(JsonObject sDataOrder)
        {
            IJsonValue value;

            Orders order = await SaveOrderDetailsAsync(sDataOrder);

            //if (sDataOrder.TryGetValue("ShippingAddress", out value))
            //{
            //    if (value.ValueType.ToString() != DataAccessUtils.Null)
            //    {
            //        JsonObject sDataShippingAdress = sDataOrder.GetNamedObject("ShippingAddress");
            //        if (!string.IsNullOrEmpty(order.CustomerId))
            //        {
            //            Address address =
            //                await
            //                    _addressRepository.AddOrUpdateAddressJsonToDbAsync(sDataShippingAdress, order.CustomerId);
            //            if (address != null)
            //            {
            //                order.AddressId = address.AddressId;
            //            }
            //        }
            //    }
            //}

            if (sDataOrder.TryGetValue("Customer", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    JsonObject sDataCustomer = sDataOrder.GetNamedObject("Customer");
                    Customer customer = await _customerRepository.AddOrUpdateCustomerJsonToDbAsync(sDataCustomer);
                    if (customer != null)
                    {
                        await _addressRepository.SaveAddressesAsync(sDataCustomer, customer.CustomerId);
                    }
                }
            }

            await _orderLineItemRepository.SaveOrderLineItemsAsync(sDataOrder, order.OrderId);
            return order;
        }

        /// <summary>
        ///     Get order details along with salesRepName
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<OrderDetails> GetOrderDetailsAsync(string orderId)
        {
            List<OrderDetails> order = null;
            try
            {
                //SELECT distinct Orders.OrderId, Orders.CreatedOn, Orders.OrderNumber, Orders.CustomerId, Orders.AddressId, Orders.TenantId, Orders.Amount, Orders.Tax, Orders.ShippingAndHandling, Orders.DiscountPercent, Orders.OrderStatus,Orders.OrderDescription, SalesRep.RepName FROM Orders  INNER JOIN SalesRep ON salesRep.RepId = Orders.RepId and Orders.OrderId=?
                order =
                    await
                        _sageSalesDB.QueryAsync<OrderDetails>(
                            "SELECT distinct Orders.OrderId, Orders.CreatedOn, Orders.OrderNumber, Orders.CustomerId, Orders.AddressId, Orders.TenantId, Orders.Amount, Orders.Tax, Orders.ShippingAndHandling, Orders.DiscountPercent, Orders.OrderStatus,Orders.OrderDescription, SalesRep.RepName FROM Orders  INNER JOIN SalesRep ON Orders.OrderId=?",
                            orderId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return order.FirstOrDefault();
        }

        /// <summary>
        ///     Get order details based on CustomerId
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<List<OrderDetails>> GetOrdersForCustomerAsync(string customerId)
        {
            List<OrderDetails> orderList = null;
            try
            {
                orderList =
                    await
                        _sageSalesDB.QueryAsync<OrderDetails>(
                            "SELECT distinct customer.customerName, orders.CustomerId, orders.AddressId, orders.TenantId, orders.OrderId, orders.CreatedOn, orders.amount, orders.DiscountPercent, orders.ShippingAndHandling, orders.Tax, orders.OrderStatus,orders.OrderDescription, SalesRep.RepName FROM customer, orders left Join SalesRep On SalesRep.RepId=Orders.RepId where Orders.OrderStatus!='IsOrder'  And Orders.OrderStatus!='Temporary' and customer.customerId=orders.customerId and orders.customerId=? order by orders.createdOn desc",
                            customerId);
                //"SELECT distinct customer.customerName, quote.CustomerId, quote.QuoteId, quote.CreatedOn, quote.amount, quote.quoteStatus,quote.QuoteDescription, SalesRep.RepName FROM customer, quote left Join SalesRep On SalesRep.RepId=Quote.RepId where Quote.QuoteStatus!='IsOrder'  And Quote.QuoteStatus!='Temporary' and customer.customerId=quote.customerId and quote.customerId=? order by quote.createdOn desc", customerId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return orderList;
        }

        /// <summary>
        ///     Get quotes list along with customerName, salesRepName
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <returns></returns>
        public async Task<List<OrderDetails>> GetOrdersListAsync(string salesRepId)
        {
            List<OrderDetails> ordersList = null;
            try
            {
                //"SELECT distinct customer.customerName, customer.CustomerId, orders.OrderId, orders.OrderStatus, orders.CreatedOn,orders.UpdatedOn,orders.AddressId, orders.Amount, orders.DiscountPercent, orders.Tax, orders.ShippingAndHandling, orders.ExternalReferenceNumber,orders.TenantId,orders.OrderDescription, SalesRep.RepName FROM orders INNER JOIN customer ON customer.customerID = orders.customerId Inner Join SalesRep On orders.RepId=?"
                ordersList =
                    await
                        _sageSalesDB.QueryAsync<OrderDetails>(
                            "SELECT distinct customer.customerName, customer.CustomerId, orders.OrderId, orders.OrderStatus, orders.CreatedOn,orders.SubmittedDate,orders.AddressId, orders.Amount, orders.DiscountPercent, orders.Tax, orders.ShippingAndHandling, orders.TenantId,orders.OrderDescription,orders.OrderNumber, SalesRep.RepName FROM orders INNER JOIN customer ON customer.customerID = orders.customerId Inner Join SalesRep");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return ordersList;
        }

        /// <summary>
        ///     Adds or updates order json response to dB
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <returns></returns>
        public async Task<Orders> AddOrUpdateOrderJsonToDbAsync(JsonObject sDataOrder)
        {
            try
            {
                IJsonValue value;
                if (sDataOrder.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Orders> orderList;
                        orderList =
                            await
                                _sageSalesDB.QueryAsync<Orders>("SELECT * FROM Orders where OrderId=?",
                                    sDataOrder.GetNamedString("Id"));

                        if (orderList.FirstOrDefault() != null)
                        {
                            return await UpdateOrderJsonToDbAsync(sDataOrder, orderList.FirstOrDefault());
                        }
                        return await AddOrderJsonToDbAsync(sDataOrder);
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
        ///     Extracts order from Json and updates the same
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <returns></returns>
        private async Task<Orders> SaveOrderDetailsAsync(JsonObject sDataOrder)
        {
            return await AddOrUpdateOrderJsonToDbAsync(sDataOrder);
        }

        /// <summary>
        ///     Add order json response to dB
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <returns></returns>
        private async Task<Orders> AddOrderJsonToDbAsync(JsonObject sDataOrder)
        {
            var orderObj = new Orders();
            try
            {
                orderObj.OrderId = sDataOrder.GetNamedString("Id");
                orderObj = await ExtractOrderFromJsonAsync(sDataOrder, orderObj);

                await _sageSalesDB.InsertAsync(orderObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return orderObj;
        }

        /// <summary>
        ///     Updates order json response to dB
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <param name="orderDbObj"></param>
        /// <returns></returns>
        private async Task<Orders> UpdateOrderJsonToDbAsync(JsonObject sDataOrder, Orders orderDbObj)
        {
            try
            {
                orderDbObj = await ExtractOrderFromJsonAsync(sDataOrder, orderDbObj);
                await _sageSalesDB.UpdateAsync(orderDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return orderDbObj;
        }

        /// <summary>
        ///     Extracts order json response
        /// </summary>
        /// <param name="sDataOrder"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private async Task<Orders> ExtractOrderFromJsonAsync(JsonObject sDataOrder, Orders order)
        {
            try
            {
                IJsonValue value;
                if (sDataOrder.TryGetValue("Description", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.OrderDescription = sDataOrder.GetNamedString("Description");
                    }
                }
                //if (sDataOrder.TryGetValue("TenantId", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        order.TenantId = sDataOrder.GetNamedString("TenantId");
                //    }
                //}
                if (sDataOrder.TryGetValue("CreatedOn", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.CreatedOn = ConvertJsonStringToDateTime(sDataOrder.GetNamedString("CreatedOn"));
                    }
                }
                //if (sDataOrder.TryGetValue("UpdatedOn", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        order.UpdatedOn = ConvertJsonStringToDateTime(sDataOrder.GetNamedString("UpdatedOn"));
                //    }
                //}
                if (sDataOrder.TryGetValue("Status", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.OrderStatus = sDataOrder.GetNamedString("Status");
                    }
                }
                if (sDataOrder.TryGetValue("SandH", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.ShippingAndHandling = Convert.ToDecimal(sDataOrder.GetNamedNumber("SandH"));
                    }
                }
                if (sDataOrder.TryGetValue("Tax", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.Tax = Convert.ToDecimal(sDataOrder.GetNamedNumber("Tax"));
                    }
                }
                if (sDataOrder.TryGetValue("OrderTotal", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.Amount = Convert.ToDecimal(sDataOrder.GetNamedNumber("OrderTotal"));
                    }
                }
                //if (sDataOrder.TryGetValue("ExternalReference", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        order.ExternalReferenceNumber = sDataOrder.GetNamedString("ExternalReference");
                //    }
                //}
                if (sDataOrder.TryGetValue("DiscountPercent", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.DiscountPercent = Convert.ToDecimal(sDataOrder.GetNamedNumber("DiscountPercent"));
                    }
                }
                if (sDataOrder.TryGetValue("OrderNumber", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        order.OrderNumber = Convert.ToInt32(sDataOrder.GetNamedNumber("OrderNumber"));
                    }
                }

                //if (sDataOrder.TryGetValue("Quote", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        JsonObject sDataQuote = sDataOrder.GetNamedObject("Quote");
                //        Quote quote = await _quoteRepository.AddOrUpdateQuoteJsonToDbAsync(sDataQuote);
                //        if (quote != null)
                //        {
                //            order.QuoteId = quote.QuoteId;
                //        }
                //    }
                //}

                if (sDataOrder.TryGetValue("CustomerId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        //JsonObject sDataCustomer = sDataOrder.GetNamedObject("Customer");                        
                        var customerDb = await _sageSalesDB.QueryAsync<Customer>("SELECT * FROM CUSTOMER WHERE CustomerId=?", sDataOrder.GetNamedString("CustomerId"));
                        //Customer customer = await _customerRepository.AddOrUpdateCustomerJsonToDbAsync(sDataCustomer);
                        //if (customer != null)
                        //{
                        //    order.CustomerId = customer.CustomerId;
                        //}
                        if (customerDb.FirstOrDefault() != null)
                        {
                            order.CustomerId = customerDb.FirstOrDefault().CustomerId;
                        }
                        else
                        {
                            Customer customer = new Customer();
                            customer.CustomerId = sDataOrder.GetNamedString("CustomerId");

                            order.CustomerId = customer.CustomerId;

                            await _sageSalesDB.InsertAsync(customer);
                        }
                    }
                }

                if (sDataOrder.TryGetValue("ShippingAddressId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        //JsonObject sDataShippingAdress = sDataOrder.GetNamedObject("ShippingAddress");
                        
                        if (!string.IsNullOrEmpty(order.CustomerId))
                        {
                            var addressDb = await _sageSalesDB.QueryAsync<Address>("SELECT * FROM ADDRESS WHERE AddressId=?", sDataOrder.GetNamedString("ShippingAddressId"));
                            //Address address =
                            //    await
                            //        _addressRepository.AddOrUpdateAddressJsonToDbAsync(sDataShippingAdress,
                            //            order.CustomerId);
                            //if (address != null)
                            //{
                            //    order.AddressId = address.AddressId;
                            //}
                            if (addressDb.FirstOrDefault() != null)
                            {
                                order.AddressId = addressDb.FirstOrDefault().AddressId;
                            }
                            else
                            {
                                Address address = new Address();
                                address.CustomerId = order.OrderId;
                                address.AddressId = sDataOrder.GetNamedString("ShippingAddressId");

                                order.AddressId = address.AddressId;

                                await _sageSalesDB.InsertAsync(address);
                            }
                        }
                    }
                }

                //if (sDataOrder.TryGetValue("SalesRep", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        JsonObject sDataSalesRep = sDataOrder.GetNamedObject("SalesRep");
                //        if (sDataSalesRep.GetNamedValue("Id").ValueType.ToString() != DataAccessUtils.Null)
                //        {
                //            SalesRep salesRep =
                //                await _salesRepRepository.AddOrUpdateSalesRepJsonToDbAsync(sDataSalesRep);
                //            order.RepId = sDataSalesRep.GetNamedString("Id");
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return order;
        }

        /// <summary>
        ///     Converts Json date string to respective Date and Time format
        /// </summary>
        /// <param name="jsonTime"></param>
        /// <returns></returns>
        private DateTime ConvertJsonStringToDateTime(string jsonTime)
        {
            if (!string.IsNullOrEmpty(jsonTime) && jsonTime.IndexOf("Date") > -1)
            {
                string milis = jsonTime.Substring(jsonTime.IndexOf("(") + 1);
                string sign = milis.IndexOf("+") > -1 ? "+" : "-";
                string hours = "";
                // Need to change based on GMT........ To be Confirmed
                if (milis.IndexOf(sign) > -1)
                {
                    hours = milis.Substring(milis.IndexOf(sign));
                    milis = milis.Substring(0, milis.IndexOf(sign));
                    hours = hours.Substring(0, hours.IndexOf(")"));
                    return
                        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
                            .AddHours(Convert.ToInt64(hours) / 100);
                }
                hours = "0";
                milis = milis.Substring(0, milis.IndexOf(")"));
                return
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
                        .AddHours(Convert.ToInt64(hours) / 100);
            }

            return DateTime.Now;
        }

        #endregion
    }
}