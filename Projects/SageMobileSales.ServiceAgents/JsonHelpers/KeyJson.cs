namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class CustomerKeyJson
    {
        public string CustomerId { get; set; }
    }

    public class CustomerContactKeyJson
    {
        public string key { get; set; }
    }

    public class SalesKeyJson
    {
        public string key { get; set; }
    }

    public class ShippingAddressKeyJson
    {
        public string ShippingAddressId { get; set; }
    }

    public class InventoryItemKeyJson
    {
        public string InventoryItemKeyId { get; set; }
    }
}