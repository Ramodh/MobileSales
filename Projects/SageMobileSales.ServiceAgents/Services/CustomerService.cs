using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters = null;

        public CustomerService(ILocalSyncDigestService localSyncDigestService, IServiceAgent serviceAgent,
            ILocalSyncDigestRepository localSyncDigestRepository, ICustomerRepository customerRepository,
            IEventAggregator eventAggregator)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _customerRepository = customerRepository;
            _eventAggregator = eventAggregator;
        }

        # region public methods

        /// <summary>
        ///     Start syncing customers data
        /// </summary>
        /// <returns></returns>
        public async Task StartCustomersSyncProcess()
        {
            Constants.IsSyncAvailable =
                await _localSyncDigestService.SyncLocalDigest(Constants.CustomerEntity, Constants.syncDigestQueryEntity);
            if (Constants.IsSyncAvailable)
            {
                await _localSyncDigestService.SyncLocalSource(Constants.CustomerEntity, Constants.syncSourceQueryEntity);
                await SyncCustomers();
            }
        }

        # endregion

        #region private methods

        /// <summary>
        ///     Makes call to BuildAndSendRequest method to make service call to get Customers data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent Customers batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of Customers data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncCustomers()
        {
            try
            {
                LocalSyncDigest digest =
                    await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.CustomerEntity);
                parameters = new Dictionary<string, string>();
                if (digest != null)
                {
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", digest.LastRecordId);
                }
                else
                {
                    digest = new LocalSyncDigest();
                    digest.SDataEntity = Constants.CustomerEntity;
                    digest.localTick = 0;
                    parameters.Add("LocalTick", digest.localTick.ToString());
                    parameters.Add("LastRecordId", null);
                }
                parameters.Add("Count", "100");
                parameters.Add("include", "Addresses");
                HttpResponseMessage customersResponse = null;
                customersResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.CustomerEntity, Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (customersResponse != null && customersResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataCustomers = await _serviceAgent.ConvertTosDataObject(customersResponse);
                    if (Convert.ToInt32(sDataCustomers.GetNamedNumber("$totalResults")) >
                        DataAccessUtils.CustomerTotalCount)
                        DataAccessUtils.CustomerTotalCount =
                            Convert.ToInt32(sDataCustomers.GetNamedNumber("$totalResults"));
                    if (DataAccessUtils.CustomerTotalCount == 0)
                    {
                        _eventAggregator.GetEvent<CustomerDataChangedEvent>().Publish(true);
                    }
                    int _totalCount = Convert.ToInt32(sDataCustomers.GetNamedNumber("$totalResults"));
                    JsonArray customersObject = sDataCustomers.GetNamedArray("$resources");
                    int _returnedCount = customersObject.Count;
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsCustomerSyncCompleted))
                    {
                        JsonObject lastCustomerObject = customersObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastCustomerObject.GetNamedString("$key");
                        int _syncEndpointTick = Convert.ToInt32(lastCustomerObject.GetNamedNumber("SyncTick"));
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves Customers data into LocalDB
                        await _customerRepository.SaveCustomersAsync(sDataCustomers, digest);
                        // Looping this method again to make request for next batch of records(Customers).
                        await SyncCustomers();
                    }
                    else
                    {
                        DataAccessUtils.CustomerTotalCount = 0;
                        DataAccessUtils.CustomerReturnedCount = 0;
                        DataAccessUtils.IsCustomerSyncCompleted = false;
                    }
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
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion
    }
}