using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("SalesHistory")]
    public class SalesHistory
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }

        public string SalesHistoryId { get; set; }
        public string CustomerId { get; set; }
        public string InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string UnitOfMeasure { get; set; }
        public bool IsDeleted { get; set; }
    }
}