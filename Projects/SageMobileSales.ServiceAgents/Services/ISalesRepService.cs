﻿using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface ISalesRepService
    {
        Task SyncSalesRep();
        Task UpdateSalesRep();
    }
}