using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.DataAccess.Model
{
    public class RecentOrders
    {
        public DateTime Date
        {
            get;
            set;
        }
        public string Invoice
        {
            get;
            set;
        }
        public int Quantity
        {
            get;
            set;
        }
        public decimal UnitPrice
        {
            get;
            set;
        }
        public decimal Total
        {
            get;
            set;
        }
    }
}
