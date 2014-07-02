using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.ServiceAgents.Common
{
    public interface IServiceAgent
    {
        Task<JsonObject> ConvertTosDataObject(HttpResponseMessage sDataResponse);
        Task<HttpResponseMessage> BuildAndSendRequest(string entity, string queryEntity, string AssociatedItems, string accessToken, Dictionary<string, string> parameters);
        Task<bool> BuildAndPostRequest(string entity, string queryEntity, string accessToken, Dictionary<string, string> parameters);
        Task<HttpResponseMessage> BuildAndPostObjectRequest(string entity, string queryEntity, string accessToken, Dictionary<string, string> parameters, object obj);
        Task<HttpResponseMessage> BuildAndPutObjectRequest(string entity, string queryEntity, string accessToken, Dictionary<string, string> parameters, object obj);
        Task<HttpResponseMessage> BuildAndDeleteRequest(string entity, string queryEntity, string accessToken, Dictionary<string, string> parameters);
    }
}
