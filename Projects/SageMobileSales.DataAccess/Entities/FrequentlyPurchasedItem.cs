using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("FrequentlyPurchasedItem")]
    public class FrequentlyPurchasedItem
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int Id { get; set; }

        public string FrequentlyPurchasedItemId { get; set; }
        public string CustomerId { get; set; }

        public int NumberOfInvoices { get; set; }
        public string ItemId { get; set; }
        public int QuantityYtd { get; set; }
        public int QuantityPriorYtd { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public string EntityStatus { get; set; }
        public bool IsDeleted { get; set; }
    }
}