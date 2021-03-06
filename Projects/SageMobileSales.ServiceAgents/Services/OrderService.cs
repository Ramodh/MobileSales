﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.JsonHelpers;
using SQLite;
using System.Net;

namespace SageMobileSales.ServiceAgents.Services
{
    public class OrderService : IOrderService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IOrderRepository _orderRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public OrderService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent,
            IQuoteRepository quoteRepository,
            ILocalSyncDigestRepository localSyncDigestRepository, IOrderRepository orderRepository,
            ISalesRepRepository salesRepRepository, IEventAggregator eventAggregator)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _orderRepository = orderRepository;
            _salesRepRepository = salesRepRepository;
            _quoteRepository = quoteRepository;
            _eventAggregator = eventAggregator;
        }

        #region public methods

        /// <summary>
        ///     Start syncing orders data
        /// </summary>
        /// <returns></returns>
        public async Task StartOrdersSyncProcess()
        {
            Constants.IsSyncAvailable =
                await _localSyncDigestService.SyncLocalDigest(Constants.OrderEntity, Constants.syncDigestQueryEntity);
            if (Constants.IsSyncAvailable)
            {
                await _localSyncDigestService.SyncLocalSource(Constants.OrderEntity, Constants.syncSourceQueryEntity);
                await SyncOrders();
            }
            //else
            //{
            //    _eventAggregator.GetEvent<OrderDataChangedEvent>().Publish(true);
            //}
        }

        //Need clarification
        public async Task<Orders> PostOrder(Quote quote)
        {
            Orders order = null;
            try
            {
                //string queryEntity = "$service/ConvertToOrder";
                object obj = null;

                obj = ConvertQuoteOrderToJsonFormattedObject(quote);

                HttpResponseMessage quoteResponse = null;

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPostObjectRequest(Constants.TenantId, Constants.QuoteToOrder, null,
                            Constants.AccessToken, null, obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    //TO DO 
                    // Need to confirm as what needs to be done here after posting order(with the response).
                    quote.QuoteStatus = "IsOrder";
                    await _quoteRepository.UpdateQuoteToDbAsync(quote);

                    //TO DO save order response
                    order = await _orderRepository.SaveOrderAsync(sDataQuote);
                }
                //Not required
                //if (quoteResponse==null || quoteResponse.StatusCode == HttpStatusCode.InternalServerError)
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
            catch (HttpRequestException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (TaskCanceledException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            return order;
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Makes call to BuildAndSendRequest method to make service call to get orders data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent orders batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of orders data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncOrders()
        {
            try
            {
                LocalSyncDigest digest =
                    await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.OrderEntity);
                parameters = new Dictionary<string, string>();
                if (digest != null)
                {
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", digest.LastRecordId);
                }
                else
                {
                    digest = new LocalSyncDigest();
                    digest.SDataEntity = Constants.OrderEntity;
                    digest.localTick = 0;
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", null);
                }
                ErrorLog("Order local tick : " + digest.localTick);
                //string salesRepId = await _salesRepRepository.GetSalesRepId();
                //if (!string.IsNullOrEmpty(salesRepId))
                //{
                //http://172.29.59.122:8080/sdata/api/msales/1.0/f200ac19-1be6-48c5-b604-2d322020f48e/Orders/
                //$SyncSource('B181349C-FFEC-42FD-9A20-B83A5C07F7A6-8e144a26-f89a-4a7f-9265-8a9453a27222')?Count=50&LocalTick=0 
                //salesRepId = "SalesRep.id eq " + "'" + salesRepId + "'";
                parameters.Add("Count", "100");
                parameters.Add("include", "Details");
                //parameters.Add("where", salesRepId);
                //parameters.Add("include", "Details,Details/InventoryItem&select=*,Details/Price,Details/Quantity,Details/InventoryItem/Name,Details/InventoryItem/Sku");
                HttpResponseMessage ordersResponse = null;

                Constants.syncQueryEntity = Constants.syncSourceQueryEntity + "('" + Constants.TrackingId + "')";

                ordersResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.OrderEntity,
                            Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (ordersResponse != null && ordersResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataOrders = await _serviceAgent.ConvertTosDataObject(ordersResponse);
                    if (Convert.ToInt32(sDataOrders.GetNamedNumber("$totalResults")) >
                        DataAccessUtils.OrdersTotalCount)
                        DataAccessUtils.OrdersTotalCount =
                            Convert.ToInt32(sDataOrders.GetNamedNumber("$totalResults"));
                    if (DataAccessUtils.OrdersTotalCount == 0)
                    {
                        _eventAggregator.GetEvent<OrderDataChangedEvent>().Publish(true);
                    }
                    int _totalCount = Convert.ToInt32(sDataOrders.GetNamedNumber("$totalResults"));
                    ErrorLog("Order total count : " + _totalCount);
                    JsonArray ordersObject = sDataOrders.GetNamedArray("$resources");
                    int _returnedCount = ordersObject.Count;
                    ErrorLog("Order returned count : " + _returnedCount);
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsOrdersSyncCompleted))
                    {
                        JsonObject lastOrderObject = ordersObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastOrderObject.GetNamedString("$key");
                        int _syncEndpointTick = Convert.ToInt32(lastOrderObject.GetNamedNumber("SyncTick"));
                        ErrorLog("Order sync tick : " + _syncEndpointTick);
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves Orders data into LocalDB
                        await _orderRepository.SaveOrdersAsync(sDataOrders, digest);
                        // Looping this method again to make request for next batch of records(Orders).
                        await SyncOrders();
                    }
                    else
                    {
                        DataAccessUtils.OrdersTotalCount = 0;
                        DataAccessUtils.OrdersReturnedCount = 0;
                        DataAccessUtils.IsOrdersSyncCompleted = false;
                    }
                }
                //}
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (TaskCanceledException ex)
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


        private OrderJson ConvertQuoteOrderToJsonFormattedObject(Quote quote)
        {
            var orderjson = new OrderJson();

            orderjson.QuoteId = quote.QuoteId;
            orderjson.AmountPaid = quote.ToOrderAmountPaid == null ? "0" : quote.ToOrderAmountPaid;
            orderjson.AuthorizationCode = quote.ToOrderAuthorizationCode == null ? "" : quote.ToOrderAuthorizationCode;
            orderjson.CreditCardLast4 = quote.ToOrderCreditCardLast4 == null ? "" : quote.ToOrderCreditCardLast4;
            orderjson.ExpirationMonth = quote.ToOrderCreditCardExpMonth == null ? "" : quote.ToOrderCreditCardExpMonth;
            orderjson.ExpirationYear = quote.ToOrderCreditCardExpYear == null ? "" : quote.ToOrderCreditCardExpYear;
            // Default value for Payment via Account
            //orderjson.InitialOrderStatus = quote.ToOrderStatus == null ? "2" : quote.ToOrderStatus;
            orderjson.PaymentMethod = quote.ToOrderPaymentType == null ? "Account" : quote.ToOrderPaymentType;
            orderjson.PaymentReference = quote.ToOrderReference == null ? "" : quote.ToOrderReference;
            // Need confirmation for status only for submitted
            //orderjson.Status = quote.QuoteStatus;
            //orderjson.Status = "Submitted";
            // Need confirmation here too
            orderjson.TaxAmount = quote.Tax == null ? "0" : quote.Tax.ToString();

            //long milliseconds = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            //orderjson.TransactionDate = "/Date(" + milliseconds + ")/";

            //order.TransactionDate ="/Date(1392833833000-0600)/";

            return orderjson;
        }

        /// <summary>
        /// Error log
        /// </summary>
        /// <param name="message"></param>
        private void ErrorLog(string message)
        {
            AppEventSource.Log.Info(message);
        }

        #endregion
    }
}