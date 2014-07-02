using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public class OrderLineItemService : IOrderLineItemService
    {
        private readonly IServiceAgent _serviceAgent;
        private readonly IOrderRepository _orderRepository;
        private Dictionary<string, string> parameters = null;
        private string _log = string.Empty;
        public OrderLineItemService(IServiceAgent serviceAgent, IOrderRepository orderRepository)
        {
            _serviceAgent = serviceAgent;            
            _orderRepository = orderRepository;
        }

        #region public methods

        /// <summary>
        /// Sync all OrderLineItems for Order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task SyncOrderLineItems(string orderId)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                parameters.Add("Include", "ShippingAddress,Details");

                string orderEntityId = Constants.OrderEntity + "('" + orderId + "')";
                HttpResponseMessage orderLineItemResponse = null;
                orderLineItemResponse = await _serviceAgent.BuildAndSendRequest(orderEntityId, null, null, Constants.AccessToken, parameters);
                if (orderLineItemResponse != null && orderLineItemResponse.IsSuccessStatusCode)
                {
                    var sDataQuoteLineItem = await _serviceAgent.ConvertTosDataObject(orderLineItemResponse);
                    //Saving or updating Order, OrderLineItem, Address table
                    await _orderRepository.SaveOrderAsync(sDataQuoteLineItem);
                }
            }
            catch (SQLite.SQLiteException ex)
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
