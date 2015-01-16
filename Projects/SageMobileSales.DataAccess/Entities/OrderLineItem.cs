using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("OrderLineItem")]
    public class OrderLineItem
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string OrderLineItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }
    }
}