using SageMobileSales.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.UILogic.Model
{
    class CustomerGroupByAlphabet
    {
        public char GroupName { get; set; }
        public List<CustomerDetails> CustomerAddressList { get; set; }
    }
}
