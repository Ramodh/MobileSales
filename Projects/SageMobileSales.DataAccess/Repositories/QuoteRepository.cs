using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SQLite;

namespace SageMobileSales.DataAccess.Repositories
{
    public class QuoteRepository : IQuoteRepository
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDatabase _database;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly SQLiteAsyncConnection _sageSalesDB;
        private readonly ISalesRepRepository _salesRepRepository;
        private string _log = string.Empty;

        public QuoteRepository(IDatabase database, ILocalSyncDigestRepository localSyncDigestRepository,
            ICustomerRepository customerRepository,
            IAddressRepository addressRepository, IQuoteLineItemRepository quoteLineItemRepository,
            ISalesRepRepository salesRepRepository,
            IOrderLineItemRepository orderLineItemRepository, IEventAggregator eventAggregator)
        {
            _database = database;
            _sageSalesDB = _database.GetAsyncConnection();
            _localSyncDigestRepository = localSyncDigestRepository;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
            _quoteLineItemRepository = quoteLineItemRepository;
            _orderLineItemRepository = orderLineItemRepository;
            _salesRepRepository = salesRepRepository;
            _eventAggregator = eventAggregator;
        }

        #region public methods

        /// <summary>
        ///     Extracts quote data from Json repsonse and updates LocalSyncDigest(local tick) in LocalDB
        /// </summary>
        /// <param name="sDataQuotes"></param>
        /// <param name="localSyncDigest"></param>
        /// <returns></returns>
        public async Task SaveQuotesAsync(JsonObject sDataQuotes, LocalSyncDigest localSyncDigest)
        {
            JsonArray sDataQuotesArray = sDataQuotes.GetNamedArray("$resources");
            DataAccessUtils.QuotesReturnedCount += sDataQuotesArray.Count;

            for (int quote = 0; quote < sDataQuotesArray.Count; quote++)
            {
                JsonObject sDataQuote = sDataQuotesArray[quote].GetObject();

                Quote savedQuote = await SaveQuoteDetailsAsync(sDataQuote);
                if (savedQuote != null)
                    await _quoteLineItemRepository.SaveQuoteLineItemsAsync(sDataQuote, savedQuote.QuoteId);

                if (localSyncDigest != null)
                {
                    if ((Convert.ToInt32(sDataQuote.GetNamedNumber("SyncTick")) > localSyncDigest.localTick))
                        localSyncDigest.localTick = Convert.ToInt32(sDataQuote.GetNamedNumber("SyncTick"));
                }

                if (quote == (sDataQuotesArray.Count - 1) && localSyncDigest != null)
                {
                    // Update UI / Publish
                    _eventAggregator.GetEvent<QuoteDataChangedEvent>().Publish(true);

                    if (DataAccessUtils.QuotesTotalCount == DataAccessUtils.QuotesReturnedCount)
                    {
                        if (localSyncDigest == null)
                            localSyncDigest = new LocalSyncDigest();
                        localSyncDigest.localTick++;
                        localSyncDigest.LastRecordId = null;
                        localSyncDigest.LastSyncTime = DateTime.Now;
                        DataAccessUtils.IsQuotesSyncCompleted = true;
                    }
                    await _localSyncDigestRepository.UpdateLocalSyncDigestDtlsAsync(localSyncDigest);
                }
            }
        }

        /// <summary>
        ///     Get quotes list along with customerName, salesRepName
        /// </summary>
        /// <param name="salesRepId"></param>
        /// <returns></returns>
     
