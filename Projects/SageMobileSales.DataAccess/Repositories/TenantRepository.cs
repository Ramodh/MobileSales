using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public TenantRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        #region public methods

        /// <summary>
        ///     Calls ExtractTenantDtlsFromJsonAsync & AddOrUpdateTenantAsync Methods to save sDataTenant into  localDB
        /// </summary>
        /// <param name="sDataTenantDtls"></param>
        /// <returns></returns>
        public async Task SaveTenantDtlsAsync(JsonObject sDataTenantDtls)
        {
            try
            {
                JsonArray sDataTenantArray = sDataTenantDtls.GetNamedArray("$resources");
                if (sDataTenantArray.Count > 0)
                {
                    JsonObject sDataTentObject = sDataTenantArray[0].GetObject();
                    Tenant tenantDtls = ExtractTenantDtlsFromJsonAsync(sDataTentObject);
                    await AddOrUpdateTenantAsync(tenantDtls);
                }
            }
            catch (SQLiteException ex)
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

        /// <summary>
        ///     Gets TenantDtls from localDB
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public async Task<Tenant> GetTenantDtlsAsync(string tenantId)
        {
            try
            {
                List<Tenant> tenants =
                    await _sageSalesDB.QueryAsync<Tenant>("select * from Tenant where Tenant.TenantId=?", tenantId);
                if (tenants.Count > 0)
                {
                    return tenants.FirstOrDefault();
                }
                return null;
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        # endregion

        # region private methods

        /// <summary>
        ///     Adds or Updates TenantDtls to localDB
        /// </summary>
        /// <param name="tenantDtls"></param>
        /// <returns></returns>
        private async Task AddOrUpdateTenantAsync(Tenant tenantDtls)
        {
            try
            {
                Tenant localDBTenant = await GetTenantDtlsAsync(tenantDtls.TenantId);
                if (localDBTenant != null)
                {
                    await _sageSalesDB.UpdateAsync(tenantDtls);
                }
                else
                {
                    await _sageSalesDB.InsertAsync(tenantDtls);
                }
            }
            catch (SQLiteException ex)
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

        private Tenant ExtractTenantDtlsFromJsonAsync(JsonObject sDataTentObject)
        {
            IJsonValue value;
            var tenant = new Tenant();

            if (sDataTentObject.TryGetValue("$key", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.TenantId = sDataTentObject.GetNamedString("$key");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyCode", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Code = sDataTentObject.GetNamedString("ERPCompanyCode");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyName", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Name = sDataTentObject.GetNamedString("ERPCompanyName");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyAddressLine1", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine1 = sDataTentObject.GetNamedString("ERPCompanyAddressLine1");
                }
            }

            if (sDataTentObject.TryGetValue("ERPCompanyAddressLine2", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine2 = sDataTentObject.GetNamedString("ERPCompanyAddressLine2");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyAddressLine3", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine3 = sDataTentObject.GetNamedString("ERPCompanyAddressLine3");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyAddressLine4", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine4 = sDataTentObject.GetNamedString("ERPCompanyAddressLine4");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyCity", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.City = sDataTentObject.GetNamedString("ERPCompanyCity");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyRegion", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Region = sDataTentObject.GetNamedString("ERPCompanyRegion");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyCountry", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.County = sDataTentObject.GetNamedString("ERPCompanyCountry");
                }
            }
            if (sDataTentObject.TryGetValue("ERPCompanyPostalCode", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.PostalCode = sDataTentObject.GetNamedString("ERPCompanyPostalCode");
                }
            }
            return tenant;
        }

        # endregion
    }
}