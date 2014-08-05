using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IQuoteLineItemService
    {
        Task SyncQuoteLineItems(Quote quote);
        Task AddQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem);
        Task EditQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem);
        Task DeleteQuoteLineItem(QuoteLineItem quoteLineItem);
        Task SyncOfflineQuoteLineItems();
        Task SyncOfflineDeletedQuoteLineItems();
    }
}