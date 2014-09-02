using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
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
        private Dictionary<string, string> parameters;

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
        public async Task SyncQuoteLineItems(Quote quote)
        {
            try
            {
                parameters = new Dictionary<string, string>();

                List<QuoteLineItem> quoteLineItemList =
                    await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);
                if (quoteLineItemList != null && quoteLineItemList.Count > 0)
                {
                    parameters.Add("Include", "Details,Customer/Addresses");
                }
                else
                {
                    parameters.Add("include",
                        "Details,Customer/Addresses,Details/InventoryItem,Details/InventoryItem/Images");
                }

                string quoteEntityId = Constants.QuoteDetailEntity + "('" + quote.QuoteId + "')";
                HttpResponseMessage quoteLineItemResponse = null;
                quoteLineItemResponse =
                    await
                        _serviceAgent.BuildAndSendRequest(Constants.TenantId, quoteEntityId, null, null,
                            Constants.AccessToken, parameters);
                if (quoteLineItemResponse != null && quoteLineItemResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuoteLineItem = await _serviceAgent.ConvertTosDataObject(quoteLineItemResponse);

                    //Saving or updating Quote, QuoteLineItem, Address table
                    await _quoteRepository.SaveQuoteAsync(sDataQuoteLineItem);

                    //await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuoteLineItem, quote, null, null);
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
                //parameters.Add("include", "Details");
                object obj;

                obj = ConvertQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItem);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;


                quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

                //quoteResponse =
                //    await
                //        _serviceAgent.BuildAndPutObjectRequest(quoteEntityId, null, Constants.AccessToken, parameters,
                //            obj);

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                            Constants.AccessToken, parameters,
                            obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, quoteLineItem);
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
                parameters.Add("include", "Details");
                object obj;

                obj = ConvertEditedQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(quote, quoteLineItem, false);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;

                quoteEntityId = Constants.DraftQuotes + "('" + quote.QuoteId + "')";

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                            Constants.AccessToken, parameters,
                            obj);

                //quoteResponse =
                //    await
                //        _serviceAgent.BuildAndPutObjectRequest(quoteEntityId, null, Constants.AccessToken, parameters,
                //            obj);
                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    JsonObject sDataQuote = await _serviceAgent.ConvertTosDataObject(quoteResponse);
                    await _quoteRepository.SavePostedQuoteToDbAsync(sDataQuote, quote, null, quoteLineItem);
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
        public async Task DeleteQuoteLineItem(QuoteLineItem quoteLineItem)
        {
            try
            {
                //HttpResponseMessage quoteResponse = null;
                //string EntityId;

                //EntityId = Constants.QuoteDetailEntity + "('" + quoteLineItemId + "')";

                //quoteResponse =
                //    await
                //        _serviceAgent.BuildAndDeleteRequest(Constants.TenantId, EntityId, null, Constants.AccessToken, parameters);

                parameters = new Dictionary<string, string>();
                parameters.Add("include", "Details");
                object obj;

                obj =
                    ConvertEditedQuoteDetailsWithShippingAddressKeyToJsonFormattedObject(
                        await _quoteRepository.GetQuoteAsync(quoteLineItem.QuoteId), quoteLineItem, true);

                HttpResponseMessage quoteResponse = null;
                string quoteEntityId;

                quoteEntityId = Constants.DraftQuotes + "('" + quoteLineItem.QuoteId + "')";

                quoteResponse =
                    await
                        _serviceAgent.BuildAndPatchObjectRequest(Constants.TenantId, quoteEntityId, null,
                            Constants.AccessToken, parameters,
                            obj);

                if (quoteResponse != null && quoteResponse.IsSuccessStatusCode)
                {
                    await _quoteLineItemRepository.DeleteQuoteLineItemFromDbAsync(quoteLineItem.QuoteLineItemId);
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
                            await _quoteService.PostDraftQuote(quote);
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
                    await DeleteQuoteLineItem(quoteLineItem);
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
            //quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            //quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.CustomerId = quote.CustomerId;
            //quoteJsonObject.SalesRep = new SalesKeyJson { key = quote.RepId };

            quoteJsonObject.ShippingAddressId = quote.AddressId;
            //quoteJsonObject.Details = new QuoteDetailsJson();

            quoteJsonObject.Details = new List<Detail>();
            Detail detail;

            if (quoteLineItem != null)
            {
                detail = new Detail();
                detail.InventoryItemId = quoteLineItem.ProductId;
                detail.Price = quoteLineItem.Price;
                detail.Quantity = quoteLineItem.Quantity;
                quoteJsonObject.Details.Add(detail);
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
                QuoteLineItem quoteLineItem, bool isDeleted)
        {
            var quoteJsonObject = new EditQuoteDetailsShippingAddressKeyJson();
            quoteJsonObject.Description = quote.QuoteDescription == null ? "" : quote.QuoteDescription;
            //quoteJsonObject.DiscountPercent = quote.DiscountPercent;
            quoteJsonObject.QuoteTotal = quote.Amount;
            quoteJsonObject.SandH = quote.ShippingAndHandling;
            //quoteJsonObject.Status = quote.QuoteStatus;
            quoteJsonObject.CustomerId = quote.CustomerId;
            //quoteJsonObject.SalesRep = new SalesKeyJson { key = quote.RepId };

            quoteJsonObject.ShippingAddressId = quote.AddressId;
            quoteJsonObject.Details = new List<EditDetail>();
            EditDetail detail;
            //quoteJsonObject.Details.resources = new List<ResourceKey>();
            //ResourceKey resource;

            if (quoteLineItem != null)
            {
                detail = new EditDetail();
                detail.key = quoteLineItem.QuoteLineItemId;
                detail.InventoryItemId = quoteLineItem.ProductId;
                detail.Price = quoteLineItem.Price;
                detail.Quantity = quoteLineItem.Quantity;

                if (isDeleted)
                    detail.IsDeleted = isDeleted;

                quoteJsonObject.Details.Add(detail);
            }

            return quoteJsonObject;
        }

        #endregion
    }
}