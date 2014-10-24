using System.Collections.Generic;

namespace SageMobileSales.DataAccess.Common
{
    public class EntityValidationResult
    {
        public EntityValidationResult()
        {
            ModelState = new Dictionary<string, List<string>>();
        }

        public Dictionary<string, List<string>> ModelState { get; private set; }
    }
}