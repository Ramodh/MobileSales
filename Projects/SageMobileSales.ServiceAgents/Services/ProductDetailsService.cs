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
        private Dictionary<string, string> parameters = null;

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
                string syncQueryEntity = Constants.ItemsEntity + "('" + productId + "')";
                parameters = new Dictionary<string, string>();

                parameters.Add("include", "AssociatedBlobs,RelatedItems");
                parameters.Add("select", "*");

                HttpResponseMessage productDetailsResponse = null;
                productDetailsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(null, null, syncQueryEntity, Constants.AssociatedBlobs,
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