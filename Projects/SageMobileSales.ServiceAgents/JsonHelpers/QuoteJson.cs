namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class QuoteJson
    {
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal SandH { get; set; }
        public string Status { get; set; }
        public CustomerKeyJson Customer { get; set; }
        public SalesKeyJson SalesRep { get; set; }
        public QuoteDetailsJson Details { get; set; }
    }

    public class QuoteDetailsShippingAddressKeyJson : QuoteJson
    {
        public ShippingAddressKeyJson ShippingAddress { get; set; }
    }

    public class QuoteDetailsShippingAddressJson : QuoteJson
    {
        public ShippingAddressJson ShippingAddress { get; set; }
    }

    public class EditQuoteJson
    {
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal SandH { get; set; }
        public string Status { get; set; }
        public CustomerKeyJson Customer { get; set; }
        public SalesKeyJson SalesRep { get; set; }
        public EditQuoteDetailsJson Details { get; set; }
    }

    public class EditQuoteDetailsShippingAddressKeyJson : EditQuoteJson
    {
        public ShippingAddressKeyJson ShippingAddress { get; set; }
    }
}