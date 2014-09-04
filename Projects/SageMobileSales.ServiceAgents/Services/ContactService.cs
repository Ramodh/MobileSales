using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.JsonHelpers;

namespace SageMobileSales.ServiceAgents.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IServiceAgent _serviceAgent;
        private Dictionary<string, string> parameters;


        public ContactService(IServiceAgent serviceAgent, IContactRepository contactRepository,
            ICustomerRepository customerRepository)
        {
            _serviceAgent = serviceAgent;
            _contactRepository = contactRepository;
            _customerRepository = customerRepository;
        }

        /// <summary>
        ///     Syn all contacts of a Customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task SyncContacts(string customerId)
        {
            parameters = new Dictionary<string, string>();
            parameters.Add("include", "PeriodToDateSales,Addresses,Contacts");

            string customerEntityId = Constants.CustomerDetailEntity + "('" + customerId + "')";
            HttpResponseMessage contactResponse = null;
            contactResponse =
                await
                    _serviceAgent.BuildAndSendRequest(Constants.TenantId, customerEntityId, null, null,
                        Constants.AccessToken, parameters);
            if (contactResponse != null && contactResponse.IsSuccessStatusCode)
            {
                JsonObject sDataCustomer = await _serviceAgent.ConvertTosDataObject(contactResponse);
                await _customerRepository.SaveCustomerAsync(sDataCustomer);
            }
        }

        /// <summary>
        ///     Post contact via service
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public async Task PostContact(Contact contact)
        {
            //Formatting contact to serialize the object into particular format as required
            ContactJson conatctJsonObject;
            try
            {
                conatctJsonObject = new ContactJson();

                conatctJsonObject.Customer = new CustomerContactKeyJson {key = contact.CustomerId};
                conatctJsonObject.EmailPersonal = contact.EmailPersonal == null ? "" : contact.EmailPersonal;
                conatctJsonObject.EmailWork = contact.EmailWork == null ? "" : contact.EmailWork;
                conatctJsonObject.FirstName = contact.FirstName;
                conatctJsonObject.LastName = contact.LastName;
                conatctJsonObject.PhoneHome = contact.PhoneHome == null ? "" : contact.PhoneHome;
                conatctJsonObject.PhoneMobile = contact.PhoneMobile == null ? "" : contact.PhoneMobile;
                conatctJsonObject.PhoneWork = contact.PhoneWork == null ? "" : contact.PhoneWork;
                conatctJsonObject.Title = contact.Title == null ? "" : contact.Title;
                //conatctJsonObject.URL = contact.Url == null ? "" : contact.Url;

                HttpResponseMessage contactResponse = null;
                contactResponse =
                    await
                        _serviceAgent.BuildAndPostObjectRequest(Constants.TenantId, Constants.ContactEntity, null,
                            Constants.AccessToken,
                            null, conatctJsonObject);
                if (contactResponse != null && contactResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataContact = await _serviceAgent.ConvertTosDataObject(contactResponse);

                    //JsonObject customerObj = sDataContact.GetNamedObject("Customer");
                    //Customer customer =
                    //    await _customerRepository.GetCustomerDataAsync(customerObj.GetNamedString("$key"));
                    //if (customer != null)
                    await _contactRepository.SavePostedContactJsonToDbAsync(sDataContact, contact.CustomerId, contact);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Syncs offline contacts
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task SyncOfflineContacts(string customerId)
        {
            List<Contact> unSyncedContacts = await _contactRepository.GetUnsyncedContacts(customerId);
            if (unSyncedContacts.Count > 0)
            {
                foreach (Contact contact in unSyncedContacts)
                {
                    await PostContact(contact);
                }
            }
        }
    }
}