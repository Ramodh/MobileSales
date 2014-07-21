using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IContactRepository
    {
        Task SaveContactsAsync(JsonObject sDataCustomer, string customerId);
        Task<List<Contact>> GetContactDetailsAsync(string customerId);
        Task SavePostedContactJSonToDbAsync(JsonObject sDataContact, string customerId, Contact contact);
        Task AddContactToDbAsync(Contact contact);
        Task<List<Contact>> GetUnsyncedContacts(string customerId);
    }
}