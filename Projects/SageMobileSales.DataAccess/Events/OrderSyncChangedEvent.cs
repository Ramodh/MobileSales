using Microsoft.Practices.Prism.PubSubEvents;
namespace SageMobileSales.DataAccess.Events
{
    public class OrderSyncChangedEvent : PubSubEvent<bool>
    {
    }
}