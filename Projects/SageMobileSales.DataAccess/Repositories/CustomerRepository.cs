using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IDatabase _database;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public CustomerRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository,
            IAddressRepository addressRepository, IContactRepository contactRepository, IEventAggregator eventAggregator)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _addressRepository = addressRepository;
            _contactRepository = contactRepository;
            _eventAggregator = eventAggregator;
        }

        # region Public Methods

        /// <summary>
        ///     Extracts customer data from Json repsonse and updates LocalSyncDigest(local tick) in LocalDB
        /// </summary>
        /// <param name="sDataProducts"></param>
        /// <param name="localSyncDigest"></param>
        /// s
        /// <returns></returns>
        public async Task SaveCustomersAsync(JsonObject sDataCustomers, LocalSyncDigest localSyncDigest)
        {
            try
            {
                JsonArray sDataCustomersArray = sDataCustomers.GetNamedArray("$resources");
                DataAccessUtils.CustomerReturnedCount += sDataCustomersArray.Count;

                //List<Customer> lstCustomers = new List<Customer>();
                for (int customer = 0; customer < sDataCustomersArray.Count; customer++)
                {
                    JsonObject sDataCustomer = sDataCustomersArray[customer].GetObject();

                    // Extracting CustomerJson and adding it into Customer Table and passing the same to Address Table to store addresses
                    await
                        _addressRepository.SaveAddressesAsync(sDataCustomer,
                            (await SaveCustomerDetailsAsync(sDataCustomer)).CustomerId);

                    if (localSyncDigest != null)
                    {
                        if ((Convert.ToInt32(sDataCustomer.GetNamedNumber("SyncTick")) >
                             localSyncDigest.localTick))
                            localSyncDigest.localTick = Convert.ToInt32(sDataCustomer.GetNamedNumber("SyncTick"));
                    }

                    if (customer == (sDataCustomersArray.Count - 1) && localSyncDigest != null)
                    {
                        //Fires an event to update UI/publish
                        _eventAggregator.GetEvent<CustomerDataChangedEvent>().Publish(true);

                        if (DataAccessUtils.CustomerTotalCount == DataAccessUtils.CustomerReturnedCount)
                        {
                            if (localSyncDigest == null)
                                localSyncDigest = new LocalSyncDigest();
                            localSyncDigest.localTick++;
                            localSyncDigest.LastRecordId = null;
                            localSyncDigest.LastSyncTime = DateTime.Now;
                            DataAccessUtils.IsCustomerSyncCompleted = true;
                        }
                        await _localSyncDigestRepository.UpdateLocalSyncDigestDtlsAsync(localSyncDigest);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Extracts data from response and updates customer, addresses and contacts
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        public async Task SaveCustomerAsync(JsonObject sDataCustomer)
        {
            Customer customer = await SaveCustomerDetailsAsync(sDataCustomer);
            await _addressRepository.SaveAddressesAsync(sDataCustomer, customer.CustomerId);

            await _contactRepository.SaveContactsAsync(sDataCustomer, customer.CustomerId);
        }

        /// <summary>
        ///     Adds or updates customer json response to dB
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<Customer> AddOrUpdateCustomerJsonToDbAsync(JsonObject sDataCustomer)
        {
            try
            {
                IJsonValue value;
                if (sDataCustomer.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Customer> customerList;
                        customerList =
                            await
                                _sageSalesDB.QueryAsync<Customer>("SELECT * FROM Customer where CustomerId=?",
                                    sDataCustomer.GetNamedString("Id"));

                        if (customerList.FirstOrDefault() != null)
                        {
                            return await UpdateCustomerJsonToDbAsync(sDataCustomer, customerList.FirstOrDefault());
                        }
                        return await AddCustomerJsonToDbAsync(sDataCustomer);
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
        ///     Returns list of all customers
        /// </summary>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerList()
        {
            List<Customer> customer = null;
            try
            {
                //Need to implement "where IsActive='1'" on completion of Entity Status handling
                customer = await _sageSalesDB.QueryAsync<Customer>("SELECT * FROM Customer order by customerName asc");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customer;
        }


        /// <summary>
        ///     Gets list of customers from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerDetails>> GetCustomerListDtlsAsync()
        {
            List<CustomerDetails> customerAddressList = null;
            try
            {
                customerAddressList =
                    await
                        _sageSalesDB.QueryAsync<CustomerDetails>(
                        "SELECT distinct customer.CustomerId, customer.CustomerName, customer.CreditAvailable, customer.CreditLimit, customer.PaymentTerms, address.AddressName, address.Street1, address.City, address.StateProvince, address.PostalCode, address.Phone FROM Customer  as customer Join Address  as address on customer.CustomerId = address.customerId and addresstype='Mailing' and Customer.EntityStatus= 'Active'");

                //"SELECT * FROM Customer where Customer.EntityStatus= 'Active' order by customerName asc "
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customerAddressList;
        }

        /// <summary>
        ///     Gets Customer based on search text
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerSearchSuggestionsAsync(string searchTerm)
        {
            try
            {
                // Retrieve the search suggestions from LocalDB
                List<Customer> searchSuggestions =
                    await
                        _sageSalesDB.QueryAsync<Customer>("SELECT * from customer where CustomerName like '%" +
                                                          searchTerm + "%'");
                return searchSuggestions;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        ///     Get Customer list for that quote
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<CustomerDetails> GetCustomerDtlsForQuote(string customerId, string addressId)
        {
            List<CustomerDetails> customerAddressList = null;
            try
            {
                customerAddressList =
                    await
                        _sageSalesDB.QueryAsync<CustomerDetails>(
                            "SELECT distinct customer.CustomerId, customer.CustomerName, customer.CreditAvailable, customer.CreditLimit, customer.PaymentTerms, address.AddressName, address.Street1, address.City, address.StateProvince, address.PostalCode, address.Phone FROM Customer  as customer Join Address  as address on customer.CustomerId = ? and AddressId=?",
                            customerId, addressId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customerAddressList.FirstOrDefault();
        }

        /// <summary>
        ///     Get Customer list for that order
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<CustomerDetails> GetCustomerDtlsForOrder(OrderDetails order)
        {
            List<CustomerDetails> customerDetailsList = null;
            try
            {
                customerDetailsList =
                    await
                        _sageSalesDB.QueryAsync<CustomerDetails>(
                            "SELECT distinct customer.CustomerId, customer.CustomerName, customer.CreditAvailable, customer.CreditLimit, customer.PaymentTerms, address.AddressName, address.Street1, address.City, address.StateProvince, address.PostalCode, address.Phone FROM Customer  as customer Join Address  as address on customer.CustomerId = ? and AddressId=?",
                            order.CustomerId, order.AddressId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customerDetailsList.FirstOrDefault();
        }

        /// <summary>
        ///     Get Customer data from LocalDB
        /// </summary>
        /// <returns></returns>
        public async Task<Customer> GetCustomerDataAsync(string customerId)
        {
            List<Customer> customer = null;
            try
            {
                customer =
                    await _sageSalesDB.QueryAsync<Customer>("SELECT * FROM Customer where CustomerId=?", customerId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customer.FirstOrDefault();
        }

        /// <summary>
        ///     Extracts customer from Json and updates the same
        /// </summary>
        /// Can implement delete functionality for customers here
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Customer> SaveCustomerDetailsAsync(JsonObject sDataCustomer)
        {
            try
            {
                return await AddOrUpdateCustomerJsonToDbAsync(sDataCustomer);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        # endregion

        # region Private Methods

        /// <summary>
        ///     Add customer json response to dB
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Customer> AddCustomerJsonToDbAsync(JsonObject sDataCustomer)
        {
            var customerObj = new Customer();
            try
            {
                customerObj.CustomerId = sDataCustomer.GetNamedString("Id");
                customerObj = ExtractCustomerFromJsonAsync(sDataCustomer, customerObj);

                await _sageSalesDB.InsertAsync(customerObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customerObj;
        }


        /// <summary>
        ///     Update customer json response to dB
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <param name="customerDbObj"></param>
        /// <returns></returns>
        private async Task<Customer> UpdateCustomerJsonToDbAsync(JsonObject sDataCustomer, Customer customerDbObj)
        {
            try
            {
                customerDbObj = ExtractCustomerFromJsonAsync(sDataCustomer, customerDbObj);
                await _sageSalesDB.UpdateAsync(customerDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customerDbObj;
        }

        /// <summary>
        ///     Extract customer json response
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private Customer ExtractCustomerFromJsonAsync(JsonObject sDataCustomer, Customer customer)
        {
            try
            {
                IJsonValue value;
                if (sDataCustomer.TryGetValue("Name", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.CustomerName = sDataCustomer.GetNamedString("Name");
                    }
                }

                //if (sDataCustomer.TryGetValue("PaymentMethod", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        customer.PaymentMethod = sDataCustomer.GetNamedString("PaymentMethod");
                //    }
                //}

                if (sDataCustomer.TryGetValue("IsOnCreditHold", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.IsOnCreditHold = sDataCustomer.GetNamedBoolean("IsOnCreditHold");
                    }
                }

                if (sDataCustomer.TryGetValue("CreditLimit", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.CreditLimit = Convert.ToDecimal(sDataCustomer.GetNamedNumber("CreditLimit"));
                    }
                }

                if (sDataCustomer.TryGetValue("IsCreditLimitUsed", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.IsCreditLimitUsed = sDataCustomer.GetNamedBoolean("IsCreditLimitUsed");
                    }
                }

                if (sDataCustomer.TryGetValue("CreditAvailable", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.CreditAvailable = Convert.ToDecimal(sDataCustomer.GetNamedNumber("CreditAvailable"));
                    }
                }

                if (sDataCustomer.TryGetValue("PaymentTerms", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.PaymentTerms = sDataCustomer.GetNamedString("PaymentTerms");
                    }
                }

                if (sDataCustomer.TryGetValue("EntityStatus", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        customer.EntityStatus = sDataCustomer.GetNamedString("EntityStatus");
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return customer;
        }

        #endregion
    }
}