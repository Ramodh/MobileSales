using System;

namespace SageMobileSales.DataAccess.Model
{
    public class QuoteDetails
    {
        private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();
        private DateTime _expiredOn;
        private string _quotedescription;

        public int Id { get; set; }
        public string QuoteId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedOn { get; set; }

        public string ExpiredOn
        {
            get
            {
                if (_expiredOn != DateTime.MinValue)
                {
                    return _expiredOn.ToString("MM/dd/yyyy");
                }
                return string.Empty;
            }
            set { _expiredOn = Convert.ToDateTime(value); }
        }

        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingAndHandling { get; set; }
        public decimal DiscountPercent { get; set; }
        public string QuoteStatus { get; set; }
        public string RepName { get; set; }

        public string QuoteDescription
        {
            get { return _quotedescription != null ? _quotedescription.TrimEnd(NewLineChars) : _quotedescription; }
            set { _quotedescription = value; }
        }

        public string CustomerId { get; set; }
        public string AddressId { get; set; }
        public string RepId { get; set; }
        public string TenantId { get; set; }
        public string QuoteNumber { get; set; }
    }
}