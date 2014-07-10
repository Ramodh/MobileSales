﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.ServiceAgents.Services
{
    public class SalesRepService : ISalesRepService
    {
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IServiceAgent _serviceAgent;

        public SalesRepService(IServiceAgent serviceAgent, ISalesRepRepository salesRepRepository)
        {
            _serviceAgent = serviceAgent;
            _salesRepRepository = salesRepRepository;
        }

        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get loginuser details(SalesRep) data.
        ///     Once we get the response converts it into JsonObject.
        /// </summary>
        /// <returns></returns>
        public async Task SyncSalesRep()
        {
            HttpResponseMessage salesRepResponse =
                await
                    _serviceAgent.BuildAndSendRequest(null, Constants.CurrentUserEnitty, null, null, Constants.AccessToken,
                        null);
            if (salesRepResponse.IsSuccessStatusCode)
            {
                var sDataSalesRep = await _serviceAgent.ConvertTosDataObject(salesRepResponse);

                await SaveSalesRepDtls(sDataSalesRep);
            }
        }

        /// <summary>
        ///     makes call to SaveSalesRepDtlsAsync method where it will save loginuser details and
        ///     makes call to BuildAndSendRequest method to make service call to get loginuser(SalesRep) settings  data.
        ///     Once we got loginuser settings data(SalesRepSettings) we will update this data again in LocalDB.
        /// </summary>
        /// <param name="sDataSalesRep"></param>
        /// <returns></returns>
        private async Task SaveSalesRepDtls(JsonObject sDataSalesRep)
        {
            //var sageSalesDB = await DataContext.InitializeDatabase();

            var userId = await _salesRepRepository.SaveSalesRepDtlsAsync(sDataSalesRep);
            Constants.TrackingId = Constants.GetDeviceId() + userId;

            // Adding TrackingId to Applicationdata Container as we are using this in every Servicerequest.
            // And this will be usefull when we are doing partial sync for particular Service.
            ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
            settingsLocal.Containers["SageSalesContainer"].Values["TrackingId"] = Constants.TrackingId;

            /*
             *  peagausus
             * if (userId != null)
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("userId", userId);
                HttpResponseMessage salesRepSettingsResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(null, Constants.GetSalesSettingsEntity,
                            Constants.GetSalesSettingsQueryEntity, null, Constants.AccessToken, parameters);
                if (salesRepSettingsResponse != null && salesRepSettingsResponse.IsSuccessStatusCode)
                {
                    JsonObject sDatasalesRepSettings =
                        await _serviceAgent.ConvertTosDataObject(salesRepSettingsResponse);
                    await _salesRepRepository.UpdateSalesRepDtlsAsync(sDatasalesRepSettings);
                }
            }
             * 
             * **/
        }
    }
}