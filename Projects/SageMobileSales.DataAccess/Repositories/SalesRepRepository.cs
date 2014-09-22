using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SQLite;
using System.Diagnostics;

namespace SageMobileSales.DataAccess.Repositories
{
    public class SalesRepRepository : ISalesRepRepository
    {
        private readonly IDatabase _database;
        private readonly ITenantRepository _tenantRepository;
        private string _log = string.Empty;
        private SQLiteAsyncConnection _sageSalesDB;

        public SalesRepRepository(IDatabase database, ITenantRepository tenantRepository)
        {
            _database = database;
            _database.Initialize();
            _sageSalesDB = _database.GetAsyncConnection();
            _tenantRepository = tenantRepository;
        }

        #region public methods
        /// <summary>
        ///     Extract and saves LoginUserdetails(SalesRep) into local dB
        /// </summary>
        /// <param name="sDataSalesRepDtls"></param>
        /// <returns>LoginUserId(RepId)</returns>
        public async Task<string> SaveSalesRepDtlsAsync(JsonObject sDataSalesRepDtls)
        {
            var salesRepDtls = new SalesRep();
            try
            {
                // ReIntializing the Database as if user clicks on Logout and start login again the database won't be initiated as 
                // we are intializing the database in the Constructor which will call only once that to for the first time when application is loaded
                // because we are using Unity Container for dependency injection.

                //await _database.Initialize();
                //_sageSalesDB = _database.GetAsyncConnection();


                /*--- Pegausus 
                //JsonArray sDataSalesRepArray = sDataSalesRepDtls.GetNamedArray("$resources");

                foreach (IJsonValue salesRep in sDataSalesRepArray)
                {
                    JsonObject sDataSalesRep = salesRep.GetObject();
                    _salesRepDtls = await AddOrUpdateSalesRepJsonToDbAsync(sDataSalesRep);
                }
                 * */

                salesRepDtls = await AddOrUpdateSalesRepJsonToDbAsync(sDataSalesRepDtls);
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
            return salesRepDtls.RepId;
        }

        /// <summary>
        ///     Update Logged in user details(SalesRep-MaxDiscPerct) into LocalDB
        /// </summary>
        /// <param name="sDataSalesRepSettings"></param>
        /// <returns></returns>
        public async Task<bool> UpdateSalesRepDtlsAsync(JsonObject sDataSalesTeamMembers)
        {
            bool isSalesPersonIdChanged = false;
            try
            {
                IJsonValue value;
                JsonObject sDataSalesTeamMember = null;
                JsonObject sDataSalesTeamMemberDetails = null;

                SalesRep _salesRepDtls = await _sageSalesDB.Table<SalesRep>().FirstOrDefaultAsync();

                if (sDataSalesTeamMembers.TryGetValue("$resources", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonArray sDataSalesTeamMemberArray = sDataSalesTeamMembers.GetNamedArray("$resources");
                        foreach (IJsonValue salesRep in sDataSalesTeamMemberArray)
                            sDataSalesTeamMember = salesRep.GetObject();
                    }
                }

                if (sDataSalesTeamMember.TryGetValue("SalesTeamMember", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        sDataSalesTeamMemberDetails = sDataSalesTeamMember.GetNamedObject("SalesTeamMember");

                        if (sDataSalesTeamMemberDetails.TryGetValue("SalesRepMaxDiscPct", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                _salesRepDtls.MaximumDiscountPercent =
                                    Convert.ToDecimal(sDataSalesTeamMemberDetails.GetNamedNumber("SalesRepMaxDiscPct"));
                            }
                        }

                        //Compare for salespersonIds
                        if (sDataSalesTeamMemberDetails.TryGetValue("SalespersonIds", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                string salesPersonDb = _salesRepDtls.SalesPersonIds;
                                _salesRepDtls.SalesPersonIds = string.Empty;

                                JsonArray salesPersonIdArray = sDataSalesTeamMemberDetails.GetNamedArray("SalespersonIds");
                                foreach (var salesPersonId in salesPersonIdArray)
                                    _salesRepDtls.SalesPersonIds = _salesRepDtls.SalesPersonIds + salesPersonId.GetString();

                                //List<SalesRep> UserSalesPersonIds = await _sageSalesDB.Table<SalesRep>().ToListAsync();
                                Debug.WriteLine("Json : " + _salesRepDtls.SalesPersonIds + "  =  dB : " + salesPersonDb);

                                if (!string.IsNullOrEmpty(salesPersonDb) &&
                                    !_salesRepDtls.SalesPersonIds.Equals(salesPersonDb))
                                    isSalesPersonIdChanged = true;
                            }
                        }
                    }
                }

                // Updates SalesRep data into SalesRep table
                await _sageSalesDB.UpdateAsync(_salesRepDtls);
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
            return isSalesPersonIdChanged;
        }

        /// <summary>
        ///     Deletes LoginUserdetails(SalesRep) from LocalDB
        /// </summary>
        /// <param name="salesRep"></param>
        /// <returns></returns>
        //public async Task DeleteSalesRepDtlsAsync(SalesRep salesRep)
        //{
        //    try
        //    {
        //        await _sageSalesDB.DeleteAsync(salesRep);
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
        ///     Gets LoginUserdetails(SalesRep) from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesRep>> GetSalesRepDtlsAsync()
        {
            try
            {
                List<SalesRep> UserDtls = await _sageSalesDB.Table<SalesRep>().ToListAsync();
                return UserDtls;
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
        ///     Gets Sales Rep Id
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSalesRepId()
        {
            try
            {
                List<SalesRep> UserDtls = await _sageSalesDB.Table<SalesRep>().ToListAsync();
                return UserDtls.FirstOrDefault().RepId;
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
        ///     Add or update salesrep json to local dB
        /// </summary>
        /// <param name="sDataSalesRep"></param>
        /// <returns></returns>
        public async Task<SalesRep> AddOrUpdateSalesRepJsonToDbAsync(JsonObject sDataSalesRepDtls)
        {
            try
            {
                ApplicationDataContainer configSettings = ApplicationData.Current.LocalSettings;

                IJsonValue value;
                if (sDataSalesRepDtls.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<SalesRep> salesRepList;
                        salesRepList =
                            await
                                _sageSalesDB.QueryAsync<SalesRep>("SELECT * FROM SalesRep where RepId=?",
                                    sDataSalesRepDtls.GetNamedString("$key"));

                        if (salesRepList.FirstOrDefault() != null)
                        {
                            //if (configSettings.Containers["ConfigurationSettingsContainer"] != null)
                            //{
                            //    if (!string.IsNullOrEmpty(configSettings.Containers["ConfigurationSettingsContainer"].Values["_previousSelectedType"].ToString()))
                            //    {
                            //        if (DataAccessUtils.SelectedServerType != configSettings.Containers["ConfigurationSettingsContainer"].Values["_previousSelectedType"].ToString())
                            //        {
                            //            //await _database.Delete();
                            //            await _database.Initialize();
                            //            _sageSalesDB = _database.GetAsyncConnection();
                            //        }
                            //    }
                            //}
                            return await UpdateSalesRepJsonToDbAsync(sDataSalesRepDtls, salesRepList.FirstOrDefault());
                        }
                        if (!DataAccessUtils.IsServerChanged)
                        {
                            await _database.Delete();
                            await _database.Initialize();
                            _sageSalesDB = _database.GetAsyncConnection();
                        }
                        else
                        {
                            DataAccessUtils.IsServerChanged = false;
                            if (configSettings.Containers["ConfigurationSettingsContainer"] != null)
                            {
                                configSettings.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"] =
                                    false;
                            }
                        }

                        return await AddSalesRepJsonToDbAsync(sDataSalesRepDtls);
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
        ///     Checks Whether Logged in user is same as previously logged in user.
        /// </summary>
        /// <param name="repId"></param>
        /// <returns></returns>
        public async Task<bool> CheckPreviousLoggedinUser(string repId)
        {
            bool isSameUser = false;
            try
            {
                List<SalesRep> salesRep =
                    await _sageSalesDB.QueryAsync<SalesRep>("select * from SalesRep where RepId=?", repId);
                if (salesRep != null && salesRep.Count > 0)
                {
                    isSameUser = true;
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
            return isSameUser;
        }

        #endregion

        #region private methods
        /// <summary>
        ///     Get Sales Rep data from json
        /// </summary>
        /// <param name="sDataSalesRep"></param>
        /// <param name="salesRepDBObj"></param>
        /// <returns></returns>
        private SalesRep GetSalesRepDataFromJson(JsonObject sDataSalesRep, SalesRep salesRepDBObj)
        {
            try
            {
                IJsonValue value;

                if (sDataSalesRep.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesRepDBObj.RepId = sDataSalesRep.GetNamedString("$key");
                    }
                }
                if (sDataSalesRep.TryGetValue("PrimaryEmailAddress", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesRepDBObj.EmailAddress = sDataSalesRep.GetNamedString("PrimaryEmailAddress");
                    }
                }
                if (sDataSalesRep.TryGetValue("FirstName", out value) ||
                    sDataSalesRep.TryGetValue("LastName", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesRepDBObj.RepName = sDataSalesRep.ContainsKey("LastName")
                            ? sDataSalesRep.GetNamedString("FirstName") + ',' + sDataSalesRep.GetNamedString("LastName")
                            : sDataSalesRep.GetNamedString("FirstName");
                    }
                }
                if (sDataSalesRep.TryGetValue("Phone", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesRepDBObj.Phone = sDataSalesRep.GetNamedString("Phone");
                    }
                }
                /* pegausus
                if (sDataSalesRep.TryGetValue("TenantId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        salesRepDBObj.TenantId = sDataSalesRep.GetNamedString("TenantId");
                    }
                }
                 * */
                if (sDataSalesRep.TryGetValue("Tenants", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonArray sDataTenants = sDataSalesRep.GetNamedArray("Tenants");
                        _tenantRepository.SaveTenantAsync(sDataTenants, salesRepDBObj.RepId);
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
            return salesRepDBObj;
        }

        /// <summary>
        /// Add salesRep to local dB
        /// </summary>
        /// <param name="sDataSalesRep"></param>
        /// <returns></returns>
        private async Task<SalesRep> AddSalesRepJsonToDbAsync(JsonObject sDataSalesRep)
        {
            var salesRep = new SalesRep();
            try
            {
                salesRep = GetSalesRepDataFromJson(sDataSalesRep, salesRep);
                await _sageSalesDB.InsertAsync(salesRep);
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
            return salesRep;
        }

        /// <summary>
        /// Update sales rep to local dB
        /// </summary>
        /// <param name="sDataSalesRep"></param>
        /// <param name="salesRep"></param>
        /// <returns></returns>
        private async Task<SalesRep> UpdateSalesRepJsonToDbAsync(JsonObject sDataSalesRep, SalesRep salesRep)
        {
            try
            {
                salesRep = GetSalesRepDataFromJson(sDataSalesRep, salesRep);
                await _sageSalesDB.UpdateAsync(salesRep);
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
            return salesRep;
        }

        #endregion
    }
}