        public async Task<List<QuoteDetails>> GetQuotesListAsync(string salesRepId)
        {
            List<QuoteDetails> quotesList = null;
            try
            {
                quotesList =
                    await
                        _sageSalesDB.QueryAsync<QuoteDetails>(
                            "SELECT distinct customer.customerName, quote.Id, quote.CustomerId, quote.QuoteId, quote.CreatedOn, quote.amount, quote.quoteStatus,quote.QuoteDescription,(select RepName from SalesRep as RP where RP.RepId='" + salesRepId + "') as RepName FROM quote INNER JOIN customer ON customer.customerID = quote.customerId And Quote.QuoteStatus!='" +
                            DataAccessUtils.IsOrderQuoteStatus + "' And Quote.QuoteStatus!='" +
                            DataAccessUtils.TemporaryQuoteStatus + "' And Quote.IsDeleted='0'");
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                ;
            }
            return quotesList;
        }

        /// <summary>
        ///     Extracts data from response and updates quotes, addresses and quoteLineItems
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        public async Task SaveQuoteAsync(JsonObject sDataQuote)
        {
            IJsonValue value;
            Quote quote = await SaveQuoteDetailsAsync(sDataQuote);

            if (sDataQuote.TryGetValue("Customer", out value))
            {
                if (value.ValueType.ToString() != DataAccessUtils.Null)
                {
                    JsonObject sDataCustomer = sDataQuote.GetNamedObject("Customer");
                    Customer customer = await _customerRepository.AddOrUpdateCustomerJsonToDbAsync(sDataCustomer);
                    if (customer != null)
                    {
                        await _addressRepository.SaveAddressesAsync(sDataCustomer, customer.CustomerId);
                    }
                }
            }
            await _quoteLineItemRepository.SaveQuoteLineItemsAsync(sDataQuote, quote.QuoteId);
        }

        /// <summary>
        ///     Save posted quote json to local dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <returns></returns>
        public async Task<Quote> SavePostedQuoteToDbAsync(JsonObject sDataQuote, Quote pendingQuote, Address address,
            QuoteLineItem pendingQuoteLineItem)
        {
            IJsonValue value;

            if (pendingQuote.QuoteId.Contains(DataAccessUtils.Pending))
            {
                if (sDataQuote.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Quote> pendingQuoteList;
                        pendingQuoteList =
                            await _sageSalesDB.QueryAsync<Quote>("SELECT * FROM Quote where QuoteId=?", pendingQuote.QuoteId);

                        if (pendingQuoteList.FirstOrDefault() != null)
                        {
                            await
                                _sageSalesDB.QueryAsync<Quote>("Update Quote Set QuoteId=? where QuoteId=?",
                                    sDataQuote.GetNamedString("Id"), pendingQuoteList.FirstOrDefault().QuoteId);
                            await
                                _sageSalesDB.QueryAsync<QuoteLineItem>(
                                    "Update QuoteLineItem Set QuoteId=? where QuoteId=?",
                                    sDataQuote.GetNamedString("Id"), pendingQuoteList.FirstOrDefault().QuoteId);
                        }
                    }
                }
            }

            //if (pendingQuoteLineItemId !=null  && pendingQuoteLineItemId.Contains(DataAccessUtils.Pending))
            //{
            //    await _sageSalesDB.QueryAsync<QuoteLineItem>("Delete From QuoteLineItem Where QuoteLineItemId=?", pendingQuoteLineItemId);
            //}

