using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class OrderLineItemService : IOrderLineItemService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters = null;

        public OrderLineItemService(IServiceAgent serviceAgent, IOrderRepository orderRepository)
        {
            _serviceAgent = serviceAgent;
            _orderRepository = orderRepository;
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
                parameters.Add("include", "Details,ShippingAddress");

                string orderEntityId = Constants.OrderEntity + "('" + orderId + "')";
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