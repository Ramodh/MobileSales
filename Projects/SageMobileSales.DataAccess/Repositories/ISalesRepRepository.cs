using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ISalesRepRepository
    {
        Task<string> SaveSalesRepDtlsAsync(JsonObject salesRepDtlsJson);
        Task<bool> UpdateSalesRepDtlsAsync(JsonObject userSettingsDtlsJson);
        //Task DeleteSalesRepDtlsAsync(SalesRep salesRep);
        Task<List<SalesRep>> GetSalesRepDtlsAsync();
        Task<string> GetSalesRepId();
        Task<SalesRep> AddOrUpdateSalesRepJsonToDbAsync(JsonObject sDataSalesRep);
    }
}