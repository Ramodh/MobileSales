using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.ServiceAgents.Services
{
    public class FrequentlyPurchasedItemService : IFrequentlyPurchasedItemService
    {
        private readonly IFrequentlyPurchasedItemRepository _frequentlyPurchasedItemRepository;
        private readonly IServiceAgent _serviceAgent;
        private Dictionary<string, string> parameters;

        public FrequentlyPurchasedItemService(IFrequentlyPurchasedItemRepository frequentlyPurchasedItemRepository,
            IServiceAgent serviceAgent)
        {
            _frequentlyPurchasedItemRepository = frequentlyPurchasedItemRepository;
            _serviceAgent = serviceAgent;
        }

        #region public methods

        public async Task SyncFrequentlyPurchasedItems(string customerId)
        {
            parameters = new Dictionary<string, string>();
            customerId = "CustomerId eq guid" + "'" + customerId + "'";
            parameters.Add("where", customerId);

            //string frequentlyPurchasedItemByCustomerEntityId = Constants.FrequentlyPurchasedItem + "('" + customerId + "')";
            HttpResponseMessage frequentlyPurchasedItemResponse = null;

            frequentlyPurchasedItemResponse =
                await
                    _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.FrequentlyPurchasedItem, null, null,
                        Constants.AccessToken, parameters);

            if (frequentlyPurchasedItemResponse != null && frequentlyPurchasedItemResponse.IsSuccessStatusCode)
            {
                JsonObject sDataFrequentlyPurchasedItem =
                    await _serviceAgent.ConvertTosDataObject(frequentlyPurchasedItemResponse);
                await _frequentlyPurchasedItemRepository.SaveFrequentlyPurchasedItemsAsync(sDataFrequentlyPurchasedItem);
            }
        }

        #endregion

        #region private methods

        #endregion
    }
}