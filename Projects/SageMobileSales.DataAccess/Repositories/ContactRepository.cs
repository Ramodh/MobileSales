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
    public class ContactRepository : IContactRepository
    {
        private readonly IDatabase _database;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private string _log = string.Empty;

        public ContactRepository(IDatabase database)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
        }

        #region public methods

        /// <summary>
        ///     Extracts contact data from Json Response
        ///     Compares contacts in localDb with Json response to add, update or delete.
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        public async Task SaveContactsAsync(JsonObject sDataCustomer, string customerId)
        {
            if (!string.IsNullOrEmpty(customerId))
            {
                //JsonObject sDataContacts = sDataCustomer.GetNamedObject("Contacts");
                //if (sDataContacts.ContainsKey("$resources"))
                //{
                //    JsonArray sDataContactArray = sDataContacts.GetNamedArray("$resources");
                //    if (sDataContactArray.Count > 0)
                //    {
                //        await SaveContactDetailsAsync(sDataContactArray, customerId);
                //    }
                //}

                IJsonValue value;
                if (sDataCustomer.TryGetValue("Contacts", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        var sDataContactArray = sDataCustomer.GetNamedArray("Contacts");
                        if (sDataContactArray.Count > 0)
                        {
                            await SaveContactDetailsAsync(sDataContactArray, customerId);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Add contact to localDb(offline capability)
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public async Task AddContactToDbAsync(Contact contact)
        {
            await _sageSalesDB.InsertAsync(contact);
        }


        /// <summary>
        ///     Posted Contact Json response is extracted and updated
        /// </summary>
        /// <param name="sDataContact"></param>
        /// <returns></returns>
        public async Task SavePostedContactJsonToDbAsync(JsonObject sDataContact, string customerId,
            Contact contactPending)
        {
            var contactResponse = new Contact();
            contactResponse.CustomerId = customerId;

            IJsonValue value;
            if (sDataContact.TryGetValue("$key", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    contactResponse.ContactId = sDataContact.GetNamedString("$key");
                }
            }

            contactResponse = ExtractContactFromJsonAsync(sDataContact, contactResponse);

            await AddOrUpdatePendingContact(contactResponse, contactPending);
        }

        /// <summary>
        ///     Gets unsynced contact data from local dB
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Contact>> GetUnsyncedContacts(string customerId)
        {
            List<Contact> unSyncedContacts = null;
            try
            {
                unSyncedContacts =
                    await
                        _sageSalesDB.QueryAsync<Contact>("Select * from Contact where CustomerId=? and IsPending='1'",
                            customerId);
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
            return unSyncedContacts;
        }

        /// <summary>
        ///     Gets list of contacts for that customerId
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Contact>> GetContactDetailsAsync(string customerId)
        {
            List<Contact> contacts = null;
            try
            {
                contacts =
                    await _sageSalesDB.QueryAsync<Contact>("SELECT * FROM Contact where CustomerId=?", customerId);
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
            return contacts;
        }

        /// <summary>
        ///     Add or update contact json response to dB
        /// </summary>
        /// <param name="sDataContact"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Contact> AddOrUpdateContactJsonToDbAsync(JsonObject sDataContact, string customerId)
        {
            try
            {
                IJsonValue value;
                if (sDataContact.TryGetValue("$key", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Contact> contactList;
                        contactList =
                            await
                                _sageSalesDB.QueryAsync<Contact>("SELECT * FROM Contact where contactId=?",
                                    sDataContact.GetNamedString("$key"));

                        if (contactList.FirstOrDefault() != null)
                        {
                            return await UpdateContactJsonToDbAsync(sDataContact, contactList.FirstOrDefault());
                        }
                        return await AddContactJsonToDbAsync(sDataContact, customerId);
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

        #endregion

        #region private methods

        /// <summary>
        ///     Compares contacts in localDb with Json response to delete, add or update
        /// </summary>
        /// <param name="sDataContactArray"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task SaveContactDetailsAsync(JsonArray sDataContactArray, string customerId)
        {
            await DeleteContactsFromDbAsync(sDataContactArray, customerId);

            foreach (var contact in sDataContactArray)
            {
                var sDataContact = contact.GetObject();
                await AddOrUpdateContactJsonToDbAsync(sDataContact, customerId);
            }
        }

        /// <summary>
        ///     Add contact json to dB
        /// </summary>
        /// <param name="sDataContact"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task<Contact> AddContactJsonToDbAsync(JsonObject sDataContact, string customerId)
        {
            var contactObj = new Contact();
            try
            {
                contactObj.CustomerId = customerId;
                contactObj.ContactId = sDataContact.GetNamedString("$key");
                contactObj = ExtractContactFromJsonAsync(sDataContact, contactObj);

                await _sageSalesDB.InsertAsync(contactObj);
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
            return contactObj;
        }

        /// <summary>
        ///     Update contact response to dB
        /// </summary>
        /// <param name="sDataContact"></param>
        /// <param name="contactDbObj"></param>
        /// <returns></returns>
        private async Task<Contact> UpdateContactJsonToDbAsync(JsonObject sDataContact, Contact contactDbObj)
        {
            try
            {
                contactDbObj = ExtractContactFromJsonAsync(sDataContact, contactDbObj);
                await _sageSalesDB.UpdateAsync(contactDbObj);
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
            return contactDbObj;
        }

        /// <summary>
        ///     Extracts contact json response
        /// </summary>
        /// <param name="sDataContact"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        private Contact ExtractContactFromJsonAsync(JsonObject sDataContact, Contact contact)
        {
            try
            {
                IJsonValue value;

                if (sDataContact.TryGetValue("EmailPersonal", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.EmailPersonal = sDataContact.GetNamedString("EmailPersonal");
                    }
                }
                if (sDataContact.TryGetValue("EmailWork", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.EmailWork = sDataContact.GetNamedString("EmailWork");
                    }
                }
                if (sDataContact.TryGetValue("FirstName", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.FirstName = sDataContact.GetNamedString("FirstName");
                    }
                }
                if (sDataContact.TryGetValue("LastName", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.LastName = sDataContact.GetNamedString("LastName");
                    }
                }
                if (sDataContact.TryGetValue("PhoneHome", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.PhoneHome = sDataContact.GetNamedString("PhoneHome");
                    }
                }
                if (sDataContact.TryGetValue("PhoneMobile", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.PhoneMobile = sDataContact.GetNamedString("PhoneMobile");
                    }
                }
                if (sDataContact.TryGetValue("PhoneWork", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.PhoneWork = sDataContact.GetNamedString("PhoneWork");
                    }
                }
                if (sDataContact.TryGetValue("Title", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.Title = sDataContact.GetNamedString("Title");
                    }
                }
                if (sDataContact.TryGetValue("URL", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        contact.Url = sDataContact.GetNamedString("URL");
                    }
                }

                contact.IsPending = false;
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
            return contact;
        }

        /// <summary>
        ///     Deletes contacts from dB which exists in dB but not in updated json response
        /// </summary>
        /// <param name="sDataContactArray"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task DeleteContactsFromDbAsync(JsonArray sDataContactArray, string customerId)
        {
            IJsonValue value;
            List<Contact> contactIdJsonList;
            List<Contact> contactIdDbList;
            List<Contact> contactRemoveList;
            var idExists = false;

            try
            {
                // Retrieve list of contactId from Json
                contactIdJsonList = new List<Contact>();
                foreach (var contact in sDataContactArray)
                {
                    var sDataContact = contact.GetObject();
                    var contactJsonObj = new Contact();
                    if (sDataContact.TryGetValue("$key", out value))
                    {
                        if (value.ValueType.ToString() != DataAccessUtils.Null)
                        {
                            contactJsonObj.ContactId = sDataContact.GetNamedString("$key");
                        }
                    }
                    contactIdJsonList.Add(contactJsonObj);
                }

                //Retrieve list of contactId from dB
                contactIdDbList = new List<Contact>();
                contactRemoveList = new List<Contact>();
                contactIdDbList =
                    await _sageSalesDB.QueryAsync<Contact>("SELECT * FROM Contact where customerId=?", customerId);


                for (var i = 0; i < contactIdDbList.Count; i++)
                {
                    idExists = false;
                    for (var j = 0; j < contactIdJsonList.Count; j++)
                    {
                        if (contactIdDbList[i].ContactId.Contains(DataAccessUtils.Pending))
                        {
                            idExists = true;
                            break;
                        }
                        if (contactIdDbList[i].ContactId == contactIdJsonList[j].ContactId)
                        {
                            idExists = true;
                            break;
                        }
                    }
                    if (!idExists)
                        contactRemoveList.Add(contactIdDbList[i]);
                }

                if (contactRemoveList.Count() > 0)
                {
                    foreach (var contactRemove in contactRemoveList)
                    {
                        await _sageSalesDB.DeleteAsync(contactRemove);
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
        ///     Check for pending contacts and updates
        /// </summary>
        /// <param name="contactResponse"></param>
        /// <param name="contactPending"></param>
        /// <returns></returns>
        private async Task AddOrUpdatePendingContact(Contact contactResponse, Contact contactPending)
        {
            try
            {
                var contactList =
                    await
                        _sageSalesDB.QueryAsync<Contact>("Select * from Contact where ContactId=? and IsPending='1'",
                            contactPending.ContactId);

                if (contactList.FirstOrDefault() != null)
                {
                    contactResponse.Id = contactList.FirstOrDefault().Id;
                    await _sageSalesDB.UpdateAsync(contactResponse);
                }
                else
                {
                    await _sageSalesDB.InsertAsync(contactResponse);
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