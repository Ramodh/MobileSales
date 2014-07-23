using System;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IOAuthService
    {
        Task<String> Authorize();
        Task Cleanup();
    }
}