using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Newtonsoft.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.ServiceAgents.Services;

namespace SageMobileSales.ServiceAgents.Common
{
    public class ServiceAgent : IServiceAgent
    {
        private readonly IOAuthService _oAuthService;

        public ServiceAgent(IOAuthService oAuthService)
        {
            var configSettings = ApplicationData.Current.LocalSettings;
            _oAuthService = oAuthService;
            //  _requestUrl = Constants.Url;
        }

        # region Local Variables

        private string _log = string.Empty;
        private string _requestUrl;

        # endregion

        #region public methods

        /// <summary>
        ///     Prepare's Request(Get) based on the parameters passed and make a call to service
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryEntity"></param>
        /// <param name="AssociatedItems"></param>
        /// <param name="accessToken"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponse</returns>
        public async Task<HttpResponseMessage> BuildAndSendRequest(string tenantId, string entity, string queryEntity,
            string associatedItems, string accessToken, Dictionary<string, string> parameters)
        {
            _requestUrl = Constants.Url;
            try
            {
                if (!Constants.IsDbDeleted)
                {
                    if (Constants.ConnectedToInternet())
                    {
                        var _url = string.Empty;
                        var _parameters = string.Empty;
                        HttpRequestMessage req = null;

                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            _requestUrl = _requestUrl + tenantId + "/";
                        }

                        if (entity != null && queryEntity != null)
                        {
                            _url += _requestUrl + entity + "/" + queryEntity;
                        }
                        else if (entity != null && queryEntity == null)
                        {
                            _url += _requestUrl + entity;
                        }
                        else
                        {
                            _url += _requestUrl + queryEntity;
                        }
                        if (parameters != null)
                        {
                            for (var parameter = 0; parameter < parameters.Count; parameter++)
                            {
                                if (parameter == parameters.Count - 1)
                                {
                                    if (parameters.ElementAt(parameter).Value != null)
                                        _parameters += parameters.ElementAt(parameter).Key + "=" +
                                                       parameters.ElementAt(parameter).Value;
                                }
                                else
                                {
                                    if (parameters.ElementAt(parameter).Value != null)
                                        _parameters += parameters.ElementAt(parameter).Key + "=" +
                                                       parameters.ElementAt(parameter).Value + "&";
                                }
                            }
                            _url += "?" + _parameters;
                        }
                        if (associatedItems != null)
                        {
                            _url += "," + associatedItems;
                        }
                        var httpClient = new HttpClient();
                        HttpResponseMessage response = null;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                            accessToken);

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                            "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                        //http://172.29.59.122:8080/sdata/api/msales/1.0/f200ac19-1be6-48c5-b604-2d322020f48e/Customers/$SyncDigest
                        //http://172.29.59.122:8080/sdata/api/msales/1.0/Customers/$SyncDigest
                        //http://ashmsalessdcweb.cloudapp.net/sdata/api/msales/1.0/70b032c0-385d-42be-8f53-2d10c53206ca/Orders/$SyncSource('AwDYagUApEAFAFruBgABAAQALiUEACC3BABUvgEAOhICALabCQD6oA==d2df1527-6d3b-4970-afc6-dea092358e13')?LocalTick=0&Count=100&where=SalesRep.id eq 'd2df1527-6d3b-4970-afc6-dea092358e13'&include=Details,Details/InventoryItem&select=*,Details/Price,Details/Quantity,Details/InventoryItem/Name,Details/InventoryItem/Sku
                        //http://172.29.59.122:8080/sdata/api/msales/1.0/f200ac19-1be6-48c5-b604-2d322020f48e/Orders/$SyncSource('B181349C-FFEC-42FD-9A20-B83A5C07F7A6-8e144a26-f89a-4a7f-9265-8a9453a27222')?Count=50&LocalTick=0 

                        req = new HttpRequestMessage(HttpMethod.Get, _url);
                        if (Constants.ConnectedToInternet())
                        {
                            response = await httpClient.SendAsync(req);
                            if (response != null)
                            {
                                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                                {
                                    return
                                        response = await GetRefreshTokenAndMakeRequest(req, _url, HttpMethod.Get, null);
                                }
                            }
                        }

                        //if (response.StatusCode == HttpStatusCode.Unauthorized)
                        //{
                        //    req = Clone(req);
                        //    await _oAuthService.Authorize();
                        //    response = await httpClient.SendAsync(req);
                        //}
                        return response;
                    }

                    return null;
                }
                return null;
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        ///     Builds Posts (http post method) based on the parameters passed and make a call to service.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryEntity"></param>
        /// <param name="accessToken"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponse which in turn service returns</returns>
        public async Task<bool> BuildAndPostRequest(string tenantId, string entity, string queryEntity,
            string accessToken,
            Dictionary<string, string> parameters)
        {
            _requestUrl = Constants.Url;
            try
            {
                if (!Constants.IsDbDeleted)
                {
                    var _url = string.Empty;
                    var _parameters = string.Empty;

                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        _requestUrl = _requestUrl + tenantId + "/";
                    }

                    if (queryEntity != null)
                    {
                        _url += _requestUrl + entity + "/" + queryEntity;
                    }
                    else
                    {
                        _url += _requestUrl + entity;
                    }

                    var httpClient = new HttpClient();
                    HttpResponseMessage response = null;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                        "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                    var serialized = JsonConvert.SerializeObject(parameters);
                    if (Constants.ConnectedToInternet())
                    {
                        response =
                            await
                                httpClient.PostAsync(_url,
                                    new StringContent(serialized, Encoding.UTF8, "application/json"));
                        if (response != null)
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                            {
                                response =
                                    await
                                        GetRefreshTokenAndMakeRequest(null, _url, HttpMethod.Post,
                                            new StringContent(serialized, Encoding.UTF8, "application/json"));
                                return response.IsSuccessStatusCode;
                            }
                        }
                    }
                    return response.IsSuccessStatusCode;
                }
                return false;
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
            catch (TimeoutException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            return false;
        }

