using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SageMobileSales.UILogic.Model;

namespace SageMobileSales.UILogic.ViewModels
{
    public class QuoteDetailsPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteLineItemService _quoteLineItemService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantService _tenantService;
        private Address _address;
        private CustomerDetails _customerDetails;
        private decimal _discountPercent;
        private bool _inProgress;

        private Visibility _isAddItemVisible;
        private bool _isBottomAppBarOpened;
        private bool _isCancelled;
        private Visibility _isChangeAddressVisible;
        private Visibility _isDeleteQuoteVisible;
        private bool _isDiscountEnabled;
        private Visibility _isEditQuoteLineItemVisible;
        private Visibility _isEditQuoteVisible;
        private Visibility _isPlaceOrderVisible;
        private Visibility _isSendmailVisible;
        private bool _isShippingAndHandlingEnabled;
        private Visibility _isSubmitQuoteVisible;
        private bool _itemNotSelected;
        private string _log = string.Empty;
        private Quote _quote;
        private QuoteDetails _quoteDetails;
        private string _quoteId;

        private ObservableCollection<QuoteLineItemViewModel> _quoteLineItemViewModels;
        private List<LineItemDetails> _quoteLineItemsList;
        private QuoteLineItemViewModel _selectedItem;
        private ShippingAddressDetails _shippingAddressDetails;
        private decimal _shippingAndHandling;
        private Tenant _tenant;
        private DataTransferManager dataTransferManager;

        public QuoteDetailsPageViewModel(INavigationService navigationService, IEventAggregator eventAggregator,
            IQuoteLineItemService quoteLineItemService, IQuoteLineItemRepository quoteLineItemRepository,
            IQuoteRepository quoteRepository,
            ICustomerRepository customerRepository, ITenantService tenantService, ITenantRepository tenantRepository,
            IQuoteService quoteService, ISalesRepRepository salesRepRepository, IAddressRepository addressRepository)
        {
            _navigationService = navigationService;
            _quoteLineItemService = quoteLineItemService;
            _quoteLineItemRepository = quoteLineItemRepository;
            _quoteRepository = quoteRepository;
            _customerRepository = customerRepository;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            _quoteService = quoteService;
            _salesRepRepository = salesRepRepository;
            _addressRepository = addressRepository;

            //IncrementCountCommand = DelegateCommand.FromAsyncHandler(IncrementCount);
            //DecrementCountCommand = DelegateCommand.FromAsyncHandler(DecrementCount, CanDecrementCount);
            AddItemCommand = DelegateCommand.FromAsyncHandler(NavigateToCatalogPage);
            SendMailCommand = DelegateCommand.FromAsyncHandler(SendQuoteDetailsMail);
            SubmitQuoteCommand = DelegateCommand.FromAsyncHandler(SubmitQuote);
            PlaceOrderCommand = DelegateCommand.FromAsyncHandler(PlaceOrder);
            DeleteQuoteCommand = DelegateCommand.FromAsyncHandler(DeleteQuote);
            DeleteQuoteLineItemCommand = DelegateCommand.FromAsyncHandler(DeleteQuoteLineItem);
            EditQuoteCommand = DelegateCommand.FromAsyncHandler(ChangeQuoteStatus);
            ShippingAndHandlingTextChangedCommand = new DelegateCommand<object>(ShippingAndHandlingTextChanged);
            DiscountTextChangedCommand = new DelegateCommand<object>(DiscountPercentageTextChanged);
            RecentOrdersCommand = new DelegateCommand(NavigateToRecentOrders);

            //IncrementCountCommand = DelegateCommand.FromAsyncHandler(IncrementCount);
            //DecrementCountCommand = DelegateCommand.FromAsyncHandler(DecrementCount);
            ////IsBottomAppBarOpened = false;
            //IsItemSelected = Visibility.Collapsed;
            //IsItemNotSelected = Visibility.Visible;
            if (eventAggregator != null)
            {
                eventAggregator.GetEvent<QuoteDetailsUpdatedEvent>().Subscribe(UpdateQuoteDetailsAsync);
            }
        }


        public List<LineItemDetails> QuoteLineItemsList
        {
            get { return _quoteLineItemsList; }
            private set { SetProperty(ref _quoteLineItemsList, value); }
        }

        public CustomerDetails CustomerDetails
        {
            get { return _customerDetails; }
            private set { SetProperty(ref _customerDetails, value); }
        }

