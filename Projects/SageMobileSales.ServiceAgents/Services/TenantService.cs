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
    public class TenantService : ITenantService
    {

        private IServiceAgent _serviceAgent;
        private ITenantRepository _tenantRepository;
        private string _log = string.Empty;
        public TenantService(IServiceAgent serviceAgent, ITenantRepository tenantRepository)
        {
            _serviceAgent = serviceAgent;
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// makes call to BuildAndSendRequest method to make service call to get loginuser details(SalesRep) data.
        /// Once we get the response converts it into JsonObject.
        /// </summary>
        /// <returns></returns>
        public async Task SyncTenant()
        {
            try
            {
                HttpResponseMessage tenantResponse = await _serviceAgent.BuildAndSendRequest(Constants.TenantEntity, null, null, Constants.AccessToken, null);
                if (tenantResponse.IsSuccessStatusCode)
                {
                    var sDataTenantDtls = await _serviceAgent.ConvertTosDataObject(tenantResponse);
                    await _tenantRepository.SaveTenantDtlsAsync(sDataTenantDtls);
                }
            }
                catch(HttpRequestException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (SQLite.SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }        
            catch (Exception e)
            {
                _log = AppEventSource.Log.WriteLine(e);
                AppEventSource.Log.Error(_log);
            }

        }
    }
}
