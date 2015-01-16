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
    public class ProductDetailsService : IProductDetailsService
    {
        private readonly IProductRepository _productRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public ProductDetailsService(IServiceAgent serviceAgent, IProductRepository productRepository)
        {
            _serviceAgent = serviceAgent;
            _productRepository = productRepository;
        }

        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get ProductDetails data.
        ///     Once we get the response converts it into JsonObject.
        ///     And then calls UpdatProductsAsync to extract JsonObject data & save it into localDB
        /// </summary>
        /// <returns></returns>
        public async Task SyncProductDetails(string productId)
        {
            try
            {
                var syncQueryEntity = Constants.ItemsEntity + "('" + productId + "')";
                parameters = new Dictionary<string, string>();

                parameters.Add("include", "Images,RelatedItems");
                parameters.Add("select", "*");

                HttpResponseMessage productDetailsResponse = null;
                productDetailsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, null, syncQueryEntity,
                            Constants.AssociatedBlobs,
                            Constants.AccessToken, parameters);
                if (productDetailsResponse != null && productDetailsResponse.IsSuccessStatusCode)
                {
                    var sDataProductDetails = await _serviceAgent.ConvertTosDataObject(productDetailsResponse);

                    // Saves ProductDetails data into LocalDB
                    await _productRepository.UpdatProductsAsync(sDataProductDetails);
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
        }
    }
}