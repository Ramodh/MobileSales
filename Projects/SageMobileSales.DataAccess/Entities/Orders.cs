using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Orders")]
    public class Orders
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string OrderId { get; set; }
        public string OrderDescription { get; set; }
        public string TenantId { set; get; }
        public DateTime CreatedOn { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal ShippingAndHandling { get; set; }
        public decimal Tax { get; set; }
        //public DateTime ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        //public string ExternalReferenceNumber { get; set; }
        public decimal DiscountPercent { get; set; }
        public int OrderNumber { get; set; }

        public string QuoteId { get; set; }
        public string RepId { get; set; }
        public string CustomerId { get; set; }
        //ShippingInfo
        public string AddressId { get; set; }
    }
}