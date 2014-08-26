using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IAddressRepository
    {
        Task SaveAddressesAsync(JsonObject sDataCustomer, string customerId);
        Task<List<CustomerDetails>> GetOtherAddressesForCustomer(string customerId);
        Task<Address> AddOrUpdateAddressJsonToDbAsync(JsonObject sDataAddress, string customerId);
        Task<Address> GetShippingAddressForCustomer(string customerId);
        Task<Address> GetQuoteShippingAddress(string addressId);
        Task<ShippingAddressDetails> GetShippingAddress(string addressId);
        Task AddAddressToDbAsync(Address address);
        Task UpdateAddressToDbAsync(Address address);
        Task<List<Address>> GetAddressesForCustomer(string customerId);
        Task<List<Address>> GetOtherAddressesForCustomers(string customerId, bool isCameFrom);
        Task<Address> GetShippingAddressForQuote(string customerId);
    }
}