using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Product")]
    public class Product
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public Decimal CostStd { get; set; }
        public string NormalizedKeywords { get; set; }
        public string PhotoUrl { get; set; }
        public Decimal PriceStd { get; set; }
        public string ProductDescription { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string SectionKey { get; set; }
        public string Sku { get; set; }
        public string TenantId { get; set; }
        public string ThumbnailUrl { get; set; }
        public string UnitOfMeasure { get; set; }
        //public int IsActive { get; set; }
        public string EntityStatus { get; set; }
    }
}