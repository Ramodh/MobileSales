using Microsoft.Practices.Prism.PubSubEvents;

namespace SageMobileSales.DataAccess.Events
{
    public class ProductSyncChangedEvent : PubSubEvent<bool>
    {
    }
}