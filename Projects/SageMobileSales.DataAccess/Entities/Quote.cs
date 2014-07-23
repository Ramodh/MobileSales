using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Quote")]
    public class Quote
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string QuoteId { get; set; }
        public string QuoteDescription { get; set; }
        public string TenantId { set; get; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string QuoteStatus { get; set; }
        public decimal ShippingAndHandling { get; set; }
        public decimal Tax { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public string ExternalReferenceNumber { get; set; }
        public decimal DiscountPercent { get; set; }
        public int QuoteNumber { get; set; }
        public bool IsPending { get; set; }
        public bool IsDeleted { get; set; }
        public decimal SubTotal { get; set; }

        //public string QuoteType { get; set; }
        public string ToOrderAmountPaid { get; set; }
        public string ToOrderAuthorizationCode { get; set; }
        public string ToOrderCreditCardExpMonth { get; set; }
        public string ToOrderCreditCardExpYear { get; set; }
        public string ToOrderCreditCardLast4 { get; set; }
        public string ToOrderDate { get; set; }
        public string ToOrderPaymentType { get; set; }
        public string ToOrderReference { get; set; }
        public string ToOrderStatus { get; set; }
        //public int PendingPostResponse { get; set; }        
        //public DateTime ApprovalDate { set; get; }
        //public string CommentForRep { set; get; }        
        //public int Deleted { set; get; }                
        //public int Pending { set; get; }


        public string RepId { get; set; }
        public string CustomerId { get; set; }
        //ShippingInfo
        public string AddressId { get; set; }
        //public string QuoteLineItemId { get; set; }

        public string EntityStatus { get; set; }
    }
}