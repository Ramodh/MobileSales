using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.DataAccess.Repositories
{
    public interface IQuoteRepository
    {
        Task SaveQuotesAsync(JsonObject sDataCustomers, LocalSyncDigest localSyncDigest);
        Task SaveQuoteAsync(JsonObject sDataQuote);
        Task<List<QuoteDetails>> GetQuotesListAsync(string salesRepId);
        Task<QuoteDetails> GetQuoteDetailsAsync(string quoteId);
        Task<Quote> GetQuoteAsync(string quoteId);
        Task<Quote> GetQuoteFromPrimaryKey(int id);
        Task<Quote> AddOrUpdateQuoteJsonToDbAsync(JsonObject sDataQuote);
        Task<List<QuoteDetails>> GetQuotesForCustomerAsync(string customerId);
        Task<Quote> AddQuoteToDbAsync(Quote quoteDtls, string selectedQuoteType, string orderId);
        Task<Quote> UpdateQuoteToDbAsync(Quote quote);

        Task<Quote> SavePostedQuoteToDbAsync(JsonObject sDataQuote, Quote quote, Address address,
            QuoteLineItem pendingQuoteLineItem);

        Task DeleteQuoteFromDbAsync(string quoteId);
        Task MarkQuoteAsDeleted(string quoteId);
        Task<List<Quote>> GetPendingQuotes();
        Task<List<Quote>> GetDeletedQuotes();
        Task<List<QuoteShippingAddress>> GetPendingShippingAddress();
    }
}