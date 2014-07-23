using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("Tenant")]
    public class Tenant
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }
        public string RepId { get; set; }
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string County { get; set; }
        public string PostalCode { get; set; }
    }
}