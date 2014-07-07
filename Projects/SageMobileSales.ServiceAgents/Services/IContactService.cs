using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IContactService
    {
        Task SyncContacts(string customerId);
        Task PostContact(Contact contact);
        Task SyncOfflineContacts(string customerId);
    }
}