using System.IO;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("ProductAssociatedBlob")]
    public class ProductAssociatedBlob
    {
        private string _name;

        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string ProductAssociatedBlobId { get; set; }
        public string TenantId { get; set; }
        public string ProductId { get; set; }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                {
                    return Path.GetFileNameWithoutExtension(_name);
                }
                return _name;
            }

            set { _name = value; }
        }

        public string Url { get; set; }
        public string productAssociatedBlobDescription { get; set; }
        public string ThumbnailUrl { get; set; }
        public string MimeType { get; set; }
        public string ThumbnailMimeType { get; set; }
        public bool IsPrimary { get; set; }
    }
}