using System;
using System.Net.Http;
using System.Threading.Tasks;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;
using System.Collections.Generic;

namespace SageMobileSales.ServiceAgents.Services
{
    public class TenantService : ITenantService
    {
        private string _log = string.Empty;
        private readonly IServiceAgent _serviceAgent;
        private readonly ITenantRepository _tenantRepository;
        private readonly ISalesRepRepository _salesRepRepository;
        Dictionary<string, string> parameters = null;

        public TenantService(IServiceAgent serviceAgent, ITenantRepository tenantRepository, ISalesRepRepository salesRepRepository)
        {
            _serviceAgent = serviceAgent;
            _tenantRepository = tenantRepository;
            _salesRepRepository = salesRepRepository;
        }

        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get loginuser details(SalesRep) data.
        ///     Once we get the response converts it into JsonObject.
        /// </summary>
        /// <returns></returns>
        public async Task SyncTenant()
        {
            try
            {

                parameters = new Dictionary<string, string>();
                parameters.Add("include", "CompanySettings,SalesTeamMember");

                HttpResponseMessage tenantResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.AppSalesUser, null, null, Constants.AccessToken,
                            parameters);
                if (tenantResponse.IsSuccessStatusCode)
                {
                    var sDataTenantSalesTeamMemberDtls = await _serviceAgent.ConvertTosDataObject(tenantResponse);
                    //Changed by ramodh for pegausus
                    await _tenantRepository.UpdateTenantAsync(sDataTenantSalesTeamMemberDtls, Constants.TenantId);
                    await _salesRepRepository.UpdateSalesRepDtlsAsync(sDataTenantSalesTeamMemberDtls);
                }
            }
            catch (HttpRequestException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (SQLiteException ex)
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