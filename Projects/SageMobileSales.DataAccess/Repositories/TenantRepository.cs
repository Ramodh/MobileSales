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
        public async Task SaveTenantAsync(JsonArray sDataTenantArray, string repId)
        {
            try
            {
                //JsonArray sDataTenantArray = sDataTenantDtls.GetNamedArray("$resources");
                //JsonArray sDataTenantArray = sDataTenantDtls.GetArray();
                if (sDataTenantArray.Count > 0)
                {
                    //JsonObject sDataTentObject = sDataTenantArray[0].GetObject();
                    //Tenant tenantDtls = ExtractTenantDtlsFromJsonAsync(sDataTentObject);
                    //await AddOrUpdateTenantAsync(tenantDtls);

                    await SaveTenantDetailsAsync(sDataTenantArray, repId);
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

        public async Task<string> GetTenantId()
        {
            try
            {
                List<Tenant> tenants =
                    await _sageSalesDB.QueryAsync<Tenant>("select * from Tenant");
                if (tenants.Count > 0)
                {
                    return tenants.FirstOrDefault().TenantId;
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
            return null;
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

        private async Task SaveTenantDetailsAsync(JsonArray sDataTenantArray, string repId)
        {
            foreach (var tenant in sDataTenantArray)
            {
                JsonObject sDataTenant = tenant.GetObject();
                await AddOrUpdateTenantJsonToDbAsync(sDataTenant, repId);
            }
        }

        private async Task<Tenant> AddOrUpdateTenantJsonToDbAsync(JsonObject sDataTenant, string repId)
        {
            try
            {
                IJsonValue value;
                if (sDataTenant.TryGetValue("TenantGuid", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Tenant> tenantList;
                        tenantList =
                            await
                                _sageSalesDB.QueryAsync<Tenant>("SELECT * FROM Tenant where TenantId=?",
                                    sDataTenant.GetNamedString("TenantGuid"));

                        if (tenantList.FirstOrDefault() != null)
                        {
                            return await UpdateTenantJsonToDbAsync(sDataTenant, tenantList.FirstOrDefault());
                        }
                        return await AddTenantJsonToDbAsync(sDataTenant, repId);
                    }
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

            return null;
        }

        private async Task<Tenant> AddTenantJsonToDbAsync(JsonObject sDataTenant, string repId)
        {
            var tenantObj = new Tenant();
            try
            {
                tenantObj.RepId = repId;
                tenantObj.TenantId = sDataTenant.GetNamedString("TenantGuid");

                tenantObj = ExtractTenantDtlsFromJsonAsync(sDataTenant, tenantObj);
                await _sageSalesDB.InsertAsync(tenantObj);
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
            return tenantObj;
        }

        private async Task<Tenant> UpdateTenantJsonToDbAsync(JsonObject sDataTenant, Tenant tenantObj)
        {
            try
            {
                tenantObj = ExtractTenantDtlsFromJsonAsync(sDataTenant, tenantObj);
                await _sageSalesDB.UpdateAsync(tenantObj);
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
            return tenantObj;
        }

        ///// <summary>
        /////     Adds or Updates TenantDtls to localDB
        ///// </summary>
        ///// <param name="tenantDtls"></param>
        ///// <returns></returns>
        //private async Task AddOrUpdateTenantAsync(Tenant tenantDtls)
        //{
        //    try
        //    {
        //        Tenant localDBTenant = await GetTenantDtlsAsync(tenantDtls.TenantId);
        //        if (localDBTenant != null)
        //        {
        //            await _sageSalesDB.UpdateAsync(tenantDtls);
        //        }
        //        else
        //        {
        //            await _sageSalesDB.InsertAsync(tenantDtls);
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        _log = AppEventSource.Log.WriteLine(ex);
        //        AppEventSource.Log.Error(_log);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log = AppEventSource.Log.WriteLine(ex);
        //        AppEventSource.Log.Error(_log);
        //    }
        //}

        private Tenant ExtractTenantDtlsFromJsonAsync(JsonObject sDataTenant, Tenant tenant)
        {
            IJsonValue value;

            if (sDataTenant.TryGetValue("UmTenantName", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Name = sDataTenant.GetNamedString("UmTenantName");
                }
            }
            /*
            if (sDataTenant.TryGetValue("ERPCompanyCode", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Code = sDataTenant.GetNamedString("ERPCompanyCode");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyName", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Name = sDataTenant.GetNamedString("ERPCompanyName");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyAddressLine1", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine1 = sDataTenant.GetNamedString("ERPCompanyAddressLine1");
                }
            }

            if (sDataTenant.TryGetValue("ERPCompanyAddressLine2", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine2 = sDataTenant.GetNamedString("ERPCompanyAddressLine2");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyAddressLine3", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine3 = sDataTenant.GetNamedString("ERPCompanyAddressLine3");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyAddressLine4", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine4 = sDataTenant.GetNamedString("ERPCompanyAddressLine4");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyCity", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.City = sDataTenant.GetNamedString("ERPCompanyCity");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyRegion", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Region = sDataTenant.GetNamedString("ERPCompanyRegion");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyCountry", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.County = sDataTenant.GetNamedString("ERPCompanyCountry");
                }
            }
            if (sDataTenant.TryGetValue("ERPCompanyPostalCode", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.PostalCode = sDataTenant.GetNamedString("ERPCompanyPostalCode");
                }
            }
             */
            return tenant;
        }

        # endregion
    }
}