            if (address!=null)
            {
                if (sDataQuote.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        //JsonObject sDataShippingAdress = sDataQuote.GetNamedObject("ShippingAddress");

                        await
                            _sageSalesDB.QueryAsync<Address>("Update Address Set AddressId=? where AddressId=?",
                                sDataQuote.GetNamedString("Id"), address.AddressId);
                        //if (!string.IsNullOrEmpty(quote.CustomerId))
                        //{
                            Address addressObj=await _addressRepository.AddOrUpdateAddressJsonToDbAsync(sDataQuote, address.CustomerId);
                            if (addressObj != null)
                            {
                                await
                                _sageSalesDB.QueryAsync<Quote>("Update Quote Set AddressId=? where AddressId=?",
                                    sDataQuote.GetNamedString("Id"), address.AddressId);
                            }
                        //}
                    }
                }
            }

            Quote quote = await SaveQuoteDetailsAsync(sDataQuote);

            // Commented below code as we are not getting Quote LineItem details in Quote response

            //await
            //    _quoteLineItemRepository.SavePostedQuoteLineItemsAsync(sDataQuote, quote.QuoteId, pendingQuoteLineItem);
            //await _quoteLineItemRepository.SaveQuoteLineItemsAsync(sDataQuote, quote.QuoteId);

            return quote;
        }

        /// <summary>
        ///     Get quote details along with salesRepName
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task<QuoteDetails> GetQuoteDetailsAsync(string quoteId)
        {
            List<QuoteDetails> quote = null;
            string salesRepId = await _salesRepRepository.GetSalesRepId();
            try
            {
               
                quote =
                    await
                        _sageSalesDB.QueryAsync<QuoteDetails>(
                            "SELECT distinct quote.Id, quote.QuoteId, quote.CreatedOn, quote.ExpiryDate, quote.CustomerId, quote.AddressId, quote.Amount, quote.Tax, quote.ShippingAndHandling, quote.DiscountPercent, quote.quoteStatus, quote.QuoteDescription, quote.TenantId, quote.QuoteNumber, (select RepName from SalesRep as RP where RP.RepId='" + salesRepId + "') as RepName FROM quote where QuoteId=?",
                            quoteId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                ;
            }
            return quote.FirstOrDefault();
        }

        /// <summary>
        ///     Get quote details along with salesRepName
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns>Quote</returns>
        public async Task<Quote> GetQuoteAsync(string quoteId)
        {
            List<Quote> quote = null;
            try
            {
                quote = await _sageSalesDB.QueryAsync<Quote>("Select * from Quote where Quote.QuoteId=?", quoteId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                ;
            }
            return quote.FirstOrDefault();
        }

        /// <summary>
        ///     Returns quote based on primary key
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Quote> GetQuoteFromPrimaryKey(int Id)
        {
            List<Quote> quote = null;
            try
            {
                quote = await _sageSalesDB.QueryAsync<Quote>("Select * from Quote where Quote.Id=?", Id);
            }

            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            return quote.FirstOrDefault();
        }

        /// <summary>
        ///     Get quote details based on CustomerId
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task<List<QuoteDetails>> GetQuotesForCustomerAsync(string customerId)
        {
            List<QuoteDetails> quote = null;
            try
            {
                quote =
                    await
                        _sageSalesDB.QueryAsync<QuoteDetails>(
                            "SELECT distinct customer.customerName, quote.Id, quote.CustomerId, quote.QuoteId, quote.CreatedOn, quote.amount, quote.quoteStatus,quote.QuoteDescription, SalesRep.RepName FROM customer, quote left Join SalesRep On SalesRep.RepId=Quote.RepId where Quote.QuoteStatus!='IsOrder'  And Quote.QuoteStatus!='Temporary' and customer.customerId=quote.customerId and quote.customerId=? order by quote.createdOn desc",
                            customerId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quote;
        }

        /// <summary>
        ///     Adds or updates quote json response to dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <returns></returns>
        public async Task<Quote> AddOrUpdateQuoteJsonToDbAsync(JsonObject sDataQuote)
        {
            try
            {
                IJsonValue value;
                bool entityStatusDeleted = false;
                if (sDataQuote.TryGetValue("Id", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        List<Quote> quoteList;
                        quoteList =
                            await
                                _sageSalesDB.QueryAsync<Quote>("SELECT * FROM Quote where QuoteId=?",
                                    sDataQuote.GetNamedString("Id"));

                        if (sDataQuote.TryGetValue("EntityStatus", out value))
                        {
                            if (value.ValueType.ToString() != DataAccessUtils.Null)
                            {
                                if (sDataQuote.GetNamedString("EntityStatus").Contains("Deleted"))
                                    entityStatusDeleted = true;
                            }
                        }

                        if (quoteList.FirstOrDefault() != null)
                        {
                            if (entityStatusDeleted)
                            {
                                if (!quoteList.FirstOrDefault().IsPending)
                                    //await _sageSalesDB.QueryAsync<Quote>("DELETE FROM Quote where QuoteId=?", sDataQuote.GetNamedString("$key"));
                                    await DeleteQuoteFromDbAsync(sDataQuote.GetNamedString("Id"));
                                else
                                    await
                                        _sageSalesDB.QueryAsync<Quote>(
                                            "UPDATE Quote SET EntityStatus=? where QuoteId=?",
                                            sDataQuote.GetNamedString("EntityStatus"), sDataQuote.GetNamedString("Id"));
                            }
                            else
                            {
                                return await UpdateQuoteJsonToDbAsync(sDataQuote, quoteList.FirstOrDefault());
                            }
                        }
                        else
                        {
                            if (!entityStatusDeleted)
                                return await AddQuoteJsonToDbAsync(sDataQuote);
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        ///     Add quote to local dB based on selectedQuoteType
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="selectedQuoteType"></param>
        /// <param name="quoteDescription"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<Quote> AddQuoteToDbAsync(Quote quoteDtls, string selectedQuoteType, string orderId)
        {
            Quote quote = null;
            SalesRep salesRep = null;
            //Returns shipping address for that customerId
            Address address = await _addressRepository.GetShippingAddressForCustomer(quoteDtls.CustomerId);
            if (address != null)
            {
                quote = new Quote();
                //Returns salesRep data
                salesRep = (await _salesRepRepository.GetSalesRepDtlsAsync()).FirstOrDefault();

                //Saving quote in local dB
                quote.QuoteId = DataAccessUtils.Pending + Guid.NewGuid();
                quote.QuoteStatus = DataAccessUtils.DraftQuote;
                quote.CreatedOn = DateTime.Now;
                quote.SubmittedDate = DateTime.Now;
                quote.QuoteDescription = quoteDtls.QuoteDescription;
                quote.CustomerId = quoteDtls.CustomerId;
                quote.AddressId = address.AddressId;
                quote.TenantId = salesRep.TenantId;
                quote.RepId = salesRep.RepId;
                quote.Amount = quoteDtls.Amount;
                quote.IsPending = true;

                //If selectedQuoteType is from previousPurchasedItems
                if (selectedQuoteType.Equals(DataAccessUtils.PreviousPurchasedItems))
                {
                    return await AddQuoteLineItemsFromPreviousPurchasedProducts(quote);
                }
                if (selectedQuoteType.Equals(DataAccessUtils.PreviousOrder))
                {
                    return await AddQuoteLineItemsFromPreviousOrder(quote, orderId);
                }
                //Amount is made '0' because no items will be added.
                //quote.Amount = CalculateAmount(0, 0);
                await _sageSalesDB.InsertAsync(quote);

                return quote;
            }
            return null;
        }

        /// <summary>
        ///     Update quote to local dB
        /// </summary>
        /// <param name="quote"></param>
        /// <returns>Quote</returns>
        public async Task<Quote> UpdateQuoteToDbAsync(Quote quote)
        {
            List<Quote> quoteList = null;
            try
            {
                quoteList = await _sageSalesDB.QueryAsync<Quote>("Select * from Quote where QuoteId=?", quote.QuoteId);

                if (quoteList.FirstOrDefault() != null)
                    await _sageSalesDB.UpdateAsync(quote);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quote;
        }

        /// <summary>
        ///     Returns pending quotes which are not synced
        /// </summary>
        /// <returns></returns>
        public async Task<List<Quote>> GetPendingQuotes()
        {
            List<Quote> quotePendingList = null;
            try
            {
                quotePendingList =
                    await
                        _sageSalesDB.QueryAsync<Quote>(
                            "Select * from Quote where QuoteId like 'Pending%' and IsDeleted='0'");
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quotePendingList;
        }

        /// <summary>
        ///     Returns pending shipping address which are not synced
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuoteShippingAddress>> GetPendingShippingAddress()
        {
            List<QuoteShippingAddress> quoteShippingAddressPendingList = null;
            try
            {
                quoteShippingAddressPendingList =
                    await
                        _sageSalesDB.QueryAsync<QuoteShippingAddress>(
                            "Select distinct * from Address inner join Quote where Quote.IsDeleted='0' and Quote.AddressId = Address.AddressId and (Address.AddressId like 'Pending%' or Address.IsPending='1')");
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteShippingAddressPendingList;
        }

        /// <summary>
        ///     Returns deleted quotes which are not synced
        /// </summary>
        /// <returns></returns>
        public async Task<List<Quote>> GetDeletedQuotes()
        {
            List<Quote> quoteDeleteList = null;
            try
            {
                quoteDeleteList = await _sageSalesDB.QueryAsync<Quote>("Select * from Quote where IsDeleted='1'");
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteDeleteList;
        }

        /// <summary>
        ///     Deletes quote and its relevant quoteLineItems from local dB
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task DeleteQuoteFromDbAsync(string quoteId)
        {
            try
            {
                await _quoteLineItemRepository.DeleteQuoteLineItemsFromDbAsync(quoteId);
                await _sageSalesDB.QueryAsync<Quote>("Delete From Quote where Quote.QuoteId=?", quoteId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Mark quote as delete to enable offline capability
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task MarkQuoteAsDeleted(string quoteId)
        {
            try
            {
                await _quoteLineItemRepository.MarkQuoteLineItemsAsDeleted(quoteId);
                await _sageSalesDB.QueryAsync<Quote>("Update Quote set isDeleted='1' where QuoteId=?", quoteId);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Add QuoteLineItems to Quote by selecting items/products for the customer
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private async Task<Quote> AddQuoteLineItemsFromPreviousPurchasedProducts(Quote quote)
        {
            List<OrderLineItem> previouslyPurchasedProductList = null;
            QuoteLineItem quoteLineItem;
            decimal amount = 0;
            previouslyPurchasedProductList =
                await _orderLineItemRepository.GetPreviouslyPurchasedProducts(quote.CustomerId);

            foreach (OrderLineItem product in previouslyPurchasedProductList)
            {
                quoteLineItem = new QuoteLineItem();
                quoteLineItem.QuoteId = quote.QuoteId;
                quoteLineItem.QuoteLineItemId = DataAccessUtils.Pending + Guid.NewGuid();
                quoteLineItem.tenantId = quote.TenantId;
                quoteLineItem.ProductId = product.ProductId;
                quoteLineItem.Price = product.Price;
                //For previously purchased products quantity is 0 by default
                quoteLineItem.Quantity = 0;
                quoteLineItem.IsPending = true;

                amount = amount + CalculateAmount(quoteLineItem.Quantity, quoteLineItem.Price);

                await _quoteLineItemRepository.AddQuoteLineItemToDbAsync(quoteLineItem);
            }
            //Amount is made '0' because from previous purchased items quantity is 0(Price * Quantity).
            quote.Amount = amount;

            await _sageSalesDB.InsertAsync(quote);
            return quote;
        }

        /// <summary>
        ///     Add QuoteLineItems to Quote by selecting items/products from previous order
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private async Task<Quote> AddQuoteLineItemsFromPreviousOrder(Quote quote, string orderId)
        {
            List<OrderLineItem> orderLineItemList;
            QuoteLineItem quoteLineItem;
            decimal amount = 0;
            if (!string.IsNullOrEmpty(orderId))
            {
                orderLineItemList = await _orderLineItemRepository.GetOrderLineItemForOrder(orderId);
                foreach (OrderLineItem orderLineItem in orderLineItemList)
                {
                    quoteLineItem = new QuoteLineItem();
                    quoteLineItem.QuoteId = quote.QuoteId;
                    quoteLineItem.QuoteLineItemId = DataAccessUtils.Pending + Guid.NewGuid();
                    quoteLineItem.tenantId = quote.TenantId;
                    quoteLineItem.ProductId = orderLineItem.ProductId;
                    quoteLineItem.Price = orderLineItem.Price;
                    quoteLineItem.Quantity = orderLineItem.Quantity;
                    quoteLineItem.IsPending = true;

                    amount = amount + CalculateAmount(quoteLineItem.Quantity, quoteLineItem.Price);

                    await _quoteLineItemRepository.AddQuoteLineItemToDbAsync(quoteLineItem);
                }
                quote.Amount = amount;

                await _sageSalesDB.InsertAsync(quote);

                return quote;
            }
            return null;
        }

        /// <summary>
        ///     Calculates amount/sub-total for the quote created
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private decimal CalculateAmount(int quantity, decimal price)
        {
            decimal amount;
            amount = Convert.ToDecimal(quantity*price);
            return amount;
        }

        /// <summary>
        ///     Extracts quote from Json and updates the same
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Quote> SaveQuoteDetailsAsync(JsonObject sDataQuote)
        {
            return await AddOrUpdateQuoteJsonToDbAsync(sDataQuote);
        }

        /// <summary>
        ///     Add quote json response to dB
        /// </summary>
        /// <param name="sDataCustomer"></param>
        /// <returns></returns>
        private async Task<Quote> AddQuoteJsonToDbAsync(JsonObject sDataQuote)
        {
            var quoteObj = new Quote();           
            try
            {
                quoteObj.QuoteId = sDataQuote.GetNamedString("Id");
                quoteObj = await ExtractQuoteFromJsonAsync(sDataQuote, quoteObj);
                await _sageSalesDB.InsertAsync(quoteObj);            
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteObj; 
        }

        /// <summary>
        ///     Update quote json response to dB
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <param name="quoteDbObj"></param>
        /// <returns></returns>
        private async Task<Quote> UpdateQuoteJsonToDbAsync(JsonObject sDataQuote, Quote quoteDbObj)
        {
            try
            {
                quoteDbObj = await ExtractQuoteFromJsonAsync(sDataQuote, quoteDbObj);
                await _sageSalesDB.UpdateAsync(quoteDbObj);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quoteDbObj;
        }

        /// <summary>
        ///     Extract quote json response
        /// </summary>
        /// <param name="sDataQuote"></param>
        /// <param name="quote"></param>
        /// <returns></returns>
        private async Task<Quote> ExtractQuoteFromJsonAsync(JsonObject sDataQuote, Quote quote)
        {
            try
            {
                IJsonValue value;
                if (sDataQuote.TryGetValue("Description", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.QuoteDescription = sDataQuote.GetNamedString("Description");
                    }
                }

                //if (sDataQuote.TryGetValue("TenantId", out value))
                //{
                //    if (value.ValueType.ToString() != DataAccessUtils.Null)
                //    {
                //        quote.TenantId = sDataQuote.GetNamedString("TenantId");
                //    }
                //}
                if (sDataQuote.TryGetValue("CreatedOn", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.CreatedOn = ConvertJsonStringToDateTime(sDataQuote.GetNamedString("CreatedOn"));
                    }
                }

                if (sDataQuote.TryGetValue("SubmittedDate", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.SubmittedDate = ConvertJsonStringToDateTime(sDataQuote.GetNamedString("SubmittedDate"));
                    }
                }
                if (sDataQuote.TryGetValue("Status", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.QuoteStatus = sDataQuote.GetNamedString("Status");
                    }
                }
                else
                {
                    quote.QuoteStatus =DataAccessUtils.SubmitQuote;
                }

                if (sDataQuote.TryGetValue("SandH", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.ShippingAndHandling = Convert.ToDecimal(sDataQuote.GetNamedNumber("SandH"));
                    }
                }
                if (sDataQuote.TryGetValue("Tax", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.Tax = Convert.ToDecimal(sDataQuote.GetNamedNumber("Tax"));
                    }
                }
                if (sDataQuote.TryGetValue("ExpiryDate", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.ExpiryDate = ConvertJsonStringToDateTime(sDataQuote.GetNamedString("ExpiryDate"));
                    }
                }
                if (sDataQuote.TryGetValue("QuoteTotal", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.Amount = Convert.ToDecimal(sDataQuote.GetNamedNumber("QuoteTotal"));
                    }
                }
                if (sDataQuote.TryGetValue("SubTotal", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.SubTotal = Convert.ToDecimal(sDataQuote.GetNamedNumber("SubTotal"));
                    }
                }
                
                if (sDataQuote.TryGetValue("ExtRef", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.ExternalReferenceNumber = sDataQuote.GetNamedString("ExtRef");
                    }
                }
                if (sDataQuote.TryGetValue("DiscountPercent", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.DiscountPercent = Convert.ToDecimal(sDataQuote.GetNamedNumber("DiscountPercent"));
                    }
                }
                if (sDataQuote.TryGetValue("QuoteNumber", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.QuoteNumber = Convert.ToInt32(sDataQuote.GetNamedNumber("QuoteNumber"));
                    }
                }

                if (sDataQuote.TryGetValue("CustomerId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.CustomerId = sDataQuote.GetNamedString("CustomerId");
                    }
                }               
                if (sDataQuote.TryGetValue("ShippingAddressId", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {                                             
                         quote.AddressId = sDataQuote.GetNamedString("ShippingAddressId");                        
                    }
                }

                //Yet to implement Sales Rep *****
                if (sDataQuote.TryGetValue("SalesRep", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        JsonObject sDataSalesRep = sDataQuote.GetNamedObject("SalesRep");
                        if (sDataSalesRep.GetNamedValue("Id").ValueType.ToString() != DataAccessUtils.Null)
                        {
                            SalesRep salesRep =
                                await _salesRepRepository.AddOrUpdateSalesRepJsonToDbAsync(sDataSalesRep);
                            quote.RepId = salesRep.RepId;
                        }
                    }
                }

                if (sDataQuote.TryGetValue("EntityStatus", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.EntityStatus = sDataQuote.GetNamedString("EntityStatus");
                    }
                }

                if (sDataQuote.TryGetValue("IsDeleted", out value))
                {
                    if (value.ValueType.ToString() != DataAccessUtils.Null)
                    {
                        quote.IsDeleted = sDataQuote.GetNamedBoolean("IsDeleted");
                    }
                }

                quote.IsPending = false;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return quote;
        }
        


        //Need to confirm with this..............*******

        /// <summary>
        ///     Converts Json date string to respective Date and Time format
        /// </summary>
        /// <param name="jsonTime"></param>
        /// <returns></returns>
        private DateTime ConvertJsonStringToDateTime(string jsonTime)
        {
            if (!string.IsNullOrEmpty(jsonTime) && jsonTime.IndexOf("Date") > -1)
            {
                string milis = jsonTime.Substring(jsonTime.IndexOf("(") + 1);
                string sign = milis.IndexOf("+") > -1 ? "+" : "-";
                string hours = "";
                // Need to change based on GMT........ To be Confirmed
                if (milis.IndexOf(sign) > -1)
                {
                    hours = milis.Substring(milis.IndexOf(sign));
                    milis = milis.Substring(0, milis.IndexOf(sign));
                    hours = hours.Substring(0, hours.IndexOf(")"));
                    return
                        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
                            .AddHours(Convert.ToInt64(hours)/100);
                }
                hours = "0";
                milis = milis.Substring(0, milis.IndexOf(")"));
                return
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(milis))
                        .AddHours(Convert.ToInt64(hours)/100);
            }

            return DateTime.Now;
        }

        #endregion
    }
}