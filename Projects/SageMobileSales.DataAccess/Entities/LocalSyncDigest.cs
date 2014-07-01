using System;
using SQLite;

namespace SageMobileSales.DataAccess.Entities
{
    [Table("LocalSyncDigest")]
    public class LocalSyncDigest
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }

        public int localTick { get; set; }
        public string TenantId { get; set; }
        public string SDataEntity { get; set; }
        public string LastRecordId { get; set; }
        public DateTime LastSyncTime { get; set; }
    }
}