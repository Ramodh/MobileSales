using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Microsoft.Practices.Prism.PubSubEvents;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.JsonHelpers;

namespace SageMobileSales.ServiceAgents.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ILocalSyncDigestService _localSyncDigestService;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters;

        public QuoteService(IServiceAgent serviceAgent, ILocalSyncDigestService localSyncDigestService,
            ILocalSyncDigestRepository localSyncDigestRepository,
            IQuoteRepository quoteRepository, ISalesRepRepository salesRepRepository,
            IQuoteLineItemRepository quoteLineItemRepository, IAddressRepository addressRepository,
            IEventAggregator eventAggregator)
        {
            _serviceAgent = serviceAgent;
            _localSyncDigestService = localSyncDigestService;
            _localSyncDigestRepository = localSyncDigestRepository;
            _quoteRepository = quoteRepository;
            _quoteLineItemRepository = quoteLineItemRepository;
            _addressRepository = addressRepository;
            _salesRepRepository = salesRepRepository;
            _eventAggregator = eventAggregator;
        }

        #region public methods

        /// <summary>
        ///     Start syncing quotes data
        /// </summary>
        /// <returns></returns>
        public async Task StartQuoteSyncProcess()
        {
            Constants.IsSyncAvailable =
                await _localSyncDigestService.SyncLocalDigest(Constants.QuoteEntity, Constants.syncDigestQueryEntity);
            if (Constants.IsSyncAvailable)
            {
                await _localSyncDigestService.SyncLocalSource(Constants.QuoteEntity, Constants.syncSourceQueryEntity);
                await SyncQuotes();
            }
            else
            {
                _eventAggregator.GetEvent<QuoteDataChangedEvent>().Publish(true);
            }
        }

        /// <summary>
        ///     Post draft quote
        /// </summary>
        /// Here Quote is submitted in three conditions(scratch, previous purchased items, previous orders).
        /// For Scratch and Previous purchased items holds good without passing inventory items along with the quote. Because for scratch no items would have been added
        /// and for previous purchased items amount is '0' because the quantity would have been set to '0' by default.
        /// For Previous order inventory items are added and the response is saved.
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<Quote> PostDraftQuote(Quote quote)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                //parameters.Add("include", "Details");
                object obj;

                List<QuoteLineItem> quoteLineItemList =
                    await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

                obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItemList);

                HttpResponseMessage quoteResponse = null;

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPostObjectRequest(Constants.TenantId, Constants.DraftQuotes, null,
                            Constants.AccessToken,
                            null, obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    return await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }

        /// <summary>
        ///     Patch draft quote
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<Quote> PatchDraftQuote(Quote quote)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                //parameters.Add("include", "Details");
                object obj;

                List<QuoteLineItem> quoteLineItemList =
                    await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

                obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItemList);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;

                quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

                quoteResponse = await
                    _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                        Constants.AccessToken,
                        null, obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    return await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return null;
        }


        /// <summary>
        ///     Submit quote via service using post/patch http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<Quote> SubmitQuote(Quote quote)
        {

            List<QuoteLineItem> quoteLineItemList =
                await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

            // Deleting quoteLineItems if quantity is <= 0........ confirm on this
            foreach (QuoteLineItem quoteLineItem in quoteLineItemList)
            {
                if (quoteLineItem.Quantity <= 0)
                    await _quoteLineItemRepository.DeleteQuoteLineItemFromDbAsync(quoteLineItem.QuoteLineItemId);
            }

            IEnumerable<QuoteLineItem> result =
                quoteLineItemList.Where(e => e.QuoteLineItemId.Contains(Constants.Pending));

            //Checking for pending quoteId before submitting
            if (quote.QuoteId.Contains(Constants.Pending))
            {
                if (quote.AddressId.Contains(Constants.Pending))
                {
                    Address address = await UpdateQuoteShippingAddress(quote, quote.AddressId);
                    if (address.AddressId.Contains(Constants.Pending))
                    {
                        quote.AddressId = null;
                        await _quoteRepository.UpdateQuoteToDbAsync(quote);
                    }
                    else if (address != null)
                    {
                        quote.AddressId = address.AddressId;
                        quote = await PostSubmitQuote(quote, result);
                    }
                    // Confirm before posting quote whether it has Valid QuoteId
                    //quote = await PostSubmitQuote(quote, result);
                    //await
                    //    UpdateQuoteShippingAddress(quote,
                    //        await _addressRepository.GetShippingAddress(quote.AddressId));
                }
                else
                {
                    quote = await PostSubmitQuote(quote, result);
                }
                return quote;
            }
            else
            {
                if (quote.AddressId.Contains(Constants.Pending))
                {
                    Address address = await UpdateQuoteShippingAddress(quote, quote.AddressId);
                    if (address.AddressId.Contains(Constants.Pending))
                    {
                        quote.AddressId = null;
                        await _quoteRepository.UpdateQuoteToDbAsync(quote);
                    }
                    else if (address != null)
                    {
                        quote.AddressId = address.AddressId;
                        quote = await PatchSubmitQuote(quote, result);
                    }
                }
                else
                {
                    quote = await PatchSubmitQuote(quote, result);
                }
            }

            //quote = await PatchSubmitQuote(quote, result);
            return quote;
        }

        /// <summary>
        ///     Revert submitted quote to draft via service using patch http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task RevertSubmittedQuoteToDraft(Quote quote)
        {
            parameters = new Dictionary<string, string>();
            parameters.Add("include", "Details,ShippingAddress");
            object obj;

            //List<QuoteLineItem> quoteLineItemList = await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

            obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, null);

            HttpResponseMessage quoteResponse = null;
            string quoteEntityId;

            quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

            quoteResponse =
                await
                    _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                        Constants.AccessToken, parameters, obj);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
            }
        }

        /// <summary>
        ///     Update quote, shipping address key via service using patch http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task UpdateQuoteShippingAddressKey(Quote quote)
        {
            parameters = new Dictionary<string, string>();
            object obj;

            //List<QuoteLineItem> quoteLineItemList =
            //  await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

            obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, null);

            HttpResponseMessage quoteResponse = null;
            string quoteEntityId;

            quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

            quoteResponse =
                await
                    _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                        Constants.AccessToken,
                        null, obj);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
            }
        }

        // If post shipping address is made seperate call from quote,
        // Then needs to move this method to AddressService if required.
        /// <summary>
        ///     Update quote with shipping address via service using post http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task<Address> UpdateQuoteShippingAddress(Quote quote, string addressId)
        {
            object obj;
            Address addressObj = null;
            Address address = await _addressRepository.GetShippingAddress(addressId);
            //List<QuoteLineItem> quoteLineItemList = await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

            obj = GetShippingAddressObj(quote.CustomerId, address);

            HttpResponseMessage addressResponse = null;
            //string quoteEntityId;

            //quoteEntityId = Constants.QuoteEntity + "('" + quote.QuoteId + "')";

            addressResponse =
                await
                    _serviceAgent.BuildAndPostObjectRequest(Constants.TenantId, Constants.Address, null,
                        Constants.AccessToken, null, obj);

            if (addressResponse.StatusCode.ToString().Equals("1011"))
            {
                await _addressRepository.DeleteAddressFromDbAsync(address);
                return address;
            }

            if (addressResponse != null && addressResponse.IsSuccessStatusCode)
            {
                JsonObject sDataAddress = await _serviceAgent.ConvertTosDataObject(addressResponse);
                addressObj = await _addressRepository.SavePostedAddressToDbAsync(sDataAddress, quote.CustomerId, addressId);
                //addressObj = await _addressRepository.AddOrUpdateAddressJsonToDbAsync(sDataAddress, quote.CustomerId);
                //quote.AddressId = sDataQuote.GetNamedString("$key");
                //quote = await PatchDraftQuote(quote);
                //await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, address, null);
            }
            return addressObj;
        }

        /// <summary>
        ///     Updated discount, shipping and handling via service using put http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task UpdateDiscountOrShippingAndHandling(Quote quote)
        {
            parameters = new Dictionary<string, string>();
            parameters.Add("include", "Details,ShippingAddress");
            object obj;

            //List<QuoteLineItem> quoteLineItemList = await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

            obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, null);

            HttpResponseMessage quoteResponse = null;
            string quoteEntityId;

            quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

            quoteResponse =
                await
                    _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                        Constants.AccessToken, parameters, obj);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
            }
        }

        /// <summary>
        ///     Deletes quote via service using delete http method
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public async Task DeleteQuote(string quoteId)
        {
            HttpResponseMessage quoteResponse = null;
            string EntityId;

            EntityId = Constants.DraftQuotes + "('" + quoteId + "')";

            quoteResponse =
                await
                    _serviceAgent.BuildAndDeleteRequest(Constants.TenantId, EntityId, null, Constants.AccessToken,
                        parameters);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                await _quoteRepository.DeleteQuoteFromDbAsync(quoteId);
            }
        }


        //Method used for "Buy it Now"
        ///// <summary>
        /////     Calculates sales tax for quote on choosing 'Buy it Now'
        ///// </summary>
        ///// <param name="quote"></param>
        ///// <returns></returns>
        //public async Task CalculateSalesTaxForQuote(Quote quote)
        //{
        //    try
        //    {
        //        parameters = new Dictionary<string, string>();
        //        parameters.Add("quoteId", quote.QuoteId);
        //        string queryEntity = "$service/CalculateSalesTax";

        //        HttpResponseMessage quoteResponse = null;

        //        quoteResponse =
        //            await
        //                _serviceAgent.BuildAndSendRequest(null, Constants.QuoteEntity, queryEntity, null,
        //                    Constants.AccessToken, parameters);
        //        if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
        //        {
        //            JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
        //            //return await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log = AppEventSource.Log.WriteLine(ex);
        //        AppEventSource.Log.Error(_log);
        //    }
        //}

        /// <summary>
        ///     Sync pending quotes
        /// </summary>
        /// <returns></returns>
        public async Task SyncOfflineQuotes()
        {
            try
            {
                List<Quote> quotePendingList = await _quoteRepository.GetPendingQuotes();
                foreach (Quote quote in quotePendingList)
                {
                    if (quote.AddressId.Contains(Constants.Pending))
                    {
                        // Confirm before posting quote whether it has Valid QuoteId
                        //Quote _quote = await PostDraftQuote(quote);
                        //if (_quote != null)
                        //    await
                        //        UpdateQuoteShippingAddress(quote,
                        //            await _addressRepository.GetShippingAddress(_quote.AddressId));
                        Address address = await UpdateQuoteShippingAddress(quote, quote.AddressId);
                        if (address.AddressId.Contains(Constants.Pending))
                        {
                            quote.AddressId = null;
                            await _quoteRepository.UpdateQuoteToDbAsync(quote);
                        }
                        else if (address != null)
                        {
                            quote.AddressId = address.AddressId;
                            await PostDraftQuote(quote);
                        }
                    }
                    else
                    {
                        await PostDraftQuote(quote);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Sync deleted quotes
        /// </summary>
        /// <returns></returns>
        public async Task SyncOfflineDeletedQuotes()
        {
            try
            {
                List<Quote> quoteDeleteList = await _quoteRepository.GetDeletedQuotes();
                foreach (Quote quote in quoteDeleteList)
                {
                    await DeleteQuote(quote.QuoteId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// Sync offline shipping address
        /// </summary>
        /// <returns></returns>
        public async Task SyncOfflineShippingAddress()
        {
            try
            {
                List<QuoteShippingAddress> shippingAddressDetails = await _quoteRepository.GetPendingShippingAddress();
                foreach (QuoteShippingAddress shippingAddress in shippingAddressDetails)
                {
                    Address address = null;
                    Quote quote = await _quoteRepository.GetQuoteAsync(shippingAddress.QuoteId);

                    if (shippingAddress.QuoteId.Contains(Constants.Pending))
                    {
                        //Address address = await _addressRepository.GetShippingAddress(shippingAddress.AddressId);

                        address = await UpdateQuoteShippingAddress(quote, shippingAddress.AddressId);
                        if (address.AddressId.Contains(Constants.Pending))
                        {
                            quote.AddressId = null;
                            await _quoteRepository.UpdateQuoteToDbAsync(quote);
                        }
                        else if (address != null)
                        {
                            quote.AddressId = address.AddressId;
                            await PostDraftQuote(quote);
                        }

                        //Quote quote = await _quoteRepository.GetQuoteAsync(shippingAddress.QuoteId);
                        //quote = await PostDraftQuote(quote);
                        //if (quote != null)
                        //    await
                        //        UpdateQuoteShippingAddress(quote,
                        //            await _addressRepository.GetShippingAddress(shippingAddress.AddressId));
                    }
                    else
                    {
                        //Quote quote = await _quoteRepository.GetQuoteAsync(shippingAddress.QuoteId);
                        if (shippingAddress.AddressId.Contains(Constants.Pending))
                        {
                            //Address address = await _addressRepository.GetShippingAddress(shippingAddress.AddressId);

                            address = await UpdateQuoteShippingAddress(quote, shippingAddress.AddressId);
                            if (address.AddressId.Contains(Constants.Pending))
                            {
                                quote.AddressId = null;
                                await _quoteRepository.UpdateQuoteToDbAsync(quote);
                            }
                            else if (address != null)
                            {
                                quote.AddressId = address.AddressId;
                                await UpdateQuoteShippingAddressKey(quote);
                            }
                        }
                        //await
                        //    UpdateQuoteShippingAddress(
                        //        await _quoteRepository.GetQuoteAsync(shippingAddress.QuoteId),
                        //        await _addressRepository.GetShippingAddress(shippingAddress.AddressId));
                        else
                            await UpdateQuoteShippingAddressKey(quote);
                        //await
                        //    UpdateQuoteShippingAddressKey(
                        //        await _quoteRepository.GetQuoteAsync(shippingAddress.QuoteId));
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// Post submit quote
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task<Quote> PostSubmitQuote(Quote quote, IEnumerable<QuoteLineItem> result)
        {
            Object obj = ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(quote, result.ToList());

            HttpResponseMessage quoteResponse = null;
            //string quoteEntityId;

            //quoteEntityId = Constants.SubmitQuoteEntity;

            quoteResponse =
                await
                    _serviceAgent.BuildAndPostObjectRequest(Constants.TenantId, Constants.SubmitQuoteEntity, null,
                        Constants.AccessToken, null, obj);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                quote = await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
            }
            return quote;
        }

        /// <summary>
        /// Patch submit quote
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task<Quote> PatchSubmitQuote(Quote quote, IEnumerable<QuoteLineItem> result)
        {
            Object obj = ConvertSubmitQuoteToJsonFormattedObject(quote, result.ToList());

            HttpResponseMessage quoteResponse = null;
            string quoteEntityId;

            quoteEntityId = Constants.SubmitQuoteEntity + "('" + quote.QuoteId + "')";

            quoteResponse =
                await
                    _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                        Constants.AccessToken, null, obj);
            if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
            {
                JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                quote = await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, null);
            }
            return quote;
        }
        #endregion

        #region private methods

        /// <summary>
        ///     Makes call to BuildAndSendRequest method to make service call to get quotes data.
        ///     Once we get the response converts it into JsonObject.
        ///     This call is looped internally to get the subsequent quotes batch's data
        ///     by updating LastRecordId parameter everytime when we make request to get next batch data.
        ///     This loop will run till the completion of quotes data Sync.
        /// </summary>
        /// <returns></returns>
        private async Task SyncQuotes()
        {
            LocalSyncDigest digest = await _localSyncDigestRepository.GetLocalSyncDigestDtlsAsync(Constants.QuoteEntity);
            parameters = new Dictionary<string, string>();
            if (digest != null)
            {
                parameters.Add("LocalTick", digest.localTick.ToString());
                parameters.Add("LastRecordId", digest.LastRecordId);
            }
            else
            {
                digest = new LocalSyncDigest();
                digest.SDataEntity = Constants.QuoteEntity;
                digest.localTick = 0;
                parameters.Add("LocalTick", digest.localTick.ToString());
                parameters.Add("LastRecordId", null);
            }

            string salesRepId = await _salesRepRepository.GetSalesRepId();
            if (!string.IsNullOrEmpty(salesRepId))
            {
                parameters.Add("Count", "100");
                HttpResponseMessage quotesResponse = null;

                Constants.syncQueryEntity = Constants.syncSourceQueryEntity + "('" + Constants.TrackingId + "')";

                quotesResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, Constants.QuoteEntity,
                            Constants.syncQueryEntity, null,
                            Constants.AccessToken, parameters);
                if (quotesResponse != null && quotesResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuotes = await _serviceAgent.ConvertTosDataObject(quotesResponse);
                    if (Convert.ToInt32(sDataQuotes.GetNamedNumber("$totalResults")) > DataAccessUtils.QuotesTotalCount)
                        DataAccessUtils.QuotesTotalCount = Convert.ToInt32(sDataQuotes.GetNamedNumber("$totalResults"));
                    if (DataAccessUtils.QuotesTotalCount == 0)
                    {
                        _eventAggregator.GetEvent<QuoteDataChangedEvent>().Publish(true);
                    }
                    int _totalCount = Convert.ToInt32(sDataQuotes.GetNamedNumber("$totalResults"));
                    JsonArray quotesObject = sDataQuotes.GetNamedArray("$resources");
                    int _returnedCount = quotesObject.Count;
                    if (_returnedCount > 0 && _totalCount - _returnedCount >= 0 &&
                        !(DataAccessUtils.IsQuotesSyncCompleted))
                    {
                        JsonObject lastQuoteObject = quotesObject.GetObjectAt(Convert.ToUInt32(_returnedCount - 1));
                        digest.LastRecordId = lastQuoteObject.GetNamedString("$key");
                        int _syncEndpointTick = Convert.ToInt32(lastQuoteObject.GetNamedNumber("SyncTick"));
                        if (_syncEndpointTick > digest.localTick)
                        {
                            digest.localTick = _syncEndpointTick;
                        }

                        // Saves Quotes data into LocalDB
                        await _quoteRepository.SaveQuotesAsync(sDataQuotes, digest);
                        // Looping this method again to make request for next batch of records(Quotes).
                        await SyncQuotes();
                    }
                    else
                    {
                        DataAccessUtils.QuotesTotalCount = 0;
                        DataAccessUtils.QuotesReturnedCount = 0;
                        DataAccessUtils.IsQuotesSyncCompleted = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Converts quote, shipping address to json formatted object
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItemList"></param>
        /// <returns></returns>
        private QuoteDetailsShippingAddressJson ConvertQuoteWithShippingAddressToJsonFormattedObject(Quote quote,
            List<QuoteLineItem> quoteLineItemList, Address address)
        {
            var quoteJsonObject = new QuoteDetailsShippingAddressJson();
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            //quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SubTotal = quote.SubTotal;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            //quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.CustomerId = quote.CustomerId;

            //quoteJsonObject.Details = new QuoteDetailsJson();

            quoteJsonObject.Details = new List<Detail>();
            Detail detail;

            // this is not required
            if (quoteLineItemList != null)
            {
                //check with quote.Amount > 0
                if (quoteLineItemList.Count > 0)
                {
                    foreach (QuoteLineItem quoteLineItem in quoteLineItemList)
                    {
                        detail = new Detail();
                        detail.InventoryItemId = quoteLineItem.ProductId;
                        detail.Price = quoteLineItem.Price;
                        detail.Quantity = quoteLineItem.Quantity;
                        quoteJsonObject.Details.Add(detail);
                    }
                }
            }


            quoteJsonObject.ShippingAddress = new ShippingAddressJson();
            //Address address = await _addressRepository.GetPendingShippingAddress(quote.CustomerId);

            quoteJsonObject.ShippingAddress.Name = address.AddressName == null ? "" : address.AddressName;
            quoteJsonObject.ShippingAddress.City = address.City;
            quoteJsonObject.ShippingAddress.Country = address.Country == null ? "" : address.Country;
            quoteJsonObject.ShippingAddress.Email = address.Email == null ? "" : address.Email;
            quoteJsonObject.ShippingAddress.Phone = address.Phone == null ? "" : address.Phone;
            quoteJsonObject.ShippingAddress.PostalCode = address.PostalCode == null ? "" : address.PostalCode;
            quoteJsonObject.ShippingAddress.StateProvince = address.StateProvince == null ? "" : address.StateProvince;
            quoteJsonObject.ShippingAddress.Street1 = address.Street1 == null ? "" : address.Street1;
            quoteJsonObject.ShippingAddress.Street2 = address.Street2 == null ? "" : address.Street2;
            quoteJsonObject.ShippingAddress.Street3 = address.Street3 == null ? "" : address.Street3;
            quoteJsonObject.ShippingAddress.Street4 = address.Street4 == null ? "" : address.Street4;
            //quoteJsonObject.ShippingAddress.Type = address.AddressType == null ? "" : address.AddressType;
            quoteJsonObject.ShippingAddress.Customer = new CustomerId { key = quote.CustomerId };
            return quoteJsonObject;
        }

        /// <summary>
        ///     Converts quote, shipping address key to json formatted object
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItemList"></param>
        /// <returns></returns>
        private QuoteDetailsShippingAddressKeyJson ConvertQuoteWithShippingAddressKeyToJsonFormattedObject(Quote quote,
            List<QuoteLineItem> quoteLineItemList)
        {
            var quoteJsonObject = new QuoteDetailsShippingAddressKeyJson();
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SubTotal = quote.SubTotal;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            //quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.CustomerId = quote.CustomerId;

            quoteJsonObject.ShippingAddressId = quote.AddressId;
            //quoteJsonObject.Details = new QuoteDetailsJson();

            quoteJsonObject.Details = new List<Detail>();
            Detail detail;

            // ispending, isdeleted, pending prefix, quantiy> 0
            if (quoteLineItemList != null)
            {
                if (quoteLineItemList.Count > 0)
                {
                    foreach (QuoteLineItem quoteLineItem in quoteLineItemList)
                    {
                        if (quoteLineItem.Quantity > 0)
                        {
                            detail = new Detail();
                            detail.InventoryItemId = quoteLineItem.ProductId;
                            detail.Price = quoteLineItem.Price;
                            detail.Quantity = quoteLineItem.Quantity;
                            quoteJsonObject.Details.Add(detail);
                        }
                    }
                }
            }
            return quoteJsonObject;
        }


        private ShippingAddressJson GetShippingAddressObj(string customerId, Address address)
        {

            var ShippingAddress = new ShippingAddressJson();

            ShippingAddress.Name = address.AddressName == null ? "" : address.AddressName;
            ShippingAddress.City = address.City;
            ShippingAddress.Country = address.Country == null ? "" : address.Country;
            ShippingAddress.Email = address.Email == null ? "" : address.Email;
            ShippingAddress.Phone = address.Phone == null ? "" : address.Phone;
            ShippingAddress.PostalCode = address.PostalCode == null ? "" : address.PostalCode;
            ShippingAddress.StateProvince = address.StateProvince == null ? "" : address.StateProvince;
            ShippingAddress.Street1 = address.Street1 == null ? "" : address.Street1;
            ShippingAddress.Street2 = address.Street2 == null ? "" : address.Street2;
            ShippingAddress.Street3 = address.Street3 == null ? "" : address.Street3;
            ShippingAddress.Street4 = address.Street4 == null ? "" : address.Street4;
            ShippingAddress.Type = address.AddressType == null ? "" : address.AddressType;
            ShippingAddress.Customer = new CustomerId { key = customerId };

            return ShippingAddress;
        }

        /// <summary>
        ///     Converts quote to json formatted object for Submit Quote Request(payload)
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItemList"></param>
        /// <returns></returns>
        private SubmitQuoteJson ConvertSubmitQuoteToJsonFormattedObject(Quote quote,
            List<QuoteLineItem> quoteLineItemList)
        {
            var quoteJsonObject = new SubmitQuoteJson();
            quoteJsonObject.key = quote.QuoteId;
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            //quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SubTotal = quote.SubTotal;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            //quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.CustomerId = quote.CustomerId;
            //quoteJsonObject.SalesRep = new SalesKeyJson { key = quote.RepId };

            quoteJsonObject.ShippingAddressId = quote.AddressId;
            //quoteJsonObject.Details = new QuoteDetailsJson();

            quoteJsonObject.Details = new List<Detail>();
            Detail detail;

            // ispending, isdeleted, pending prefix, quantiy> 0
            if (quoteLineItemList != null)
            {
                if (quoteLineItemList.Count > 0)
                {
                    foreach (QuoteLineItem quoteLineItem in quoteLineItemList)
                    {
                        if (quoteLineItem.Quantity > 0)
                        {
                            detail = new Detail();
                            detail.InventoryItemId = quoteLineItem.ProductId;
                            detail.Price = quoteLineItem.Price;
                            detail.Quantity = quoteLineItem.Quantity;
                            quoteJsonObject.Details.Add(detail);
                        }
                    }
                }
            }
            return quoteJsonObject;
        }

        #endregion
    }
}