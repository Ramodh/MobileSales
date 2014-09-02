using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class TenantService : ITenantService
    {
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IServiceAgent _serviceAgent;
        private readonly ITenantRepository _tenantRepository;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public TenantService(IServiceAgent serviceAgent, ITenantRepository tenantRepository,
            ISalesRepRepository salesRepRepository)
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
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.AppSalesUser, null, null,
                            Constants.AccessToken,
                            parameters);
                if (tenantResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataTenantSalesTeamMemberDtls = await _serviceAgent.ConvertTosDataObject(tenantResponse);
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