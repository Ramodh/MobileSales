using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.ServiceAgents.Services
{
    public class LocalSyncDigestService : ILocalSyncDigestService
    {
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly IServiceAgent _serviceAgent;
        private bool _isSyncAvailable;
        private LocalSyncDigest _localDigest;
        private string _log = string.Empty;

        public LocalSyncDigestService(IServiceAgent serviceAgent, ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestRepository = localSyncDigestRepository;
        }

        /// <summary>
        ///     makes call to BuildAndSendRequest method to make service call to get LocalSyncDigest data.
        ///     Once we get the response converts it into JsonObject.
        ///     Makes call to CheckSyncAvailable method to check whether sync is available or not.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SyncLocalDigest(string entity, string queryEntity)
        {
            try
            {
                HttpResponseMessage localDigestResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, entity, queryEntity, null,
                            Constants.AccessToken, null);
                if (localDigestResponse != null && localDigestResponse.StatusCode == HttpStatusCode.OK)
                {
                    // Make's call to ConvertToJsonObject and inturn we will get JsonObject
                    JsonObject sDataLocalDigest = await _serviceAgent.ConvertTosDataObject(localDigestResponse);

                    // Check's whether Sync is available or not to go forward
                    _isSyncAvailable = await CheckSyncAvailable(sDataLocalDigest);
                }
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (ArgumentException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _isSyncAvailable;
        }
        



        /// <summary>
        ///     Makes call to BuildAndPostRequest method to make POST request to service.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SyncLocalSource(string entity, string queryEntity)
        {
            var parameters = new Dictionary<string, string>();
            try
            {
                // Adding parameters to Dictionary object which we use to make POST request.
                parameters.Add("TrackingId", Constants.TrackingId);
                if (_localDigest != null)
                {
                    parameters.Add("LocalTick", _localDigest.localTick.ToString());
                }
                else
                {
                    parameters.Add("LocalTick", "0");
                }
                ;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return
                await
                    _serviceAgent.BuildAndPostRequest(Constants.TenantId, entity, queryEntity, Constants.AccessToken,
                        parameters);
        }


        # region Private Methods

        /// <summary>
        ///     Makes call to save LocalSyncDigest data into LocalDB
        /// </summary>
        /// <param name="sDataLocalDigest"></param>
        /// <returns></returns>
        private async Task<LocalSyncDigest> SaveLocalDigest(JsonObject sDataLocalDigest)
        {
            LocalSyncDigest digest = null;
            try
            {
                if (sDataLocalDigest.GetNamedString("ResourceKind") != null)
                {
                    digest =
                        await
                            _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(
                                sDataLocalDigest.GetNamedString("ResourceKind"));
                    if (digest == null)
                    {
                        digest = await _localSyncDigestRepository.SaveLocalSyncDigestDtlsAsync(sDataLocalDigest);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return digest;
        }

        /// <summary>
        ///     Checks whether Sync is available or not by comparing with LocalTick & the Tick which service returned.
        ///     If Tick value is greater than LocalTick we will start our Sync process.
        /// </summary>
        /// <param name="sDataLocalDigest"></param>
        /// <returns>bool value(Whether sync is available or not)</returns>
        private async Task<bool> CheckSyncAvailable(JsonObject sDataLocalDigest)
        {
            try
            {
                _localDigest =
                    await
                        _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(
                            sDataLocalDigest.GetNamedString("ResourceKind"));
                if (_localDigest != null)
                {
                    if (Convert.ToInt32(sDataLocalDigest.GetNamedNumber("Tick")) >= _localDigest.localTick)
                    {
                        _isSyncAvailable = true;
                    }
                    else
                    {
                        _isSyncAvailable = false;
                    }
                }
                else
                {
                    _isSyncAvailable = true;
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _isSyncAvailable;
        }

        # endregion
    }
}