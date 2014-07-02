using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.DataAccess.Model
{
   public class CustomerDetails
    {
       public string CustomerId { get; set; }
       public string CustomerName { get; set; }
       public string AddressName { get; set; }
       public string Street1 { get; set; }
       public string City { get; set; }
       public string StateProvince { get; set; }
       public string PostalCode { get; set; }
       public string Phone { get; set; }
       public Decimal CreditAvailable { get; set; }
       public Decimal CreditLimit { get; set; }
       public string PaymentTerms { get; set; }      
    }
}
