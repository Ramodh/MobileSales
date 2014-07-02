using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.JsonHelpers
{   
    public class QuoteDetailsJson
    {
        public List<Resource> resources { get; set; }
    }    

    public class Resource
    {
        public InventoryItemKeyJson InventoryItem { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
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