        public QuoteDetails QuoteDetails
        {
            get { return _quoteDetails; }
            private set { SetProperty(ref _quoteDetails, value); }
        }

        public ShippingAddressDetails ShippingAddressDetails
        {
            get { return _shippingAddressDetails; }
            private set { SetProperty(ref _shippingAddressDetails, value); }
        }

        public Address Address
        {
            get { return _address; }
            private set { SetProperty(ref _address, value); }
        }

        public decimal ShippingAndHandling
        {
            get { return _shippingAndHandling; }
            set { SetProperty(ref _shippingAndHandling, value); }
        }

        public decimal DiscountPercent
        {
            get { return _discountPercent; }
            set { SetProperty(ref _discountPercent, value); }
        }

        public decimal DiscountPercentageValue
        {
            get { return CalculateDiscountPercent(); }
        }

        public decimal Total
        {
            get { return Math.Round(CalculateTotal(), 2); }
        }

        public decimal SubTotal
        {
            get
            {
                return Math.Round(CalculateSubTotal(), 2);
            }
        }

        public bool IsBottomAppBarOpened
        {
            get { return _isBottomAppBarOpened; }
            set
            {
                // We always fire the PropertyChanged event because the 
                // AppBar.IsOpen property doesn't notify when the property is set.
                // See http://go.microsoft.com/fwlink/?LinkID=288840

                if (_isBottomAppBarOpened == value)
                {
                    return;
                }
                _isBottomAppBarOpened = value;
                OnPropertyChanged("IsBottomAppBarOpened");
            }
        }

        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        /// <summary>
        ///     AddItem bottom app bar button visibility property
        /// </summary>
        public Visibility IsAddItemVisible
        {
            get { return _isAddItemVisible; }
            private set { SetProperty(ref _isAddItemVisible, value); }
        }

        /// <summary>
        ///     SubmitQuote bottom app bar button visibility property
        /// </summary>
        public Visibility IsSubmitQuoteVisible
        {
            get { return _isSubmitQuoteVisible; }
            private set { SetProperty(ref _isSubmitQuoteVisible, value); }
        }

        /// <summary>
        ///     PlaceOrder bottom appbar visibility property
        /// </summary>
        public Visibility IsPlaceOrderVisible
        {
            get { return _isPlaceOrderVisible; }
            private set { SetProperty(ref _isPlaceOrderVisible, value); }
        }

        public Visibility IsChangeAddressVisible
        {
            get { return _isChangeAddressVisible; }
            private set { SetProperty(ref _isChangeAddressVisible, value); }
        }

        public Visibility IsSendmailVisible
        {
            get { return _isSendmailVisible; }
            private set { SetProperty(ref _isSendmailVisible, value); }
        }

        public Visibility IsEditQuoteVisible
        {
            get { return _isEditQuoteVisible; }
            private set { SetProperty(ref _isEditQuoteVisible, value); }
        }

        public Visibility IsEditQuoteLineItemVisible
        {
            get { return _isEditQuoteLineItemVisible; }
            private set { SetProperty(ref _isEditQuoteLineItemVisible, value); }
        }

        public Visibility IsDeleteQuoteVisible
        {
            get { return _isDeleteQuoteVisible; }
            private set { SetProperty(ref _isDeleteQuoteVisible, value); }
        }

        public ObservableCollection<QuoteLineItemViewModel> QuoteLineItemViewModels
        {
            get { return _quoteLineItemViewModels; }
            private set
            {
                SetProperty(ref _quoteLineItemViewModels, value);
                InProgress = false;
            }
        }

        public bool IsShippingAndHandlingEnabled
        {
            get { return _isShippingAndHandlingEnabled; }
            private set { SetProperty(ref _isShippingAndHandlingEnabled, value); }
        }

        public bool IsDiscountEnabled
        {
            get { return _isDiscountEnabled; }
            private set { SetProperty(ref _isDiscountEnabled, value); }
        }

        public DelegateCommand IncrementCountCommand { get; private set; }

        public DelegateCommand DecrementCountCommand { get; private set; }

        public DelegateCommand CheckoutCommand { get; private set; }

        public DelegateCommand AddItemCommand { get; private set; }

        public DelegateCommand SendMailCommand { get; private set; }

        public DelegateCommand SubmitQuoteCommand { get; private set; }

