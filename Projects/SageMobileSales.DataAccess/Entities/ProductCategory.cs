using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("ProductCategory")]
    public class ProductCategory
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ParentId { get; set; }
        public string TenantId { get; set; }
        public string ProductLinks { get; set; }
    }
}