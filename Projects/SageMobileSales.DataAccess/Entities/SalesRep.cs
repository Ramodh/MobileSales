using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("SalesRep")]
    public class SalesRep
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public string RepId { get; set; }
        public string RepName { get; set; }
        //public string RepOffice { get; set; }
        //public string RepTitle { get; set; }
        //public string Quotes { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public string TenantId { get; set; }
        public string MaximumDiscountPercent { get; set; }
    }
}