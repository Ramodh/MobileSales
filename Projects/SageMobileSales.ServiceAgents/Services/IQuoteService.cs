using System.Threading.Tasks;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IQuoteService
    {
        Task StartQuoteSyncProcess();
        Task<Quote> PostDraftQuote(Quote quote);
        //Task<Quote> PatchDraftQuote(Quote quote);
        Task<Quote> SubmitQuote(Quote quote);
        Task DeleteQuote(string quoteId);
        Task UpdateQuoteShippingAddressKey(Quote quote);
        Task<Address> UpdateQuoteShippingAddress(Quote quote, string addressId);
        //Task<Quote> PostQuoteShippingAddress(Quote quote, Address address);
        Task UpdateDiscountOrShippingAndHandling(Quote quote);
        //Task CalculateSalesTaxForQuote(Quote quote);
        Task RevertSubmittedQuoteToDraft(Quote quote);
        Task SyncOfflineQuotes();
        Task SyncOfflineDeletedQuotes();
        Task SyncOfflineShippingAddress();
    }
}