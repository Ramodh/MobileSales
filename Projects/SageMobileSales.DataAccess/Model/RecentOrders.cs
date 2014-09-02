using System;

namespace SageMobileSales.DataAccess.Model
{
    public class RecentOrders
    {
        public DateTime Date { get; set; }
        public string Invoice { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}