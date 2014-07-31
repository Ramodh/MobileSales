using System.Collections.Generic;

namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class QuoteDetailsJson
    {
        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        //public InventoryItemKeyJson InventoryItem { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string InventoryItemKeyId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class EditQuoteDetailsJson
    {
        public List<ResourceKey> resources { get; set; }
    }

    public class ResourceKey
    {
        public string key { set; get; }
        public InventoryItemKeyJson InventoryItem { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }
}