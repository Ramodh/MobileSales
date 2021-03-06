﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.ServiceAgents.Common;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace SageMobileSales.UILogic.ViewModels
{
    [DataContract]
    public class QuoteLineItemViewModel : ViewModel
    {
        private readonly string _imageUri;
        private readonly string _lineItemId;
        private readonly decimal _lineItemPrice;
        private readonly INavigationService _navigationService;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteLineItemService _quoteLineItemService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private Quote _quote;
        private readonly string _productId;
        private readonly string _productName;
        private readonly int _productQuantity;
        private readonly string _productSku;
        private readonly string _quoteId;
        private int _lineItemQuantity;
        private int _enteredQuantity;
        private bool _isEnabled;
        private bool _isCancelled;
        private string _log = string.Empty;


        public QuoteLineItemViewModel(INavigationService navigationService, LineItemDetails quoteLineItemDetails, IQuoteService quoteService, IQuoteRepository quoteRepository, IQuoteLineItemService quoteLineItemService, IQuoteLineItemRepository quoteLineItemRepository)
        {
            _navigationService = navigationService;
            _quoteService = quoteService;
            _quoteLineItemService = quoteLineItemService;
            _quoteRepository = quoteRepository;
            _quoteLineItemRepository = quoteLineItemRepository;

            if (quoteLineItemDetails == null)
            {
                throw new ArgumentNullException("quoteLineItem", "quoteLineItem cannot be null");
            }

            _lineItemId = quoteLineItemDetails.LineItemId;
            _lineItemQuantity = quoteLineItemDetails.LineItemQuantity;
            _lineItemPrice = quoteLineItemDetails.LineItemPrice;
            _imageUri = quoteLineItemDetails.Url;
            _productName = quoteLineItemDetails.ProductName;
            _productQuantity = quoteLineItemDetails.ProductQuantity;
            _productSku = quoteLineItemDetails.ProductSku;
            _quoteId = quoteLineItemDetails.LineId;
            _productId = quoteLineItemDetails.ProductId;
        }

        public int EnteredQuantity
        {
            get { return _enteredQuantity; }
            private set
            {
                SetProperty(ref _enteredQuantity, value);
                OnPropertyChanged("EnteredQuantity");
            }

        }

        public string QuoteId
        {
            get { return _quoteId; }
        }

        public string CustomerId { get; set; }

        public string ProductId
        {
            get { return _productId; }
        }

        public string ProductName
        {
            get { return _productName != null ? _productName.Trim() : string.Empty; }
        }

        public string LineItemId
        {
            get { return _lineItemId; }
        }

        public decimal LineItemPrice
        {
            get { return _lineItemPrice; }
        }

        public int LineItemQuantity
        {
            get { return _lineItemQuantity; }
            set
            {
                if (SetProperty(ref _lineItemQuantity, value))
                {
                    OnPropertyChanged("Amount");
                    OnPropertyChanged("LineItemQuantity");
                    //OnPropertyChanged("FullPriceDouble");
                    //OnPropertyChanged("DiscountedPriceDouble");
                }
            }
        }

        public string ImageUri
        {
            get { return _imageUri; }
        }

        public decimal Amount
        {
            get { return Math.Round(LineItemQuantity * _lineItemPrice, 2); }
        }

        //public decimal Total
        //{
        //    get { return Math.Round(LineItemQuantity * _lineItemPrice, 2); }
        //}

        public int ProductQuantity
        {
            get { return _productQuantity; }
        }

        public string ProductSku
        {
            get { return _productSku; }
        }

        public bool IsEnabled
        {

            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        //public string FullPrice
        //{
        //    get { return _currencyFormatter.FormatDouble(FullPriceDouble); }
        //}

        //public double DiscountPercentage
        //{
        //    get { return _discountPercentage; }
        //}

        //public ImageSource Image
        //{
        //    get { return new BitmapImage(_imageUri); }
        //}

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void RecentOrderClick(object sender, object parameter)
        {
            try
            {
                var lineItemDtls = sender as QuoteLineItemViewModel;
                if (lineItemDtls != null)
                {
                    _navigationService.Navigate("RecentOrders", lineItemDtls);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        public async void DecrementQuantity(object sender, object parameter)
        {
            try
            {
                if (LineItemQuantity > 0)
                {
                    LineItemQuantity -= 1;
                    await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        public async void IncrementQuantity(object sender, object parameter)
        {
            try
            {
                LineItemQuantity += 1;
                await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// LostFocus event to get entered quantity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void QuanityTextChanged(object sender, object parameter)
        {
            try
            {
                QuoteLineItemViewModel quotelineItemObj = sender as QuoteLineItemViewModel;
                if (quotelineItemObj != null)
                {
                    LineItemQuantity = quotelineItemObj.LineItemQuantity;
                    await UpdateQuoteLineItemToLocalDB(quotelineItemObj);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        public async void DeleteQuoteLineItem(object sender, object parameter)
        {
            try
            {
                await
              ShowMessageDialog(
                  ResourceLoader.GetForCurrentView("Resources").GetString("DeleteQuoteLineItemMessageDialog"),
                  string.Empty, false, false);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async Task UpdateQuoteLineItemToLocalDB(QuoteLineItemViewModel selectedItem)
        {
            try
            {


                QuoteLineItem selectedQuoteLineItem =
                  await _quoteLineItemRepository.GetQuoteLineAsync(selectedItem.LineItemId);
                selectedQuoteLineItem.Quantity = selectedItem.LineItemQuantity;
                await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(selectedQuoteLineItem);
                _quote = await UpdateQuoteToLocalDB(string.Empty);

                //if (_quote != null && Constants.ConnectedToInternet())
                //{
                //    await _quoteLineItemService.EditQuoteLineItem(_quote, selectedQuoteLineItem);
                //}
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async Task<Quote> UpdateQuoteToLocalDB(string quoteStatus)
        {
            try
            {
                _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
                decimal subTotal = 0;
                List<LineItemDetails> quoteLineItemsList = await _quoteLineItemRepository.GetQuoteLineItemDetailsAsync(_quoteId);
                foreach (LineItemDetails lineItem in quoteLineItemsList)
                {
                    subTotal += lineItem.Amount;
                }
                if (quoteStatus != string.Empty)
                {
                    _quote.QuoteStatus = quoteStatus;
                }
                _quote.SubTotal = subTotal;
                decimal discountPercentageValue = Math.Round(((_quote.DiscountPercent / 100) * subTotal), 2);
                _quote.Amount = subTotal - discountPercentageValue + _quote.ShippingAndHandling + _quote.Tax;
                _quote = await _quoteRepository.UpdateQuoteToDbAsync(_quote);
                if (quoteStatus != string.Empty && quoteStatus == DataAccessUtils.DraftQuote)
                {
                    await _quoteService.RevertSubmittedQuoteToDraft(_quote);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _quote;
        }

        /// <summary>
        ///     Displays Messgae Dialog
        /// </summary>
        private async Task ShowMessageDialog(string messageText, string messageTitle, bool deleteQuote,
            bool isChangeStatus)
        {
            var messageDialog = new MessageDialog(messageText, messageTitle);

            // Add commands and set their command ids
            messageDialog.Commands.Add(
                new UICommand(ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogCancelbuttonText"),
                    command => { _isCancelled = true; }, 0));

            messageDialog.Commands.Add(
                new UICommand(
                    ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogChangeStatusbuttonText"),
                    RevertQuoteStatusCommandInvokedHandler, 1));

            //messageDialog.Commands.Add(
            //    new UICommand(ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogDeletebuttonText"),
            //        DeleteQuoteLineItemCommandInvokedHandler, 1));


            // Set the command that will be invoked by default
            // messageDialog.DefaultCommandIndex = 1;

            // Show the message dialog and get the event that was invoked via the async operator
            await messageDialog.ShowAsync();
        }

        ///     Callback function for the invocation of the dialog commands.

        private async void RevertQuoteStatusCommandInvokedHandler(IUICommand command)
        {
            try
            {
                _quote = await UpdateQuoteToLocalDB(DataAccessUtils.DraftQuote);
                IsEnabled = false;
                OnPropertyChanged("IsEnabled");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }
}