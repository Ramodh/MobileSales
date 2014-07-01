using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents.Services
{
    public interface IQuoteService
    {
        Task StartQuoteSyncProcess();
        Task<Quote> PostQuote(Quote quote);
        Task<Quote> SubmitQuote(Quote quote);
        Task DeleteQuote(string quoteId);
        Task UpdateQuoteShippingAddressKey(Quote quote);
        Task UpdateQuoteShippingAddress(Quote quote, Address address);
        //Task<Quote> PostQuoteShippingAddress(Quote quote, Address address);
        Task UpdateDiscountOrShippingAndHandling(Quote quote);
        Task CalculateSalesTaxForQuote(Quote quote);
        Task RevertSubmittedQuoteToDraft(Quote quote);
        Task SyncOfflineQuotes();
        Task SyncOfflineDeletedQuotes();
        Task SyncOfflineShippingAddress();
    }
}
