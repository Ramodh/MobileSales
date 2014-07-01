using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.DataAccess.Model
{
    public class ProductDetails
    {
        //Product Associated Blobs        
        public string ProductAssociatedBlobId { get; set; }        
        public string Name { get; set; }
        public string Url { get; set; }
        public string productAssociatedBlobDescription { get; set; }        
        public string MimeType { get; set; }
        public string ThumbnailMimeType { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsPrimary { get; set; }
        //Products        
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
        public string UnitOfMeasure { get; set; }
        public int IsActive { get; set; }
    }
}
