namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class ShippingAddressJson
    {
        public string Name { get; set; }
        public string PostalCode { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        //public string URL { get; set; }
        public string Country { get; set; }
        public string Street4 { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string Street1 { get; set; }
        public string Phone { get; set; }
        public string Street3 { get; set; }
        public string StateProvince { get; set; }
        public CustomerId Customer { get; set; }
    }

    public class CustomerId
    {
        public string Id { get; set; }
    }
}