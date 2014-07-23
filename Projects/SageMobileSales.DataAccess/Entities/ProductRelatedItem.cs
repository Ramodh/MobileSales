using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("ProductRelatedItem")]
    public class ProductRelatedItem
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string ProductId { get; set; }
        public string RelatedItemId { get; set; }
    }
}