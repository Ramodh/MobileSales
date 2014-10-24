using System;

namespace SageMobileSales.DataAccess.Model
{
    public class OrderDetails
    {
        private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();
        private string _orderdescription;

        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingAndHandling { get; set; }
        public decimal DiscountPercent { get; set; }
        public string OrderStatus { get; set; }
        public string RepName { get; set; }

        public string OrderDescription
        {
            get { return _orderdescription.TrimEnd(NewLineChars); }
            set { _orderdescription = value; }
        }

        //public string ExternalReferenceNumber { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string RepId { get; set; }
        public string TenantId { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}