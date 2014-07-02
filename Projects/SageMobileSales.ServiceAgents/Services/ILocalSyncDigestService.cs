using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SageMobileSales.ServiceAgents.Services
{
   public interface ILocalSyncDigestService
    {
       Task<bool> SyncLocalDigest(string entity, string queryEntity);
       Task<bool> SyncLocalSource(string entity, string queryEntity);
    }
}
