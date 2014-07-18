using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.JsonHelpers;
using SQLite;

namespace SageMobileSales.ServiceAgents.Services
{
    public class QuoteLineItemService : IQuoteLineItemService
    {
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private readonly IServiceAgent _serviceAgent;
        private string _log = string.Empty;
        private Dictionary<string, string> parameters = null;

        public QuoteLineItemService(IServiceAgent serviceAgent, IQuoteRepository quoteRepository,
            IQuoteLineItemRepository quoteLineItemRepository, IQuoteService quoteService)
        {
            _serviceAgent = serviceAgent;
            _quoteService = quoteService;
            _quoteRepository = quoteRepository;
            _quoteLineItemRepository = quoteLineItemRepository;
        }

        #region public methods

        /// <summary>
        ///     Sync all QuoteLineItems for Quote
        /// </summary>
        /// <param name="quoteId"></param>
        /// <returns></returns>
        public async Task SyncQuoteLineItems(string quoteId)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                parameters.Add("Include", "Customer/Addresses,Details");

                string quoteEntityId = Constants.QuoteEntity + "('" + quoteId + "')";
                HttpResponseMessage quoteLineItemResponse = null;
                quoteLineItemResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, quoteEntityId, null, null, Constants.AccessToken, parameters);
                if (quoteLineItemResponse != null && quoteLineItemResponse.IsSuccessStatusCode)
                {
                    var sDataQuoteLineItem = await _serviceAgent.ConvertTosDataObject(quoteLineItemResponse);
                    //Saving or updating Quote, QuoteLineItem, Address table
                    await _quoteRepository.SaveQuoteAsync(sDataQuoteLineItem);
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
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
        ///     Adds quoteLineItem via put(http) request
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        public async Task AddQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                parameters.Add("include", "Details,ShippingAddress");
                object obj;

                obj = ConvertQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItem);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;


                quoteEntityId = Constants.QuoteEntity + "('" + quote.QuoteId + "')";

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPutObjectRequest(quoteEntityId, null, Constants.AccessToken, parameters,
                            obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    var sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote.QuoteId, null, quoteLineItem);
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
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
        ///     Edit quoteLineItem via put(http) request
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        public async Task EditQuoteLineItem(Quote quote, QuoteLineItem quoteLineItem)
        {
            try
            {
                parameters = new Dictionary<string, string>();
                parameters.Add("include", "Details,ShippingAddress");
                object obj;

                obj = ConvertEditedQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItem);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;

                quoteEntityId = Constants.QuoteEntity + "('" + quote.QuoteId + "')";

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPutObjectRequest(quoteEntityId, null, Constants.AccessToken, parameters,
                            obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    var sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote.QuoteId, null, quoteLineItem);
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
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
        ///     Deletes quoteLineItem via delete(http) request
        /// </summary>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        //Need to confirm whether is it quotelineItemId or quoteId
        public async Task DeleteQuoteLineItem(string quoteLineItemId)
        {
            try
            {
                HttpResponseMessage quoteResponse = null;
                string quoteDetailEntityId;

                quoteDetailEntityId = Constants.QuoteDetailEntity + "('" + quoteLineItemId + "')";

                quoteResponse =
                    await
                        _serviceAgent.BuildAndDeleteRequest(quoteDetailEntityId, null, Constants.AccessToken, parameters);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    await _quoteLineItemRepository.DeleteQuoteLineItemFromDbAsync(quoteLineItemId);
                }
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (HttpRequestException ex)
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
        ///     Sync pending quoteLineItems
        /// </summary>
        /// <returns></returns>
        public async Task SyncOfflineQuoteLineItems()
        {
            try
            {
                List<QuoteLineItem> quoteLineItemPendingList = await _quoteLineItemRepository.GetPendingQuoteLineItems();

                foreach (QuoteLineItem quoteLineItem in quoteLineItemPendingList)
                {
                    Quote quote = await _quoteRepository.GetQuoteAsync(quoteLineItem.QuoteId);
                    if (quote != null)
                    {
                        if (quote.QuoteId.Contains(Constants.Pending))
                        {
                            await _quoteService.PostQuote(quote);
                        }
                        else
                        {
                            if (quoteLineItem.QuoteLineItemId.Contains(Constants.Pending))
                            {
                                await AddQuoteLineItem(quote, quoteLineItem);
                            }
                            else
                            {
                                await EditQuoteLineItem(quote, quoteLineItem);
                            }
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
        }

        /// <summary>
        ///     Sync deleted quoteLineItems
        /// </summary>
        /// <returns></returns>
        public async Task SyncOfflineDeletedQuoteLineItems()
        {
            try
            {
                List<QuoteLineItem> quoteLineItemDeleteList = await _quoteLineItemRepository.GetDeletedQuoteLineItems();
                foreach (QuoteLineItem quoteLineItem in quoteLineItemDeleteList)
                {
                    await DeleteQuoteLineItem(quoteLineItem.QuoteLineItemId);
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
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Converts quote details, shipping address key to json formatted object
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        private QuoteDetailsShippingAddressKeyJson ConvertQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(
            Quote quote, QuoteLineItem quoteLineItem)
        {
            var quoteJsonObject = new QuoteDetailsShippingAddressKeyJson();
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.Customer = new CustomerKeyJson { key = quote.CustomerId };
            quoteJsonObject.SalesRep = new SalesKeyJson { key = quote.RepId };

            quoteJsonObject.ShippingAddress = new ShippingAddressKeyJson { key = quote.AddressId };
            quoteJsonObject.Details = new QuoteDetailsJson();

            quoteJsonObject.Details.resources = new List<Resource>();
            Resource resource;

            if (quoteLineItem != null)
            {
                resource = new Resource();
                resource.InventoryItem = new InventoryItemKeyJson { key = quoteLineItem.ProductId };
                resource.Price = quoteLineItem.Price;
                resource.Quantity = quoteLineItem.Quantity;
                quoteJsonObject.Details.resources.Add(resource);
            }

            return quoteJsonObject;
        }

        /// <summary>
        ///     Converts edited quote details, shipping address key to json formatted object
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="quoteLineItem"></param>
        /// <returns></returns>
        private EditQuoteDetailsShippingAddressKeyJson
            ConvertEditedQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(Quote quote,
                QuoteLineItem quoteLineItem)
        {
            var quoteJsonObject = new EditQuoteDetailsShippingAddressKeyJson();
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.Customer = new CustomerKeyJson { key = quote.CustomerId };
            quoteJsonObject.SalesRep = new SalesKeyJson { key = quote.RepId };

            quoteJsonObject.ShippingAddress = new ShippingAddressKeyJson { key = quote.AddressId };
            quoteJsonObject.Details = new EditQuoteDetailsJson();

            quoteJsonObject.Details.resources = new List<ResourceKey>();
            ResourceKey resource;

            if (quoteLineItem != null)
            {
                resource = new ResourceKey();
                resource.key = quoteLineItem.QuoteLineItemId;
                resource.InventoryItem = new InventoryItemKeyJson { key = quoteLineItem.ProductId };
                resource.Price = quoteLineItem.Price;
                resource.Quantity = quoteLineItem.Quantity;
                quoteJsonObject.Details.resources.Add(resource);
            }

            return quoteJsonObject;
        }

        #endregion
    }
}