using System.Collections.Generic;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.UILogic.Model
{
    internal class CustomerGroupByAlphabet
    {
        public char GroupName { get; set; }
        public List<CustomerDetails> CustomerAddressList { get; set; }
    }
}