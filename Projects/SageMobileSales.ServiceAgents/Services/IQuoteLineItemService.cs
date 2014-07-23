using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IQuoteLineItemService
    {
        Task SyncQuoteLineItems(string quoteId);
        Task AddQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem);
        Task EditQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem);
        Task DeleteQuoteLineItem(string quoteLineItemId);
        Task SyncOfflineQuoteLineItems();
        Task SyncOfflineDeletedQuoteLineItems();
    }
}