        public DelegateCommand PlaceOrderCommand { get; private set; }

        public DelegateCommand DeleteQuoteCommand { get; private set; }

        public DelegateCommand DeleteQuoteLineItemCommand { get; private set; }

        public DelegateCommand EditQuoteCommand { get; private set; }

        public DelegateCommand<object> ShippingAndHandlingTextChangedCommand { get; private set; }

        public DelegateCommand<object> DiscountTextChangedCommand { get; private set; }

        public DelegateCommand RecentOrdersCommand { get; private set; }


        public QuoteLineItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (_selectedItem != null)
                    {
                        // Display the AppBar 
                        IsBottomAppBarOpened = true;
                        _itemNotSelected = false;
                    }
                    else
                    {
                        IsBottomAppBarOpened = false;
                        _itemNotSelected = true;
                    }

                    // changes app bar buttons visibility based on quote quote status
                    ChangeVisibility();
                }
            }
        }

        # region Delegate Command Methods

        private bool CanDecrementCount()
        {
            if (SelectedItem != null && SelectedItem.LineItemQuantity > 1)
            {
                return true;
            }
            return false;
        }

        # region Increment, Decrement Quote LineItems Code using Flyout
        /*
        private async Task DecrementCount()
        {
            try
            {
                //if (QuoteDetails.QuoteStatus == DataAccessUtils.SubmitQuote || QuoteDetails.QuoteStatus == "Quote")
                //{

                //    await ShowMessageDialog(ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogText"), ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogTitle"), false, true);
                //    return;
                //}
                // await _shoppingCartRepository.RemoveProductFromShoppingCartAsync(SelectedItem.ProductId);
                SelectedItem.LineItemQuantity -= 1;
                await UpdateQuote(string.Empty);
                QuoteLineItem SelectedQuoteLineItem =
                    await _quoteLineItemRepository.GetQuoteLineAsync(SelectedItem.LineItemId);
                SelectedQuoteLineItem.Quantity = SelectedItem.LineItemQuantity;
                await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(SelectedQuoteLineItem);
                if (_quote != null && Constants.ConnectedToInternet())
                {
                    await _quoteLineItemService.EditQuoteLineItem(_quote, SelectedQuoteLineItem);
                }
                DecrementCountCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async Task IncrementCount()
        {
            try
            {
                //if (QuoteDetails.QuoteStatus == DataAccessUtils.SubmitQuote || QuoteDetails.QuoteStatus == "Quote")
                //{

                //    await ShowMessageDialog(ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogText"), ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogTitle"), false, true);
                //    return;
                //}
                // await _shoppingCartRepository.AddProductToShoppingCartAsync(SelectedItem.ProductId);
                SelectedItem.LineItemQuantity += 1;
                await UpdateQuote(string.Empty);
                QuoteLineItem SelectedQuoteLineItem =
                    await _quoteLineItemRepository.GetQuoteLineAsync(SelectedItem.LineItemId);
                SelectedQuoteLineItem.Quantity = SelectedItem.LineItemQuantity;
                await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(SelectedQuoteLineItem);
                if (_quote != null && Constants.ConnectedToInternet())
                {
                    await _quoteLineItemService.EditQuoteLineItem(_quote, SelectedQuoteLineItem);
                }
                DecrementCountCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
        */

        # endregion

        private async Task NavigateToCatalogPage()
        {
            PageUtils.SelectedQuoteId = _quoteId;
            PageUtils.CamefromQuoteDetails = true;
            if (!_isCancelled)
            {
                _navigationService.Navigate("CategoryLevelOne", QuoteDetails);
            }
            _isCancelled = false;
        }

        private async Task SendQuoteDetailsMail()
        {
            try
            {
                // await _tenantService.SyncTenant();
                _tenant = await _tenantRepository.GetTenantDtlsAsync(Constants.TenantId);
                DataTransferManager.ShowShareUI();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Method to Update Quotestatus to Submit and sending the updated status to service.
        /// </summary>
        /// <returns></returns>
        private async Task SubmitQuote()
        {
            try
            {
                MessageDialog msgDialog;
                if (QuoteLineItemsList.Count > 0)
                {
                    InProgress = true;
                    _quote = await UpdateQuote(DataAccessUtils.SubmitQuote);

                    if (Constants.ConnectedToInternet())
                        _quote = await _quoteService.SubmitQuote(_quote);

                    InProgress = false;
                    //if (_quote.QuoteStatus == DataAccessUtils.SubmitQuote)
                    //{
                    //    IsSubmitQuote = Visibility.Collapsed;
                    //    IsPlaceOrder = Visibility.Visible;
                    //}
                    QuoteDetails.QuoteStatus = _quote.QuoteStatus;
                    OnPropertyChanged("QuoteDetails");

                    _itemNotSelected = true;
                    ChangeVisibility();
                    await DisplayQuotedetails();
                    msgDialog =
                        new MessageDialog(
                            ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogSubmittedQuoteText"),
                            ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogSubmittedQuoteTitle"));
                    msgDialog.Commands.Add(new UICommand("Ok"));

                    // _navigationService.GoBack();
                }
                else
                {
                    msgDialog =
                        new MessageDialog(
                            ResourceLoader.GetForCurrentView("Resources").GetString("SubmitQuoteErrorMessage"),
                            ResourceLoader.GetForCurrentView("Resources").GetString("SubmitQuoteErrorTitle"));
                    msgDialog.Commands.Add(new UICommand("Ok"));
                }
                await msgDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private async Task PlaceOrder()
        {
            if (_quote == null)
            {
                _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
            }

            _navigationService.Navigate("PlaceOrder", _quote);
        }

        /// <summary>
        ///     Deletes QuoteLineItem
        /// </summary>
        /// <returns></returns>
        private async Task DeleteQuoteLineItem()
        {
            await
                ShowMessageDialog(
                    ResourceLoader.GetForCurrentView("Resources").GetString("DeleteQuoteLineItemMessageDialog"),
                    string.Empty, false, false);
        }

        /// <summary>
        ///     Deletes Quote
        /// </summary>
        /// <returns></returns>
        private async Task DeleteQuote()
        {
            await
                ShowMessageDialog(ResourceLoader.GetForCurrentView("Resources").GetString("DeleteQuoteMessageDialog"),
                    string.Empty, true, false);
        }

        /// <summary>
        ///     Changes Quote status from Submit/Quote to draft.
        /// </summary>
        /// <returns></returns>
        private async Task ChangeQuoteStatus()
        {
            try
            {
                if (_quote == null)
                {
                    _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
                }
                if (_quote.QuoteStatus == "Quote")
                {
                    await
                        ShowMessageDialog(
                            ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogText"),
                            ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogTitle"), false,
                            true);
                }
                _itemNotSelected = true;
                ChangeVisibility();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Textchanged event to get entered ShippingAndHandling
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async void ShippingAndHandlingTextChanged(object args)
        {
            try
            {
                if (((TextBox)args).Text != null && ((TextBox)args).Text != string.Empty)
                {
                    _shippingAndHandling = Convert.ToDecimal(((TextBox)args).Text);
                }
                else
                {
                    _shippingAndHandling = 0;
                }
                OnPropertyChanged("Total");
                await UpdateQuote(string.Empty);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Textchanged event to get entered DiscountPercentage
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async void DiscountPercentageTextChanged(object args)
        {
            try
            {
                if (((TextBox)args).Text != null && ((TextBox)args).Text != string.Empty)
                {
                    SalesRep salesRep = (await _salesRepRepository.GetSalesRepDtlsAsync()).FirstOrDefault();

                    DiscountPercent = Convert.ToDecimal(((TextBox)args).Text);

                    if (DiscountPercent < 100)
                    {
                        if (salesRep.MaximumDiscountPercent != null)
                        {
                            if (DiscountPercent > Convert.ToDecimal(salesRep.MaximumDiscountPercent))
                            {
                                var maxDiscountMesDialog =
                                    new MessageDialog(
                                        ResourceLoader.GetForCurrentView("Resources")
                                            .GetString("MesDialogDiscountPercentageText"),
                                        ResourceLoader.GetForCurrentView("Resources")
                                            .GetString("MesDialogDiscountPercentageTitle"));
                                await maxDiscountMesDialog.ShowAsync();
                                DiscountPercent = Convert.ToDecimal(salesRep.MaximumDiscountPercent);
                            }
                        }
                        // DiscountPercentageValue = Math.Round(((_discountPercent / 100) * SubTotal), 2);
                    }
                }

                OnPropertyChanged("DiscountPercent");
                OnPropertyChanged("DiscountPercentageValue");
                OnPropertyChanged("Total");

                await UpdateQuote(string.Empty);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        public string RemoveSpecialCharacters(string str)
        {
            if (Regex.IsMatch(str, "[^0-9]+"))
            {
                return Regex.Replace(str, "[^0-9]+", "", RegexOptions.None);
            }
            return Regex.Replace(str, "[^0-9]+", "", RegexOptions.None);
        }


        private void OnDataRequested(DataTransferManager mgr, DataRequestedEventArgs args)
        {
            if (GetShareContent(args.Request))
            {
                if (String.IsNullOrEmpty(args.Request.Data.Properties.Title))
                {
                    //  e.Request.FailWithDisplayText(MainPage.MissingTitleError);
                }
            }
        }

        private bool GetShareContent(DataRequest request)
        {
            bool succeeded = false;
            var objMail = new MailViewModel();
            QuoteDetails.ShippingAndHandling = ShippingAndHandling;
            QuoteDetails.DiscountPercent = DiscountPercent;
            string HtmlContentString = objMail.BuildQuoteEmailContent(_tenant, CustomerDetails, QuoteDetails,
                QuoteLineItemsList, SubTotal.ToString(), Total.ToString());
            if (!String.IsNullOrEmpty(HtmlContentString))
            {
                DataPackage requestData = request.Data;
                requestData.Properties.Title = "Quote";
                requestData.Properties.Description = CustomerDetails.CustomerName; // The description is optional.
                //requestData.SetData(HtmlContentString,HtmlContentString);
                requestData.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(HtmlContentString));
                succeeded = true;
            }
            else
            {
                request.FailWithDisplayText("Enter the details you would like to share and try again.");
            }


            return succeeded;
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
            if (deleteQuote)
            {
                messageDialog.Commands.Add(
                    new UICommand(ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogDeletebuttonText"),
                        DeleteQuoteCommandInvokedHandler, 1));
            }
            else if (isChangeStatus)
            {
                messageDialog.Commands.Add(
                    new UICommand(
                        ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogChangeStatusbuttonText"),
                        RevertQuoteStatusCommandInvokedHandler, 1));
            }
            else
            {
                messageDialog.Commands.Add(
                    new UICommand(ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogDeletebuttonText"),
                        DeleteQuoteLineItemCommandInvokedHandler, 1));
            }

            // Set the command that will be invoked by default
            // messageDialog.DefaultCommandIndex = 1;

            // Show the message dialog and get the event that was invoked via the async operator
            await messageDialog.ShowAsync();
        }

        /// <summary>
        ///     Callback function for the invocation of the dialog commands.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        private async void DeleteQuoteCommandInvokedHandler(IUICommand command)
        {
            InProgress = true;
            if (QuoteDetails.QuoteId.Contains(PageUtils.Pending))
            {
                await _quoteRepository.DeleteQuoteFromDbAsync(QuoteDetails.QuoteId);
            }
            else
            {
                //Quote quote = await _quoteRepository.GetQuoteAsync(QuoteDetails.QuoteId);
                await _quoteRepository.MarkQuoteAsDeleted(QuoteDetails.QuoteId);

                if (Constants.ConnectedToInternet())
                {
                    //Delete quote
                    await _quoteService.DeleteQuote(QuoteDetails.QuoteId);
                }
            }

            InProgress = false;
            _navigationService.GoBack();
        }


        /// <summary>
        ///     Callback function for the invocation of the dialog commands.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        private async void DeleteQuoteLineItemCommandInvokedHandler(IUICommand command)
        {
            InProgress = true;
            QuoteLineItem selectedLineItem = await _quoteLineItemRepository.GetQuoteLineAsync(SelectedItem.LineItemId);
            //Quote quote = await _quoteRepository.GetQuoteAsync(selectedLineItem.QuoteId);
            //quote.Amount = Math.Round((quote.Amount - (selectedLineItem.Quantity * selectedLineItem.Price)), 2);
            //await _quoteRepository.UpdateQuoteToDbAsync(quote);
            //_quote = await UpdateQuote(string.Empty);

            if (selectedLineItem.QuoteLineItemId.Contains(PageUtils.Pending))
            {
                await _quoteLineItemRepository.DeleteQuoteLineItemFromDbAsync(selectedLineItem.QuoteLineItemId);
            }
            else
            {
                await _quoteLineItemRepository.MarkQuoteLineItemAsDeleted(selectedLineItem.QuoteLineItemId);

                if (Constants.ConnectedToInternet())
                {
                    //Delete quote line item via service
                    await _quoteLineItemService.DeleteQuoteLineItem(selectedLineItem);
                }
            }
            await DisplayQuotedetails();
            InProgress = false;
        }

        /// <summary>
        ///     Callback function for the invocation of the dialog commands.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        private async void RevertQuoteStatusCommandInvokedHandler(IUICommand command)
        {
            try
            {
                _quote = await UpdateQuote(DataAccessUtils.DraftQuote);
                await DisplayQuotedetails();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private void NavigateToRecentOrders()
        {
            try
            {
                //SalesHistory salesHistory=new SalesHistory();
                //salesHistory.CustomerId=CustomerDetails.CustomerName;
                //salesHistory.ProductId=
                //_navigationService.Navigate("",);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        # endregion

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            _quoteId = navigationParameter as string;
            //  await _addressRepository.GetShippingAddressForCustomer(QuoteDetails.CustomerId);

            var Frame = Window.Current.Content as Frame;

            if (Frame != null)
            {
                if (Frame.BackStack.Count >= 2)
                {
                    Frame.BackStack.RemoveAt(Frame.BackStack.Count - 1);
                }
            }

            // Register the current page as a share source.
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += OnDataRequested;

            QuoteDetails = await _quoteRepository.GetQuoteDetailsAsync(_quoteId);


            ShippingAddressDetails = await _addressRepository.GetShippingAddressDetails(QuoteDetails.AddressId);
            InProgress = true;
            await DisplayQuotedetails();

            //Process all pending tasks
            if (Constants.ConnectedToInternet())
                await ProcessPendingRequestsForQuote();

            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            if (dataTransferManager != null)
            {
                // Unregister the current page as a share source.
                dataTransferManager.DataRequested -= OnDataRequested;
            }
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        /// <summary>
        ///     Navigate to other Addresses screen on click of changeAddress app bar button
        /// </summary>
        public async void ChangeAddressButton_Click(object sender, object parameter)
        {
            try
            {
                //_quote = await _quoteRepository.GetQuoteAsync(QuoteDetails.QuoteId);

                //if (_quote.QuoteStatus == DataAccessUtils.SubmitQuote || _quote.QuoteStatus == "Quote")
                //{

                //    await ShowMessageDialog(ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogText"), ResourceLoader.GetForCurrentView("Resources").GetString("ChangeQuoteMesDialogTitle"), false, true);
                //}
                if (QuoteDetails != null)
                {
                    _navigationService.Navigate("OtherAddresses", QuoteDetails);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Gets Quotedetails data from localDB and binds to appropriate properties.
        /// </summary>
        private async Task DisplayQuotedetails()
        {
            try
            {
                if (Constants.ConnectedToInternet())
                {
                    await _quoteLineItemService.SyncQuoteLineItems(await _quoteRepository.GetQuoteAsync(_quoteId));
                }

                QuoteLineItemsList = await _quoteLineItemRepository.GetQuoteLineItemDetailsAsync(_quoteId);

                if (QuoteLineItemsList != null && QuoteLineItemsList.Count > 0)
                {
                    QuoteLineItemViewModels = new ObservableCollection<QuoteLineItemViewModel>();
                    foreach (LineItemDetails lineitem in QuoteLineItemsList)
                    {
                        var quoteLineItemViewModel = new QuoteLineItemViewModel(_navigationService, lineitem, _quoteService, _quoteRepository, _quoteLineItemService, _quoteLineItemRepository);
                        if (_quote == null)
                        {
                            _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
                        }
                        if (_quote.QuoteStatus == DataAccessUtils.DraftQuote)
                        {
                            quoteLineItemViewModel.IsEnabled = true;
                        }
                        else
                        {
                            quoteLineItemViewModel.IsEnabled = false;
                        }
                        quoteLineItemViewModel.PropertyChanged += quoteLineItemViewModel_PropertyChanged;
                        QuoteLineItemViewModels.Add(quoteLineItemViewModel);
                    }
                }

                else
                {
                    InProgress = false;
                    QuoteLineItemViewModels = null;
                }
                QuoteDetails = await _quoteRepository.GetQuoteDetailsAsync(_quoteId);

                ShippingAndHandling = QuoteDetails.ShippingAndHandling;
                DiscountPercent = QuoteDetails.DiscountPercent;
                //if (QuoteDetails.QuoteStatus == DataAccessUtils.DraftQuote)
                //{
                //    IsSubmitQuote = Visibility.Visible;
                //    IsPlaceOrder = Visibility.Collapsed;
                //}
                //else if (QuoteDetails.QuoteStatus == DataAccessUtils.SubmitQuote || QuoteDetails.QuoteStatus == "Quote")
                //{
                //    IsSubmitQuote = Visibility.Collapsed;
                //    IsPlaceOrder = Visibility.Visible;
                //}
                CustomerDetails =
                    await _customerRepository.GetCustomerDtlsForQuote(_quoteDetails.CustomerId, _quoteDetails.AddressId);

                _itemNotSelected = true;
                // changes app bar buttons visibility based on quote quote status
                ChangeVisibility();

                OnPropertyChanged("SubTotal");
                OnPropertyChanged("Total");
                OnPropertyChanged("ShippingAndHandling");
                OnPropertyChanged("DiscountPercent");
                OnPropertyChanged("DiscountPercentageValue");


                if (QuoteDetails.Amount > Total || QuoteDetails.Amount < Total)
                {
                    _quote = await UpdateQuote(string.Empty);
                }
                //MailViewModel objMail = new MailViewModel();
                //await objMail.ReadTextFile();               
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async void quoteLineItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LineItemQuantity")
            {
                OnPropertyChanged("Amount");
                OnPropertyChanged("SubTotal");
                OnPropertyChanged("Total");
            }
            //await UpdateQuote(string.Empty);    
        }

        public async void UpdateQuoteDetailsAsync(object notUsed)
        {
            await UpdateQuoteDetailsAsync();
        }

        private async Task UpdateQuoteDetailsAsync()
        {
            try
            {
                CheckoutCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SubTotal");
                //OnPropertyChanged("TotalDiscount");
                OnPropertyChanged("Total");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private decimal CalculateSubTotal()
        {
            decimal SubTotal = 0;
            if (_quoteLineItemViewModels != null)
            {
                foreach (QuoteLineItemViewModel quoteLineItemViewModel in _quoteLineItemViewModels)
                {
                    SubTotal += quoteLineItemViewModel.Amount;
                }
            }
            return SubTotal;
        }

        private decimal CalculateTotal()
        {
            decimal Total = 0;
            if (_quoteDetails != null)
            {
                Total += SubTotal - DiscountPercentageValue + _shippingAndHandling + _quoteDetails.Tax;
            }
            return Total;
        }

        private decimal CalculateDiscountPercent()
        {
            decimal discountPercentage = 0;
            if (DiscountPercent != 0)
            {
                // discountPercentage = Math.Round(((1 - ((SubTotal - DiscountPercentageValue) / SubTotal)) * 100), 2);
                discountPercentage = Math.Round(((DiscountPercent / 100) * SubTotal), 2);
            }
            return discountPercentage;
        }

        private async Task<Quote> UpdateQuote(string quoteStatus)
        {
            try
            {
                _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
                if (quoteStatus != string.Empty)
                {
                    _quote.QuoteStatus = quoteStatus;
                }
                _quote.Amount = Total;
                _quote.SubTotal = SubTotal;
                _quote.ShippingAndHandling = ShippingAndHandling;
                _quote.DiscountPercent = DiscountPercent;
                _quote = await _quoteRepository.UpdateQuoteToDbAsync(_quote);
                if (quoteStatus != string.Empty && quoteStatus == DataAccessUtils.DraftQuote)
                {
                    await _quoteService.RevertSubmittedQuoteToDraft(_quote);
                }
                //else
                //{
                //    await _quoteService.UpdateDiscountOrShippingAndHandling(_quote);
                //}
                await _quoteService.UpdateDiscountOrShippingAndHandling(_quote);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return _quote;
        }

        /// <summary>
        ///     Changes appbar buttons visibility based on quote status & selected items.
        /// </summary>
        private async void ChangeVisibility()
        {
            try
            {
                if (_quote == null)
                {
                    _quote = await _quoteRepository.GetQuoteAsync(_quoteId);
                }
                if (_quote.QuoteStatus == DataAccessUtils.DraftQuote && _itemNotSelected)
                {
                    IsAddItemVisible = Visibility.Visible;
                    IsChangeAddressVisible = Visibility.Visible;
                    IsSubmitQuoteVisible = Visibility.Visible;
                    IsSendmailVisible = Visibility.Visible;
                    IsDeleteQuoteVisible = Visibility.Visible;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Collapsed;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                    IsShippingAndHandlingEnabled = true;
                    IsDiscountEnabled = true;
                }
                else if (_quote.QuoteStatus == DataAccessUtils.SubmitQuote && _itemNotSelected)
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Visible;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Collapsed;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                    IsShippingAndHandlingEnabled = false;
                    IsDiscountEnabled = false;
                }
                else if (_quote.QuoteStatus == DataAccessUtils.Quote && _itemNotSelected)
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Visible;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Visible;
                    IsPlaceOrderVisible = Visibility.Visible;
                }
                else if (_quote.QuoteStatus == DataAccessUtils.DraftQuote && !(_itemNotSelected))
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Collapsed;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Visible;
                    IsEditQuoteVisible = Visibility.Collapsed;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                    IsShippingAndHandlingEnabled = true;
                    IsDiscountEnabled = true;
                }
                else if ((_quote.QuoteStatus == DataAccessUtils.SubmitQuote) && !(_itemNotSelected))
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Collapsed;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Collapsed;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                    IsShippingAndHandlingEnabled = false;
                    IsDiscountEnabled = false;
                }
                else if ((_quote.QuoteStatus == DataAccessUtils.Quote) && !(_itemNotSelected))
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Collapsed;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Visible;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                }
                else
                {
                    IsAddItemVisible = Visibility.Collapsed;
                    IsChangeAddressVisible = Visibility.Collapsed;
                    IsSubmitQuoteVisible = Visibility.Collapsed;
                    IsSendmailVisible = Visibility.Collapsed;
                    IsDeleteQuoteVisible = Visibility.Collapsed;
                    IsEditQuoteLineItemVisible = Visibility.Collapsed;
                    IsEditQuoteVisible = Visibility.Collapsed;
                    IsPlaceOrderVisible = Visibility.Collapsed;
                    IsShippingAndHandlingEnabled = true;
                    IsDiscountEnabled = true;
                }

                OnPropertyChanged("IsAddItemVisible");
                OnPropertyChanged("IsChangeAddressVisible");
                OnPropertyChanged("IsSubmitQuoteVisible");
                OnPropertyChanged("IsSendmailVisible");
                OnPropertyChanged("IsDeleteQuoteVisible");
                OnPropertyChanged("IsEditQuoteLineItemVisible");
                OnPropertyChanged("IsEditQuoteVisible");
                OnPropertyChanged("IsPlaceOrderVisible");
                OnPropertyChanged("IsShippingAndHandlingEnabled");
                OnPropertyChanged("IsDiscountEnabled");
                
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        public void CatalogButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("CategoryLevelOne", null);
        }

        public void QuotesButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("Quotes", null);
        }

        public void CustomersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("CustomersGroup", null);
        }

        public void OrdersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("Orders", null);
        }

        /// <summary>
        ///     Process all pending tasks for quotes
        /// </summary>
        /// <returns></returns>
        public async Task ProcessPendingRequestsForQuote()
        {
            // To sync the current quote
            if (_quoteId.Contains(PageUtils.Pending))
            {
                Quote quote = await _quoteRepository.GetQuoteAsync(_quoteId);

                if (quote.AddressId.Contains(Constants.Pending))
                {
                    // Confirm before posting quote whether it has Valid QuoteId                        
                    quote = await _quoteService.PostDraftQuote(quote);

                    if (quote != null)
                        await
                            _quoteService.UpdateQuoteShippingAddress(quote,
                                await _addressRepository.GetShippingAddress(quote.AddressId));
                }
                else
                {
                    quote = await _quoteService.PostDraftQuote(quote);
                }

                if (quote != null)
                {
                    _quoteId = quote.QuoteId;
                    QuoteDetails.QuoteId = quote.QuoteId;
                    if (_quote != null)
                        _quote.QuoteId = quote.QuoteId;
                }
            }

            await _quoteService.SyncOfflineQuotes();
            await _quoteService.SyncOfflineDeletedQuotes();
            await _quoteLineItemService.SyncOfflineQuoteLineItems();
            await _quoteService.SyncOfflineShippingAddress();
            await _quoteLineItemService.SyncOfflineQuoteLineItems();
        }

    }
}