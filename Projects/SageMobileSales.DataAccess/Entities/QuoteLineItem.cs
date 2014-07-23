using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("QuoteLineItem")]
    public class QuoteLineItem
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { set; get; }

        public string QuoteLineItemId { set; get; }
        public decimal Price { set; get; }
        public int Quantity { set; get; }
        public string tenantId { set; get; }

        public string QuoteId { set; get; }
        public string ProductId { set; get; }

        public bool IsPending { get; set; }
        public bool IsDeleted { get; set; }
    }
}