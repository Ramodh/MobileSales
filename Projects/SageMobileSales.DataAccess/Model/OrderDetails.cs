using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.DataAccess.Model
{
    public class OrderDetails
    {
        private string _orderdescription;
        private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

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
        public string ExternalReferenceNumber { get; set; }
        public int OrderNumber { get; set; }
        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string RepId { get; set; }
        public string TenantId { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
