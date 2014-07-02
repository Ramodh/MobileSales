using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IContactService
    {
        Task SyncContacts(string customerId);
        Task PostContact(Contact contact);
        Task SyncOfflineContacts(string customerId);
    }
}
