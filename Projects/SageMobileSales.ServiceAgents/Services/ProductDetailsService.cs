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
    public class ProductDetailsService: IProductDetailsService
    {

        private readonly IServiceAgent _serviceAgent;
        private readonly IProductRepository _productRepository;
        private Dictionary<string, string> parameters = null;
        private string _log = string.Empty;
        public ProductDetailsService(IServiceAgent serviceAgent, IProductRepository productRepository)
        {

            _serviceAgent = serviceAgent;           
            _productRepository = productRepository;
        }

    
        /// <summary>
        /// makes call to BuildAndSendRequest method to make service call to get ProductDetails data.
        /// Once we get the response converts it into JsonObject.
        /// And then calls UpdatProductsAsync to extract JsonObject data & save it into localDB
        /// </summary>
        /// <returns></returns>
        public async Task SyncProductDetails(string productId)
        {
            try
            {
                var syncQueryEntity = Constants.ItemsEntity + "('" + productId + "')";
                parameters = new Dictionary<string, string>();

                parameters.Add("include", "AssociatedBlobs,RelatedItems");
                parameters.Add("select", "*");

                HttpResponseMessage productDetailsResponse = null;
                productDetailsResponse = await _serviceAgent.BuildAndSendRequest(null,syncQueryEntity, Constants.AssociatedBlobs, Constants.AccessToken, parameters);
                if (productDetailsResponse!=null && productDetailsResponse.IsSuccessStatusCode)
                {
                    var sDataProductDetails = await _serviceAgent.ConvertTosDataObject(productDetailsResponse);
                                     
                        // Saves ProductDetails data into LocalDB
                    await _productRepository.UpdatProductsAsync(sDataProductDetails);
                                              
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
