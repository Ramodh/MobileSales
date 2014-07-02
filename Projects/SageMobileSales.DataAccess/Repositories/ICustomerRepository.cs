using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface ICustomerRepository
    {
        Task SaveCustomersAsync(JsonObject sDataCustomers, LocalSyncDigest localSyncDigest);
        Task SaveCustomerAsync(JsonObject sDataCustomer);
        Task<List<CustomerDetails>> GetCustomerListDtlsAsync();
        Task<Customer> GetCustomerDataAsync(string customerId);
        Task<List<Customer>> GetCustomerList();
        Task<CustomerDetails> GetCustomerDtlsForQuote(string customerId, string addressId);
        Task<List<Customer>> GetCustomerSearchSuggestionsAsync(string searchTerm);
        Task<Customer> AddOrUpdateCustomerJsonToDbAsync(JsonObject sDataCustomer);
        Task<CustomerDetails> GetCustomerDtlsForOrder(OrderDetails order);
    }
}