using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.ServiceAgents.Common
{
    public interface IServiceAgent
    {
        Task<JsonObject> ConvertTosDataObject(HttpResponseMessage sDataResponse);

        Task<HttpResponseMessage> BuildAndSendRequest(string tenantId, string entity, string queryEntity,
            string AssociatedItems,
            string accessToken, Dictionary<string, string> parameters);

        Task<bool> BuildAndPostRequest(string tenantId, string entity, string queryEntity, string accessToken,
            Dictionary<string, string> parameters);

        Task<HttpResponseMessage> BuildAndPostObjectRequest(string tenantId, string entity, string queryEntity,
            string accessToken,
            Dictionary<string, string> parameters, object obj);

        Task<HttpResponseMessage> BuildAndPutObjectRequest(string entity, string queryEntity, string accessToken,
            Dictionary<string, string> parameters, object obj);

        Task<HttpResponseMessage> BuildAndDeleteRequest(string tenantId, string entity, string queryEntity,
            string accessToken,
            Dictionary<string, string> parameters);

        Task<HttpResponseMessage> BuildAndPatchObjectRequest(string tenantId, string entity, string queryEntity,
            string accessToken, Dictionary<string, string> parameters, object obj);
    }
}