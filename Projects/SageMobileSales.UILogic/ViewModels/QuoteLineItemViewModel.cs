using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Services;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace SageMobileSales.UILogic.ViewModels
{
    [DataContract]
    public class QuoteLineItemViewModel : ViewModel
    {
        private readonly string _imageUri;
        private readonly string _lineItemId;
        private readonly decimal _lineItemPrice;
        private readonly INavigationService _navigationService;
        private readonly string _productId;
        private readonly string _productName;
        private readonly int _productQuantity;
        private readonly string _productSku;
        private readonly string _quoteId;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteLineItemService _quoteLineItemService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private int _enteredQuantity;
        private bool _isCancelled;
        private bool _isEnabled;
        private string _lineItemQuantity;
        private string _log = string.Empty;
        private Quote _quote;

        public QuoteLineItemViewModel(INavigationService navigationService, LineItemDetails quoteLineItemDetails,
            IQuoteService quoteService, IQuoteRepository quoteRepository, IQuoteLineItemService quoteLineItemService,
            IQuoteLineItemRepository quoteLineItemRepository)
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
            _lineItemQuantity = quoteLineItemDetails.LineItemQuantity.ToString();
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

        public string LineItemQuantity
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
            get { return Math.Round(Convert.ToInt32(LineItemQuantity)*_lineItemPrice, 2); }
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
                if (!string.IsNullOrEmpty(LineItemQuantity))
                {
                    var lineItemQnty = Convert.ToInt32(LineItemQuantity);

                    if (lineItemQnty > 0)
                    {
                        lineItemQnty -= 1;
                        LineItemQuantity = lineItemQnty.ToString();
                        await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
                    }
                }
                //if (LineItemQuantity > 0)
                //{
                //    LineItemQuantity -= 1;
                //    await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
                //}
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
                if (!string.IsNullOrEmpty(LineItemQuantity))
                {
                    var lineItemQnty = Convert.ToInt32(LineItemQuantity);
                    lineItemQnty += 1;
                    LineItemQuantity = lineItemQnty.ToString();
                    await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
                }
                // LineItemQuantity += 1;
                // await UpdateQuoteLineItemToLocalDB(sender as QuoteLineItemViewModel);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     TextChanged event to get entered quantity
        /// </summary>
        /// <param name="args"></param>
        public void QunatityTextBoxGotFocus(object sender, object parameter)
        {
            var quantity = (TextBox) (parameter as RoutedEventArgs).OriginalSource;

            if (quantity.Text != null && (quantity.Text != string.Empty))
            {
                var enteredQnty = Convert.ToInt32(quantity.Text.Trim());
                if (enteredQnty > 0)
                {
                    //QuoteLineItemViewModel obj=sender as QuoteLineItemViewModel;
                    // obj.LineItemQuantity = enteredQnty.ToString();
                    var quotelineItemObj = sender as QuoteLineItemViewModel;
                    if (quotelineItemObj != null)
                    {
                        LineItemQuantity = quotelineItemObj.LineItemQuantity;
                        // await UpdateQuoteLineItemToLocalDB(quotelineItemObj);
                    }
                }
                else
                {
                    LineItemQuantity = string.Empty;
                }
            }
            else
            {
                LineItemQuantity = string.Empty;
            }
        }

        /// <summary>
        ///     TextChanged event to get Lineitem quantity
        /// </summary>
        /// <param name="args"></param>
        public async void QunatityTextBoxLostFocus(object sender, object parameter)
        {
            var quantity = (TextBox) (parameter as RoutedEventArgs).OriginalSource;

            if (quantity.Text != null && (quantity.Text != string.Empty))
            {
                LineItemQuantity = quantity.Text.Trim();
                var quotelineItemObj = sender as QuoteLineItemViewModel;
                if (quotelineItemObj != null)
                {
                    LineItemQuantity = quotelineItemObj.LineItemQuantity;
                    await UpdateQuoteLineItemToLocalDB(quotelineItemObj);
                }
            }
            else
            {
                LineItemQuantity = "0";
            }
        }

        /// <summary>
        ///     LostFocus event to get lineitem quantity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void QuanityTextChanged(object sender, object parameter)
        {
            try
            {
                var quotelineItemObj = sender as QuoteLineItemViewModel;
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
                var selectedQuoteLineItem =
                    await _quoteLineItemRepository.GetQuoteLineAsync(selectedItem.LineItemId);
                selectedQuoteLineItem.Quantity = Convert.ToInt32(selectedItem.LineItemQuantity);
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
                var quoteLineItemsList = await _quoteLineItemRepository.GetQuoteLineItemDetailsAsync(_quoteId);
                foreach (var lineItem in quoteLineItemsList)
                {
                    subTotal += lineItem.Amount;
                }
                if (quoteStatus != string.Empty)
                {
                    _quote.QuoteStatus = quoteStatus;
                }
                _quote.SubTotal = subTotal;
                var discountPercentageValue = Math.Round(((_quote.DiscountPercent/100)*subTotal), 2);
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

        /// Callback function for the invocation of the dialog commands.
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