using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