        /// <summary>
        ///     Builds serialized object and Posts (http post method) based on the parameters passed and make a call to service.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryEntity"></param>
        /// <param name="accessToken"></param>
        /// <param name="parameters"></param>
        /// <param name="obj"></param>
        /// <returns>HttpResponse which in turn service returns</returns>
        public async Task<HttpResponseMessage> BuildAndPostObjectRequest(string tenantId, string entity,
            string queryEntity,
            string accessToken, Dictionary<string, string> parameters, object obj)
        {
            _requestUrl = Constants.Url;
            HttpResponseMessage response = null;
            try
            {
                var _url = string.Empty;
                var _parameters = string.Empty;

                if (!string.IsNullOrEmpty(tenantId))
                {
                    _requestUrl = _requestUrl + tenantId + "/";
                }

                if (queryEntity != null)
                {
                    _url += _requestUrl + entity + "/" + queryEntity;
                }
                else
                {
                    _url += _requestUrl + entity;
                }

                if (parameters != null)
                {
                    for (var parameter = 0; parameter < parameters.Count; parameter++)
                    {
                        if (parameter == parameters.Count - 1)
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value;
                        }
                        else
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value + "&";
                        }
                    }
                    _url += "?" + _parameters;
                }

                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                var serialized = GetSerializedObject(obj);

                if (Constants.ConnectedToInternet())
                {
                    response =
                        await
                            httpClient.PostAsync(_url, new StringContent(serialized, Encoding.UTF8, "application/json"));
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                        {
                            response =
                                await
                                    GetRefreshTokenAndMakeRequest(null, _url, HttpMethod.Post,
                                        new StringContent(serialized, Encoding.UTF8, "application/json"));
                            return response;
                        }
                    }
                }
                return response;
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
            catch (TimeoutException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return response;
        }

