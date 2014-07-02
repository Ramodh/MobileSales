using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.JsonHelpers
{
    public class ContactJson
    {
        public CustomerKeyJson Customer { get; set; }
        public string EmailWork { get; set; }
        public string EmailPersonal { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneWork { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
    }    
}
