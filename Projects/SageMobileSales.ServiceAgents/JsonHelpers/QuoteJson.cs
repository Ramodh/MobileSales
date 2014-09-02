using System.Collections.Generic;

namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class QuoteJson
    {
        public string Description { get; set; }
        //public decimal DiscountPercent { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal SandH { get; set; }
        //public string Status { get; set; }
        //public CustomerKeyJson Customer { get; set; }
        public string CustomerId { get; set; }
        //public SalesKeyJson SalesRep { get; set; }
        //public QuoteDetailsJson Details { get; set; }
        public List<Detail> Details { get; set; }
        //public string Id { get; set; }
        //public int QuoteNumber { get; set; }
        public decimal Tax { get; set; }
        public decimal SubTotal { get; set; }
        //public DateTime ExpiryDate { get; set; }
        //public DateTime SubmittedDate{get; set;}
    }

    public class QuoteDetailsShippingAddressKeyJson : QuoteJson
    {
        //public ShippingAddressKeyJson ShippingAddress { get; set; }
        public string ShippingAddressId { get; set; }
    }

    public class QuoteDetailsShippingAddressJson : QuoteJson
    {
        public ShippingAddressJson ShippingAddress { get; set; }
    }

    public class EditQuoteJson
    {
        //public string Description { get; set; }
        //public decimal DiscountPercent { get; set; }
        //public decimal QuoteTotal { get; set; }
        //public decimal SandH { get; set; }
        //public string Status { get; set; }
        //public CustomerKeyJson Customer { get; set; }
        //public SalesKeyJson SalesRep { get; set; }
        //public EditQuoteDetailsJson Details { get; set; }

        public string Description { get; set; }
        //public decimal DiscountPercent { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal SandH { get; set; }
        //public string Status { get; set; }
        //public CustomerKeyJson Customer { get; set; }
        public string CustomerId { get; set; }
        //public SalesKeyJson SalesRep { get; set; }
        //public QuoteDetailsJson Details { get; set; }
        public List<EditDetail> Details { get; set; }

        //public int QuoteNumber { get; set; }
        public decimal Tax { get; set; }
        public decimal SubTotal { get; set; }
        //public DateTime ExpiryDate { get; set; }
        //public DateTime SubmittedDate{get; set;}
    }

    public class EditQuoteDetailsShippingAddressKeyJson : EditQuoteJson
    {
        //public ShippingAddressKeyJson ShippingAddress { get; set; }
        public string ShippingAddressId { get; set; }
    }

    public class SubmitQuoteJson : QuoteJson
    {
        public string ShippingAddressId { get; set; }
        public string key { get; set; }
    }
}