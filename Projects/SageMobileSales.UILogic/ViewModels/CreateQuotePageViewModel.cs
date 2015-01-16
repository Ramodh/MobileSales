using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CreateQuotePageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IFrequentlyPurchasedItemService _frequentlyPurchasedItemService;
        private readonly INavigationService _navigationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private IContactRepository _contactRepository;
        private List<QuoteType> _createQuoteFrom;
        private CustomerDetails _customerAddress;
        private string _customerId;
        private List<Customer> _customerList;
        private string _customerName;
        private string _customerSearchBoxText;
        private bool _inProgress;
        private Visibility _isCustomerSearchVisible;
        private bool _isSaveEnabled;
        private Visibility _isTextBlockVisible;
        private string _log = string.Empty;
        private OrderDetails _orderDetails;
        private string _orderId;
        private Product _productDetail;
        private string _quoteDescription;
        private QuoteType _selectedtype;

        public CreateQuotePageViewModel(INavigationService navigationService, IContactRepository contactRepository,
            IOrderRepository orderRepository, IFrequentlyPurchasedItemService frequentlyPurchasedItemService,
            ICustomerRepository customerRepository, IQuoteRepository quoteRepository, IQuoteService quoteService,
            IQuoteLineItemRepository quoteLineItemRepository)
        {
            _navigationService = navigationService;
            _contactRepository = contactRepository;
            _customerRepository = customerRepository;
            _quoteRepository = quoteRepository;
            _orderRepository = orderRepository;
            _quoteService = quoteService;
            _quoteLineItemRepository = quoteLineItemRepository;
            _frequentlyPurchasedItemService = frequentlyPurchasedItemService;
            QuoteTypeListViewSelectionChanged = new DelegateCommand<object>(SelectedQuoteType);
            SearchSuggestionsCommand =
                new DelegateCommand<SearchBoxSuggestionsRequestedEventArgs>(
                    async eventArgs => { await SearchBoxSuggestionsRequested(eventArgs); });
            ResultSuggestionChosenCommand =
                new DelegateCommand<SearchBoxResultSuggestionChosenEventArgs>(OnResultSuggestionChosen);
        }

        public List<Customer> CustomerList
        {
            get { return _customerList; }
            private set { SetProperty(ref _customerList, value); }
        }

        /// <summary>
        ///     List for selecting method of creating a quote
        /// </summary>
        public List<QuoteType> CreateQuoteFrom
        {
            get { return BindItemsToListView(); }
            private set { SetProperty(ref _createQuoteFrom, value); }
        }

        /// <summary>
        ///     List for selecting method of creating a quote
        /// </summary>
        public QuoteType SelectedType
        {
            get { return _selectedtype; }
            private set { SetProperty(ref _selectedtype, value); }
        }

        public string CustomerId
        {
            get { return _customerId; }
            private set { SetProperty(ref _customerId, value); }
        }

        public Product ProductDetails
        {
            get { return _productDetail; }
            private set { SetProperty(ref _productDetail, value); }
        }

        /// <summary>
        ///     Holds Customer Name
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }

        public string QuoteDescription
        {
            get { return _quoteDescription; }
            set { SetProperty(ref _quoteDescription, value); }
        }

        public string CustomerSearchBoxText
        {
            get { return _customerSearchBoxText; }
            set { SetProperty(ref _customerSearchBoxText, value); }
        }

        /// <summary>
        ///     checks whether customer search should be visible or not
        /// </summary>
        public Visibility IsCustomerSearchVisible
        {
            get { return _isCustomerSearchVisible; }
            private set { SetProperty(ref _isCustomerSearchVisible, value); }
        }

        /// <summary>
        ///     checks whether customer textblock should be visible or not
        /// </summary>
        public Visibility IsTextBlockVisible
        {
            get { return _isTextBlockVisible; }
            private set { SetProperty(ref _isTextBlockVisible, value); }
        }

        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        /// <summary>
        ///     evaluate validation
        /// </summary>
        public bool IsSaveEnabled
        {
            get { return _isSaveEnabled; }
            set
            {
                SetProperty(ref _isSaveEnabled, value);
                OnPropertyChanged("IsSaveEnabled");
            }
        }

        public DelegateCommand CustomerCommand { get; private set; }
        public DelegateCommand<SearchBoxQuerySubmittedEventArgs> SearchCommand { get; set; }
        public DelegateCommand<SearchBoxSuggestionsRequestedEventArgs> SearchSuggestionsCommand { get; set; }
        public DelegateCommand<SearchBoxResultSuggestionChosenEventArgs> ResultSuggestionChosenCommand { get; set; }
        public DelegateCommand<object> QuoteTypeListViewSelectionChanged { get; set; }

        /// <summary>
        ///     Navigate to ItemDetail page on choosing result suggestion
        /// </summary>
        /// <param name="args"></param>
        private async void OnResultSuggestionChosen(SearchBoxResultSuggestionChosenEventArgs eventArgs)
        {
            var customerId = eventArgs.Tag != null ? eventArgs.Tag.Trim() : null;
            if (!string.IsNullOrEmpty(customerId))
            {
                CustomerId = customerId;
                var customer = await _customerRepository.GetCustomerDataAsync(CustomerId);
                PageUtils.SelectedCustomer = customer;
            }
        }

        /// <summary>
        ///     Display result suggestions in searchbox
        /// </summary>
        /// <param name="args"></param>
        private async Task SearchBoxSuggestionsRequested(SearchBoxSuggestionsRequestedEventArgs eventArgs)
        {
            IsSaveEnabled = true;
            var queryText = eventArgs.QueryText != null ? eventArgs.QueryText.Trim() : null;
            var deferral = eventArgs.Request.GetDeferral();
            var obj =
                RandomAccessStreamReference.CreateFromUri(
                    new Uri((ResourceLoader.GetForCurrentView("Resources").GetString("NoImageUrl"))));
            try
            {
                var suggestionCollection = eventArgs.Request.SearchSuggestionCollection;
                //if (queryText == "")
                //{
                //    if (queryText == "")
                //    {
                //        foreach (Customer customer in CustomerList)
                //        {
                //            if (!string.IsNullOrEmpty(customer.CustomerName))
                //            {
                //                suggestionCollection.AppendResultSuggestion(customer.CustomerName, string.Empty,
                //                    customer.CustomerId, obj, string.Empty);
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    queryText = queryText.Trim().Replace("'", string.Empty);
                //}

                if (!queryText.Equals(string.Empty))
                {
                    queryText = queryText.Trim().Replace("'", string.Empty);
                }
                var querySuggestions = await _customerRepository.GetCustomerSearchSuggestionsAsync(queryText);
                if (querySuggestions != null && querySuggestions.Count > 0)
                {
                    foreach (var suggestion in querySuggestions)
                    {
                        suggestionCollection.AppendResultSuggestion(suggestion.CustomerName, string.Empty,
                            suggestion.CustomerId, obj, string.Empty);
                    }
                }
                else
                {
                    suggestionCollection.AppendQuerySuggestion(
                        ResourceLoader.GetForCurrentView("Resources").GetString("NoSuggestions"));
                }
                if (queryText == "")
                {
                    CustomerId = null;
                }
                PageUtils.SelectedCustomer = null;
                CustomerSearchBoxText = "Search for a Customer";
                OnPropertyChanged("CustomerSearchBoxText");
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            deferral.Complete();
        }

        /// <summary>
        ///     Loading data
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                IsSaveEnabled = true;
                var rootFrame = Window.Current.Content as Frame;
                var navigationHistory = rootFrame.BackStack.ToList();
                var pageStack = navigationHistory.LastOrDefault();

                CustomerList = await _customerRepository.GetCustomerList();
                SelectedType = CreateQuoteFrom.FirstOrDefault();
                if (PageUtils.SelectedCustomer != null)
                {
                    CustomerSearchBoxText = PageUtils.SelectedCustomer.CustomerName;
                }
                else
                {
                    CustomerSearchBoxText = "Search for a Customer";
                }

                if (navigationParameter != null)
                {
                    if (pageStack.SourcePageType.Name == PageUtils.CustomerDetailPage ||
                        pageStack.SourcePageType.Name == PageUtils.QuotesPage)
                    {
                        _customerAddress = navigationParameter as CustomerDetails;
                        PageUtils.SelectedCustomerDetails = _customerAddress;

                        IsCustomerSearchVisible = Visibility.Collapsed;
                        IsTextBlockVisible = Visibility.Visible;
                    }
                    else if (pageStack.SourcePageType.Name == PageUtils.OrdersPage)
                    {
                        _orderId = navigationParameter as string;
                        _orderDetails = await _orderRepository.GetOrderDetailsAsync(_orderId);
                        CustomerId = _orderDetails.CustomerId;
                        SelectedType = CreateQuoteFrom[1];
                        if (PageUtils.SelectedCustomerDetails != null)
                        {
                            IsCustomerSearchVisible = Visibility.Collapsed;
                            IsTextBlockVisible = Visibility.Visible;
                        }
                        else
                        {
                            IsCustomerSearchVisible = Visibility.Visible;
                            IsTextBlockVisible = Visibility.Collapsed;
                        }
                    }
                }

                else
                {
                    IsCustomerSearchVisible = Visibility.Visible;
                    IsTextBlockVisible = Visibility.Collapsed;
                }

                if (PageUtils.SelectedCustomerDetails != null)
                {
                    CustomerName = PageUtils.SelectedCustomerDetails.CustomerName;
                    CustomerId = PageUtils.SelectedCustomerDetails.CustomerId;
                }
                OnPropertyChanged("CreateQuoteFrom");
                OnPropertyChanged("SelectedType");
                OnPropertyChanged("CustomerSearchBoxText");


                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Bind Items for Listview to select method of creating a quote
        /// </summary>
        public List<QuoteType> BindItemsToListView()
        {
            _createQuoteFrom = new List<QuoteType>();
            _createQuoteFrom.Add(new QuoteType {createFrom = PageUtils.Scratch, createFromText = PageUtils.ScratchText});
            if (_orderDetails != null)
            {
                _createQuoteFrom.Add(new QuoteType
                {
                    createFrom = PageUtils.PreviousOrder,
                    createFromText = _orderDetails.OrderNumber
                });
            }
            else
            {
                _createQuoteFrom.Add(new QuoteType
                {
                    createFrom = PageUtils.PreviousOrder,
                    createFromText = PageUtils.PreviousOrderText
                });
            }
            _createQuoteFrom.Add(new QuoteType
            {
                createFrom = PageUtils.FrequentlyPurchasedItems,
                createFromText = PageUtils.PreviousPurchasedItemsText
            });
            return _createQuoteFrom;
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            try
            {
                //PageUtils.SelectedProduct = null;
                //PageUtils.SelectedCustomer = null;
                //PageUtils.SelectedCustomerDetails = null;

                base.OnNavigatedFrom(viewModelState, suspending);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Navigate to Quotes on save click
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        public async void SaveQuoteButton_Click(object sender, object parameter)
        {
            try
            {
                var productExists = false;

                if (IsSaveEnabled)
                {
                    InProgress = true;

                    var quote = new Quote();
                    IsSaveEnabled = false;
                    if (PageUtils.SelectedCustomer != null)
                    {
                        quote.CustomerId = PageUtils.SelectedCustomer.CustomerId;
                    }
                    else
                    {
                        quote.CustomerId = CustomerId;
                    }
                    if (quote.CustomerId == null)
                    {
                        InProgress = false;
                        var msgDialog = new MessageDialog(
                            ResourceLoader.GetForCurrentView("Resources").GetString(" MesDialogCreateQuoteSaveMessage"),
                            ResourceLoader.GetForCurrentView("Resources").GetString("MesDialogCreateQuoteTitle"));
                        msgDialog.Commands.Add(new UICommand("Ok"));
                        await msgDialog.ShowAsync();
                        IsSaveEnabled = true;
                    }
                    quote.QuoteDescription = QuoteDescription;
                    if (PageUtils.SelectedProduct != null)
                    {
                        ProductDetails = PageUtils.SelectedProduct;
                        quote.Amount += Math.Round((ProductDetails.Quantity*ProductDetails.PriceStd), 2);
                    }
                    //if (_orderId != null)
                    //{
                    //    SelectedType.createFrom = PageUtils.PreviousOrder;
                    //}
                    if (SelectedType.createFrom == DataAccessUtils.PreviousPurchasedItems &&
                        Constants.ConnectedToInternet())
                    {
                        //FrequentlyPurchasedItems
                        await _frequentlyPurchasedItemService.SyncFrequentlyPurchasedItems(quote.CustomerId);
                    }
                    quote = await _quoteRepository.AddQuoteToDbAsync(quote, SelectedType.createFrom, _orderId);

                    if (ProductDetails != null)
                    {
                        // Check for any duplications of lineItems
                        var quoteLineItemList =
                            await _quoteLineItemRepository.GetQuoteLineItemsForQuote(quote.QuoteId);

                        foreach (var quoteLineItem in quoteLineItemList)
                        {
                            if (quoteLineItem.ProductId == ProductDetails.ProductId)
                            {
                                quoteLineItem.Quantity = ProductDetails.Quantity;
                                quoteLineItem.Price = ProductDetails.PriceStd;

                                await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(quoteLineItem);
                                productExists = true;
                                break;
                            }
                        }

                        if (!productExists)
                        {
                            var quoteLineItemAdd = new QuoteLineItem();
                            quoteLineItemAdd.QuoteId = quote.QuoteId;
                            quoteLineItemAdd.QuoteLineItemId = PageUtils.Pending + Guid.NewGuid();
                            quoteLineItemAdd.ProductId = ProductDetails.ProductId;
                            quoteLineItemAdd.tenantId = ProductDetails.TenantId;
                            quoteLineItemAdd.Price = ProductDetails.PriceStd;
                            quoteLineItemAdd.Quantity = ProductDetails.Quantity;
                            quoteLineItemAdd.IsPending = true;
                            await _quoteLineItemRepository.AddQuoteLineItemToDbAsync(quoteLineItemAdd);
                        }
                    }

                    if (quote != null && Constants.ConnectedToInternet())
                    {
                        var postedQuote = await _quoteService.PostDraftQuote(quote);
                        if (postedQuote != null)
                            quote = postedQuote;
                    }

                    InProgress = false;
                    PageUtils.ResetLocalVariables();
                    PageUtils.SelectedProduct = null;
                    if (quote != null)
                        _navigationService.Navigate("QuoteDetails", quote.QuoteId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private void SelectedQuoteType(object args)
        {
            try
            {
                var quoteTypeListView = ((ListView) args);
                if (quoteTypeListView != null && quoteTypeListView.SelectedItem != null)
                {
                    SelectedType = (QuoteType) quoteTypeListView.SelectedItem;
                    if (SelectedType.createFrom == PageUtils.PreviousOrder)
                    {
                        string customerId = null;
                        if (PageUtils.SelectedCustomer != null)
                        {
                            customerId = PageUtils.SelectedCustomer.CustomerId;
                        }
                        else if (PageUtils.SelectedCustomerDetails != null)
                        {
                            customerId = PageUtils.SelectedCustomerDetails.CustomerId;
                        }
                        _navigationService.Navigate("Orders", customerId);
                    }
                    else if (SelectedType.createFrom == PageUtils.Scratch ||
                             SelectedType.createFrom == PageUtils.FrequentlyPurchasedItems)
                    {
                        _createQuoteFrom[1].createFromText = PageUtils.PreviousOrderText;
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }

    /// <summary>
    ///     class for method of creating quote
    /// </summary>
    public class QuoteType : INotifyPropertyChanged
    {
        private string _createFrom;
        private string _createFromText;

        public string createFrom
        {
            get { return _createFrom; }
            set
            {
                _createFrom = value;
                OnPropertyChanged("createFrom");
            }
        }

        public string createFromText
        {
            get { return _createFromText; }
            set
            {
                _createFromText = value;
                OnPropertyChanged("createFromText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }
    }
}