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
        ///     Extract Tenants from json, save into  local dB
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

        /// <summary>
        ///     Gets tenantId
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTenantId()
        {
            try
            {
                var tenants =
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
                var tenants =
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

        /// <summary>
        ///     Update tenant dtls to local dB
        /// </summary>
        /// <param name="sDataTenants"></param>
        /// <param name="tenantGuid"></param>
        /// <returns></returns>
        public async Task UpdateTenantAsync(JsonObject sDataTenants, string tenantGuid)
        {
            try
            {
                IJsonValue value;
                JsonObject sDataTenantDetails = null;

                List<Tenant> tenantList;
                tenantList =
                    await
                        _sageSalesDB.QueryAsync<Tenant>("SELECT * FROM Tenant where TenantId=?",
                            tenantGuid);

                if (sDataTenants.TryGetValue("$resources", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataTenantArray = sDataTenants.GetNamedArray("$resources");
                        foreach (var tenant in sDataTenantArray)
                            sDataTenantDetails = tenant.GetObject();
                        //_tenantRepository.SaveTenantAsync(sDataTenants, salesRepDBObj.RepId);
                    }
                }

                await UpdateTenantJsonToDbAsync(sDataTenantDetails, tenantList.FirstOrDefault());
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

        # endregion

        # region private methods

        /// <summary>
        ///     Save tenant details to local dB
        /// </summary>
        /// <param name="sDataTenantArray"></param>
        /// <param name="repId"></param>
        /// <returns></returns>
        private async Task SaveTenantDetailsAsync(JsonArray sDataTenantArray, string repId)
        {
            foreach (var tenant in sDataTenantArray)
            {
                var sDataTenant = tenant.GetObject();
                await AddOrUpdateTenantJsonToDbAsync(sDataTenant, repId);
            }
        }

        /// <summary>
        ///     Add or Update tenant to local dB
        /// </summary>
        /// <param name="sDataTenant"></param>
        /// <param name="repId"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Add tenant to local dB
        /// </summary>
        /// <param name="sDataTenant"></param>
        /// <param name="repId"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Update tenant to local dB
        /// </summary>
        /// <param name="sDataTenantDetails"></param>
        /// <param name="tenantObj"></param>
        /// <returns></returns>
        private async Task<Tenant> UpdateTenantJsonToDbAsync(JsonObject sDataTenantDetails, Tenant tenantObj)
        {
            try
            {
                IJsonValue value;
                if (sDataTenantDetails.TryGetValue("CompanySettings", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        sDataTenantDetails = sDataTenantDetails.GetNamedObject("CompanySettings");
                    }
                }

                tenantObj = ExtractTenantDtlsFromJsonAsync(sDataTenantDetails, tenantObj);
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

        /// <summary>
        ///     Extract tenant from json
        /// </summary>
        /// <param name="sDataTenant"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private Tenant ExtractTenantDtlsFromJsonAsync(JsonObject sDataTenant, Tenant tenant)
        {
            IJsonValue value;

            /*if (sDataTenant.TryGetValue("UmTenantName", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Name = sDataTenant.GetNamedString("UmTenantName");
                }
            }
            
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

            if (sDataTenant.TryGetValue("Name", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Name = sDataTenant.GetNamedString("Name");
                }
            }

            if (sDataTenant.TryGetValue("Code", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Code = sDataTenant.GetNamedString("Code");
                }
            }
            //if (sDataTenant.TryGetValue("ERPCompanyName", out value))
            //{
            //    if (value.ValueType.ToString() != DataAccessUtils.Null)
            //    {
            //        tenant.Name = sDataTenant.GetNamedString("ERPCompanyName");
            //    }
            //}
            if (sDataTenant.TryGetValue("AddressLine1", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine1 = sDataTenant.GetNamedString("AddressLine1");
                }
            }

            if (sDataTenant.TryGetValue("AddressLine2", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine2 = sDataTenant.GetNamedString("AddressLine2");
                }
            }
            if (sDataTenant.TryGetValue("AddressLine3", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine3 = sDataTenant.GetNamedString("AddressLine3");
                }
            }
            if (sDataTenant.TryGetValue("AddressLine4", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.AddressLine4 = sDataTenant.GetNamedString("AddressLine4");
                }
            }
            if (sDataTenant.TryGetValue("City", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.City = sDataTenant.GetNamedString("City");
                }
            }
            if (sDataTenant.TryGetValue("Region", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.Region = sDataTenant.GetNamedString("Region");
                }
            }
            if (sDataTenant.TryGetValue("Country", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.County = sDataTenant.GetNamedString("Country");
                }
            }
            if (sDataTenant.TryGetValue("PostalCode", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    tenant.PostalCode = sDataTenant.GetNamedString("PostalCode");
                }
            }

            if (sDataTenant.TryGetValue("Applications", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    var Applications = sDataTenant.GetNamedArray("Applications");
                    foreach (var application in Applications)
                    {
                        var sDataTenantApplications = application.GetObject();
                        if (sDataTenantApplications.TryGetValue("Application", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                var sDataTenantApplication = sDataTenantApplications.GetNamedObject("Application");
                                if (sDataTenantApplication.GetNamedString("Name").Equals("SageMobileSales"))
                                {
                                    if (sDataTenantApplications.GetNamedString("EntitlementKind").Equals("Free"))
                                        DataAccessUtils.EntitlementKind = true;
                                }
                            }
                        }
                    }
                }
            }

            return tenant;
        }

        # endregion
    }
}