        /// <summary>
        ///     Builds serialized object and uses put(http put method) to update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryEntity"></param>
        /// <param name="accessToken"></param>
        /// <param name="parameters"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> BuildAndPutObjectRequest(string entity, string queryEntity,
            string accessToken, Dictionary<string, string> parameters, object obj)
        {
            _requestUrl = Constants.Url;
            HttpResponseMessage response = null;
            try
            {
                var _url = string.Empty;
                var _parameters = string.Empty;
                if (queryEntity != null)
                {
                    _url += _requestUrl + entity + "/" + queryEntity;
                }
                else
                {
                    _url += _requestUrl + entity;
                }

                if (parameters != null)
                {
                    for (var parameter = 0; parameter < parameters.Count; parameter++)
                    {
                        if (parameter == parameters.Count - 1)
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value;
                        }
                        else
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value + "&";
                        }
                    }
                    _url += "?" + _parameters;
                }

                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                var serialized = GetSerializedObject(obj);

                if (Constants.ConnectedToInternet())
                {
                    response =
                        await
                            httpClient.PutAsync(_url, new StringContent(serialized, Encoding.UTF8, "application/json"));
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                        {
                            response =
                                await
                                    GetRefreshTokenAndMakeRequest(null, _url, HttpMethod.Put,
                                        new StringContent(serialized, Encoding.UTF8, "application/json"));
                            return response;
                        }
                    }
                }
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
            catch (TimeoutException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return response;
        }


        public async Task<HttpResponseMessage> BuildAndPatchObjectRequest(string tenantId, string entity,
            string queryEntity,
            string accessToken, Dictionary<string, string> parameters, object obj)
        {
            _requestUrl = Constants.Url;
            HttpResponseMessage response = null;
            try
            {
                var _url = string.Empty;
                var _parameters = string.Empty;

                if (!string.IsNullOrEmpty(tenantId))
                {
                    _requestUrl = _requestUrl + tenantId + "/";
                }

                if (queryEntity != null)
                {
                    _url += _requestUrl + entity + "/" + queryEntity;
                }
                else
                {
                    _url += _requestUrl + entity;
                }

                if (parameters != null)
                {
                    for (var parameter = 0; parameter < parameters.Count; parameter++)
                    {
                        if (parameter == parameters.Count - 1)
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value;
                        }
                        else
                        {
                            if (parameters.ElementAt(parameter).Value != null)
                                _parameters += parameters.ElementAt(parameter).Key + "=" +
                                               parameters.ElementAt(parameter).Value + "&";
                        }
                    }
                    _url += "?" + _parameters;
                }

                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");


                //public async static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
                //{
                var method = new HttpMethod("PATCH");

                var serialized = GetSerializedObject(obj);

                var request = new HttpRequestMessage(method, _url)
                {
                    Content = new StringContent(serialized, Encoding.UTF8, "application/json")
                };
                if (Constants.ConnectedToInternet())
                {
                    response = await httpClient.SendAsync(request);
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                        {
                            response = await GetRefreshTokenAndMakeRequest(request, _url, method, null);
                            return response;
                        }
                    }
                }

                //}

                //string serialized = GetSerializedObject(obj);

                //if (Constants.ConnectedToInternet())
                //{
                //    response =
                //        await
                //            httpClient.PutAsync(_url, new StringContent(serialized, Encoding.UTF8, "application/json"));
                //}
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
            catch (TimeoutException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return response;
        }

        /// <summary>
        ///     Builds and uses delete(http method) to delete
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryEntity"></param>
        /// <param name="accessToken"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> BuildAndDeleteRequest(string tenantId, string entity, string queryEntity,
            string accessToken, Dictionary<string, string> parameters)
        {
            _requestUrl = Constants.Url;
            HttpResponseMessage response = null;
            try
            {
                var _url = string.Empty;
                var _parameters = string.Empty;

                if (!string.IsNullOrEmpty(tenantId))
                {
                    _requestUrl = _requestUrl + tenantId + "/";
                }

                if (queryEntity != null)
                {
                    _url += _requestUrl + entity + "/" + queryEntity;
                }
                else
                {
                    _url += _requestUrl + entity;
                }

                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                //var serialized = GetSerializedObject(obj);

                if (Constants.ConnectedToInternet())
                {
                    response = await httpClient.DeleteAsync(_url);
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == 0)
                        {
                            response = await GetRefreshTokenAndMakeRequest(null, _url, HttpMethod.Delete, null);
                            return response;
                        }
                    }
                }
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
            catch (TimeoutException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return response;
        }


        /// <summary>
        ///     Convert's HttpResponse into JsonObject
        /// </summary>
        /// <param name="sDataResponse"></param>
        /// <returns>JsonObject</returns>
        public async Task<JsonObject> ConvertTosDataObject(HttpResponseMessage sDataResponse)
        {
            try
            {
                var responseBodyAsText = await sDataResponse.Content.ReadAsStringAsync();
                var responsesDataObject = JsonValue.Parse(responseBodyAsText).GetObject();
                return responsesDataObject;
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Returns Serialized Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string GetSerializedObject(Object obj)
        {
            var serialized = JsonConvert.SerializeObject(obj);
            serialized = serialized.Replace("\"key\"", "\"$key\"");
            serialized = serialized.Replace("\"resources", "\"$resources");

            return serialized;
        }

        /// <summary>
        ///     Clone
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private HttpRequestMessage Clone(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            clone.Content = req.Content;
            clone.Version = req.Version;

            foreach (var prop in req.Properties)
            {
                clone.Properties.Add(prop);
            }

            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        /// <summary>
        ///     Get Refresh token from WINRT library and sending request again to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <param name="requestType"></param>
        /// ///
        /// <param name="requestConent"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetRefreshTokenAndMakeRequest(HttpRequestMessage request, string url,
            HttpMethod requestType, HttpContent requestConent)
        {
            HttpResponseMessage response = null;
            try
            {
                request = Clone(request);
                Constants.AccessToken = await _oAuthService.Authorize();

                var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    Constants.AccessToken);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocol", "Mobile Sales");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("AppProtocolVersion", "1.0.0.0");

                if (requestType == HttpMethod.Get || requestType == new HttpMethod("PATCH"))
                {
                    request = new HttpRequestMessage(requestType, url);
                    response = await httpClient.SendAsync(request);
                }
                else if (requestType == HttpMethod.Post)
                {
                    response =
                        await
                            httpClient.PostAsync(url, requestConent);
                }
                else if (requestType == HttpMethod.Put)
                {
                    response =
                        await
                            httpClient.PutAsync(url, requestConent);
                }
                else if (requestType == HttpMethod.Delete)
                {
                    response = await httpClient.DeleteAsync(url);
                }

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        return response;
                    }
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return response;
        }

        #endregion
    }
}