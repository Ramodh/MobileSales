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
    public class SalesHistoryService : ISalesHistoryService
    {
        private readonly ISalesHistoryRepository _salesHistoryRepository;
        private readonly IServiceAgent _serviceAgent;
        private Dictionary<string, string> parameters = null;


        public SalesHistoryService(IServiceAgent serviceAgent, ISalesHistoryRepository salesHistoryRepository)
        {
            _serviceAgent = serviceAgent;
            _salesHistoryRepository = salesHistoryRepository;
        }

        #region public methods
        public async Task SyncSalesHistory(string customerId, string itemId)
        {
            parameters = new Dictionary<string, string>();
            parameters.Add("Count", "50");
            parameters.Add("startindex", "1");

            string customerQuery = "CustomerId eq guid'" + customerId + "' and ";
            string itemQuery = "ItemId eq guid'" + itemId + "'";
            parameters.Add("where", customerQuery + itemQuery);

            //string customerEntityId = Constants.CustomerDetailEntity + "('" + customerId + "')";
            HttpResponseMessage salesHistoryResponse = null;
            salesHistoryResponse =
                await _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.CustomerSalesHistory, null, null, Constants.AccessToken, parameters);
            if (salesHistoryResponse != null && salesHistoryResponse.IsSuccessStatusCode)
            {
                var sDataSalesHistory = await _serviceAgent.ConvertTosDataObject(salesHistoryResponse);
                await _salesHistoryRepository.SaveSalesHistoryAsync(sDataSalesHistory);
            }

        }
        #endregion

        #region private methods
        #endregion
    }
}
