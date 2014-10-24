using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("ProductCategoryLink")]
    public class ProductCategoryLink
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string CategoryId { get; set; }
        public string ProductId { get; set; }
    }
}