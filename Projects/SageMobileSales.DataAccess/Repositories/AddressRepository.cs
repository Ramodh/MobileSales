using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public AddressRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        #region public methods

        /// <summary>
        ///     Extracts address json and saves it in local dB for that customerId
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task SaveAddressesAsync(JsonObject sDataCustomer, string customerId)
        {
            try
            {
                if (!string.IsNullOrEmpty(customerId))
                {
                    //JsonObject sDataAddresses = sDataCustomer.GetNamedObject("Addresses");
                    //if (sDataAddresses.ContainsKey("Addresses"))
                    //{
                    //    JsonArray sDataAddressArray = sDataAddresses.GetArray();
                    //    if (sDataAddressArray.Count > 0)
                    //    {
                    //        await SaveAddressDetailsAsync(sDataAddressArray, customerId);
                    //    }
                    //}
                    IJsonValue value;
                    if (sDataCustomer.TryGetValue("Addresses", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            JsonArray sDataAddressArray = sDataCustomer.GetNamedArray("Addresses");
                            if (sDataAddressArray.Count > 0)
                            {
                                await SaveAddressDetailsAsync(sDataAddressArray, customerId);
                            }
                        }
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
        }

        /// <summary>
        /// Save posted address to local dB
        /// </summary>
        /// <param name="sDataAddress"></param>
        /// <param name="customerId"></param>
        /// <param name="addressPending"></param>
        /// <returns></returns>
        public async Task<Address> SavePostedAddressToDbAsync(JsonObject sDataAddress, string customerId, string addressPendingId)
        {
            var addressResponse = new Address();
            addressResponse.CustomerId = customerId;

            IJsonValue value;
            if (sDataAddress.TryGetValue("$key", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    addressResponse.AddressId = sDataAddress.GetNamedString("$key");
                }
            }

            addressResponse = ExtractAddressFromJsonAsync(sDataAddress, addressResponse);
            addressResponse = await AddOrUpdatePendingAddress(addressResponse, addressPendingId);

            return addressResponse;
        }

        /// <summary>
        ///     Adds or updates address json response to local dB
        /// </summary>
        /// <param name="sDataAddress"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Address> AddOrUpdateAddressJsonToDbAsync(JsonObject sDataAddress, string customerId)
        {
            try
            {
                IJsonValue value;
                if (sDataAddress.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Address> addressList;
                        addressList =
                            await
                                _sageSalesDB.QueryAsync<Address>("SELECT * FROM Address where AddressId=?",
                                    sDataAddress.GetNamedString("$key"));

                        if (addressList.FirstOrDefault() != null)
                        {
                            return await UpdateAddressJsonToDbAsync(sDataAddress, addressList.FirstOrDefault());
                        }
                        return await AddAddressJsonToDbAsync(sDataAddress, customerId);
                    }
                }
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }


        /// <summary>
        ///     Gets other addresses data from LocalDB
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<CustomerDetails>> GetOtherAddressesForCustomer(string customerId)
        {
            List<CustomerDetails> otherAddress = null;
            try
            {
                otherAddress =
                    await
                        _sageSalesDB.QueryAsync<CustomerDetails>("select * from Address where CustomerId=?", customerId);
                if (otherAddress != null && otherAddress.Count > 0)
                {
                    otherAddress.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return otherAddress;
        }

        /// <summary>
        ///     Gets mailing address or the first address in the Address table for that customerId
        /// </summary>
        /// Shipping Address is same as Address
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Address> GetShippingAddressForCustomer(string customerId)
        {
            List<Address> shippingAddress = null;
            try
            {
                // Check for valid address if exists then return addresstype 'mailing' or return first object in the table
                shippingAddress =
                    await
                        _sageSalesDB.QueryAsync<Address>(
                            "Select * from Address where CustomerId=? Order by AddressType", customerId);
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
            return shippingAddress.FirstOrDefault();
        }

        /// <summary>
        ///     Gets shipping address details
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public async Task<ShippingAddressDetails> GetShippingAddressDetails(string addressId)
        {
            List<ShippingAddressDetails> shippingAddressList = null;
            try
            {
                shippingAddressList =
                    await
                        _sageSalesDB.QueryAsync<ShippingAddressDetails>(
                            "Select * from Address where Address.AddressId=?", addressId);
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
            return shippingAddressList.FirstOrDefault();
        }

        /// <summary>
        ///     Returns shipping address
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public async Task<Address> GetShippingAddress(string addressId)
        {
            List<Address> shippingAddress = null;
            try
            {
                shippingAddress =
                    await _sageSalesDB.QueryAsync<Address>("Select * from Address where Address.AddressId=?", addressId);
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
            return shippingAddress.FirstOrDefault();
        }

        ///// <summary>
        /////     Returns shipping address for quote
        ///// </summary>
        ///// <param name="quoteId"></param>
        ///// <returns></returns>
        //public async Task<Address> GetShippingAddressForQuote(string quoteId)
        //{
        //    List<Address> shippingAddress = null;
        //    try
        //    {
        //        // Check for valid address if exists then return addresstype 'mailing' or return first object in the table
        //        shippingAddress =
        //            await
        //                _sageSalesDB.QueryAsync<Address>(
        //                    "Select Address.AddressName,Address.AddressId,Address.CustomerId,Address.Street1,Address.Street2,Address.Street3,Address.Street4,Address.City,Address.StateProvince,Address.PostalCode,Address.Country from Address INNER JOIN Quote ON quote.AddressId = Address.AddressId and Quote.QuoteId=?",
        //                    quoteId);
        //        //   ("SELECT distinct customer.customerName, quote.CustomerId, quote.QuoteId, quote.CreatedOn, quote.amount, quote.quoteStatus,quote.QuoteDescription, SalesRep.RepName FROM quote INNER JOIN customer ON customer.customerID = quote.customerId Inner Join SalesRep On Quote.RepId=? And Quote.QuoteStatus!='" + DataAccessUtils.IsOrderQuoteStatus + "' And Quote.QuoteStatus!='" + DataAccessUtils.TemporaryQuoteStatus + "'", salesRepId);
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
        //    return shippingAddress.FirstOrDefault();
        //}

        /// <summary>
        ///     Gets addresses from LocalDB for that customerId
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Address>> GetAddressesForCustomer(string customerId)
        {
            List<Address> customerAddresses = null;
            try
            {
                customerAddresses =
                    await _sageSalesDB.QueryAsync<Address>("Select * from Address where CustomerId=?", customerId);
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
            return customerAddresses;
        }

        /// <summary>
        ///     Gets other address data from local dB for that customerId
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Address>> GetOtherAddresses(string customerId)
        {
            List<Address> customerAddresses = null;
            try
            {
                customerAddresses =
                    await
                        _sageSalesDB.QueryAsync<Address>(
                            "Select * from Address where CustomerId=? and AddressType='Other'", customerId);
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
            return customerAddresses;
        }

        /// Add address to local dB(offline capability)
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public async Task AddAddressToDbAsync(Address address)
        {
            await _sageSalesDB.InsertAsync(address);
        }


        /// Update address to localDb
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public async Task UpdateAddressToDbAsync(Address address)
        {
            await _sageSalesDB.UpdateAsync(address);
        }

        /// <summary>
        ///     Gets shipping address for order
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        public async Task<Address> GetShippingAddressForOrder(string addressId)
        {
            List<Address> shippingAddressList = null;
            try
            {
                shippingAddressList =
                    await _sageSalesDB.QueryAsync<Address>("Select * from Address where Address.AddressId=?", addressId);
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
            return shippingAddressList.FirstOrDefault();
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Compares list of addresses with the Json response and local Db to delete, add or update
        /// </summary>
        /// <param name="sDataAddressArray"></param>
        /// <param name="customerId"></param>
        private async Task SaveAddressDetailsAsync(JsonArray sDataAddressArray, string customerId)
        {
            // Delete if any address exists in dB but not in Json by comparing addressId
            await DeleteAddressesFromDbAsync(sDataAddressArray, customerId);

            foreach (IJsonValue adress in sDataAddressArray)
            {
                JsonObject sDataAddress = adress.GetObject();
                await AddOrUpdateAddressJsonToDbAsync(sDataAddress, customerId);
            }
        }

        /// <summary>
        ///     Adds address json response to dB
        /// </summary>
        /// <param name="sDataAddress"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task<Address> AddAddressJsonToDbAsync(JsonObject sDataAddress, string customerId)
        {
            var addressObj = new Address();
            try
            {
                addressObj.CustomerId = customerId;
                addressObj.AddressId = sDataAddress.GetNamedString("$key");

                addressObj = ExtractAddressFromJsonAsync(sDataAddress, addressObj);
                await _sageSalesDB.InsertAsync(addressObj);
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
            return addressObj;
        }

        /// <summary>
        ///     Updates address json reponse to dB
        /// </summary>
        /// <param name="sDataAddress"></param>
        /// <param name="addressDbObj"></param>
        /// <returns></returns>
        private async Task<Address> UpdateAddressJsonToDbAsync(JsonObject sDataAddress, Address addressDbObj)
        {
            try
            {
                addressDbObj = ExtractAddressFromJsonAsync(sDataAddress, addressDbObj);
                await _sageSalesDB.UpdateAsync(addressDbObj);
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
            return addressDbObj;
        }

        /// <summary>
        /// Add or update pending contact
        /// </summary>
        /// <param name="addressResponse"></param>
        /// <param name="addressPending"></param>
        /// <returns></returns>
        private async Task<Address> AddOrUpdatePendingAddress(Address addressResponse, string addressPendingId)
        {
            try
            {
                List<Address> addressList =
                    await
                        _sageSalesDB.QueryAsync<Address>("Select * from Address where AddressId=? and IsPending='1'",
                            addressPendingId);

                if (addressList.FirstOrDefault() != null)
                {
                    addressResponse.Id = addressList.FirstOrDefault().Id;
                    await _sageSalesDB.UpdateAsync(addressResponse);
                }
                else
                {
                    await _sageSalesDB.InsertAsync(addressResponse);
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

            return addressResponse;
        }

        /// <summary>
        ///     Extracts address json response
        /// </summary>
        /// <param name="sDataAddress"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private Address ExtractAddressFromJsonAsync(JsonObject sDataAddress, Address address)
        {
            try
            {
                IJsonValue value;

                if (sDataAddress.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.AddressName = sDataAddress.GetNamedString("Name");
                    }
                }

                if (sDataAddress.TryGetValue("Type", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.AddressType = sDataAddress.GetNamedString("Type");
                    }
                }

                if (sDataAddress.TryGetValue("Street1", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Street1 = sDataAddress.GetNamedString("Street1");
                    }
                }
                if (sDataAddress.TryGetValue("Street2", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Street2 = sDataAddress.GetNamedString("Street2");
                    }
                }

                if (sDataAddress.TryGetValue("Street3", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Street3 = sDataAddress.GetNamedString("Street3");
                    }
                }
                if (sDataAddress.TryGetValue("Street4", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Street4 = sDataAddress.GetNamedString("Street4");
                    }
                }
                if (sDataAddress.TryGetValue("City", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.City = sDataAddress.GetNamedString("City");
                    }
                }
                if (sDataAddress.TryGetValue("StateProvince", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.StateProvince = sDataAddress.GetNamedString("StateProvince");
                    }
                }
                if (sDataAddress.TryGetValue("PostalCode", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.PostalCode = sDataAddress.GetNamedString("PostalCode");
                    }
                }
                if (sDataAddress.TryGetValue("Country", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Country = sDataAddress.GetNamedString("Country");
                    }
                }
                if (sDataAddress.TryGetValue("Phone", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Phone = sDataAddress.GetNamedString("Phone");
                    }
                }
                if (sDataAddress.TryGetValue("Email", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Email = sDataAddress.GetNamedString("Email");
                    }
                }
                if (sDataAddress.TryGetValue("URL", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        address.Url = sDataAddress.GetNamedString("URL");
                    }
                }
                address.IsPending = false;
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
            return address;
        }


        /// <summary>
        ///     Deletes addresses from dB which exists in dB but not in updated json response
        /// </summary>
        /// <param name="sDataAddressArray"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task DeleteAddressesFromDbAsync(JsonArray sDataAddressArray, string customerId)
        {
            IJsonValue value;
            bool idExists = false;
            List<Address> addressIdJsonList;
            List<Address> addressIdDbList;
            List<Address> addressRemoveList;

            try
            {
                // Retrieve list of addressId from Json
                addressIdJsonList = new List<Address>();
                foreach (IJsonValue adress in sDataAddressArray)
                {
                    JsonObject sDataAddress = adress.GetObject();
                    var adressJsonObj = new Address();
                    if (sDataAddress.TryGetValue("$key", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            adressJsonObj.AddressId = sDataAddress.GetNamedString("$key");
                        }
                    }
                    addressIdJsonList.Add(adressJsonObj);
                }

                //Retrieve list of addressId from dB
                addressIdDbList = new List<Address>();
                addressRemoveList = new List<Address>();
                addressIdDbList =
                    await _sageSalesDB.QueryAsync<Address>("SELECT * FROM Address where customerId=?", customerId);

                for (int i = 0; i < addressIdDbList.Count; i++)
                {
                    idExists = false;
                    for (int j = 0; j < addressIdJsonList.Count; j++)
                    {
                        if (addressIdDbList[i].AddressId.Contains(DataAccessUtils.Pending))
                        {
                            idExists = true;
                            break;
                        }
                        if (addressIdDbList[i].AddressId == addressIdJsonList[j].AddressId)
                        {
                            idExists = true;
                            break;
                        }
                    }
                    if (!idExists)
                        addressRemoveList.Add(addressIdDbList[i]);
                }

                //addressRemoveList = addressIdJsonList.Except(addressIdDbList, new AddressIdComparer()).ToList();
                if (addressRemoveList.Count() > 0)
                {
                    foreach (Address addressRemove in addressRemoveList)
                    {
                        await _sageSalesDB.DeleteAsync(addressRemove);
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
        }

        #endregion
    }
}