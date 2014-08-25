using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public class OrderLineItemService : IOrderLineItemService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters = null;

        public OrderLineItemService(IServiceAgent serviceAgent, IOrderRepository orderRepository, IOrderLineItemRepository orderLineItemRepository)
        {
            _serviceAgent = serviceAgent;
            _orderRepository = orderRepository;
            _orderLineItemRepository = orderLineItemRepository;
        }

        #region public methods

        /// <summary>
        ///     Sync all OrderLineItems for Order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task SyncOrderLineItems(string orderId)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                //parameters.Add("include", "Details");                               

                string orderEntityId = Constants.OrderDetailEntity + "('" + orderId + "')";

                List<OrderLineItem> orderLineItemList = await _orderLineItemRepository.GetOrderLineItemForOrder(orderId);
                if (orderLineItemList != null && orderLineItemList.Count > 0)
                {
                    parameters.Add("include", "Details,ShippingAddress");
                }
                else
                {
                    parameters.Add("include", "Details,ShippingAddress,Details/InventoryItem,Details/InventoryItem/Images");
                }

                HttpResponseMessage orderLineItemResponse = null;
                orderLineItemResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, orderEntityId, null, null, Constants.AccessToken, parameters);
                if (orderLineItemResponse != null && orderLineItemResponse.IsSuccessStatusCode)
                {
                    var sDataQuoteLineItem = await _serviceAgent.ConvertTosDataObject(orderLineItemResponse);
                    //Saving or updating Order, OrderLineItem, Address table
                    await _orderRepository.SaveOrderAsync(sDataQuoteLineItem);
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

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion

        #region private methods

        #endregion
    }
}