using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
   public interface ICustomerService
    {
        Task StartCustomersSyncProcess();
    }
}
