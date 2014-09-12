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
        Task<Address> AddOrUpdateAddressJsonToDbAsync(JsonObject sDataAddress, string customerId);
        Task AddAddressToDbAsync(Address address);
        Task UpdateAddressToDbAsync(Address address);

        Task<Address> SavePostedAddressToDbAsync(JsonObject sDataAddress, string customerId, string addressPendingId);
        Task<Address> GetShippingAddressForCustomer(string customerId);
        Task<Address> GetShippingAddress(string addressId);
        Task<ShippingAddressDetails> GetShippingAddressDetails(string addressId);
        Task<List<Address>> GetAddressesForCustomer(string customerId);
        Task<List<Address>> GetOtherAddresses(string customerId);
        //Task<Address> GetShippingAddressForQuote(string customerId);
        //Task<List<CustomerDetails>> GetOtherAddressesForCustomer(string customerId);
    }
}