namespace SageMobileSales.DataAccess.Model
{
    public class ShippingAddressDetails
    {
        public string CustomerId { get; set; }
        public string AddressName { get; set; }
        public string AddressId { get; set; }
        public string AddressType { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string Street4 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public bool IsPending { get; set; }
    }
}