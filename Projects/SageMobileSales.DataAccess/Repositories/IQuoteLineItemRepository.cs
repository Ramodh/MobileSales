using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IQuoteLineItemRepository
    {
        Task SaveQuoteLineItemsAsync(JsonObject SDataQuote, string quoteId);
        Task<List<LineItemDetails>> GetQuoteLineItemDetailsAsync(string quoteId);
        Task AddQuoteLineItemToDbAsync(QuoteLineItem quoteLineItem);
        Task<List<QuoteLineItem>> GetQuoteLineItemsForQuote(string quoteId);
        Task<QuoteLineItem> GetQuoteLineAsync(string quoteLineItemId);
        Task<QuoteLineItem> GetQuoteLineItemIfExistsForQuote(string quoteId, string productId);
        Task UpdateQuoteLineItemToDbAsync(QuoteLineItem quoteLineItem);
        Task MarkQuoteLineItemAsDeleted(string quoteLineItemId);
        Task MarkQuoteLineItemsAsDeleted(string quoteId);
        Task DeleteQuoteLineItemFromDbAsync(string quoteLineItemId);
        Task DeleteQuoteLineItemsFromDbAsync(string quoteId);
        Task<List<QuoteLineItem>> GetPendingQuoteLineItems();
        Task<List<QuoteLineItem>> GetDeletedQuoteLineItems();
        Task SavePostedQuoteLineItemsAsync(JsonObject sDataQuote, string quoteId, QuoteLineItem pendingQuoteLineItem);
    }
}