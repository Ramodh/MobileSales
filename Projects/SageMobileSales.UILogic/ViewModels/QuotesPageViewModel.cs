using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using Windows.Storage;
using Windows.UI.Xaml;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class QuotesPageViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteLineItemService _quoteLineItemService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private CustomerDetails _customerAddress;
        private string _customerName;
        private string _emptyQuotes;
        private bool _inProgress;
        private bool _isAscending;
        private bool _isDescending;
        private string _log = string.Empty;
        private Quote _quote;
        private bool _gridViewItemClickable;
        private List<QuoteDetails> _quoteDetails;
        private string _quotePageTitle;
        private ToggleMenuFlyoutItem _selectedColumn;
        private string _selectedItem;
        private ToggleMenuFlyoutItem _sortByAscending;
        private ToggleMenuFlyoutItem _sortByDescending;
        private bool _syncProgress;
        private ToggleMenuFlyoutItem selectedItem;
        private string _previousSelectedSortType;
        private string _previousSortBy;
        private bool _camefromQuoteDetail;
        private bool _customerNamesort;
        private bool _quoteDate;
        private bool _quoteAmount;
        private bool _quoteStatus;
        private bool _salesPerson;


       
        public QuotesPageViewModel(INavigationService navigationService, ISalesRepRepository salesRepRepository,
            IQuoteRepository quoteRepository, IQuoteLineItemRepository quoteLineItemRepository,
            IQuoteService quoteService,
            IQuoteLineItemService quoteLineItemService, IEventAggregator eventAggregataor,
            ISyncCoordinatorService syncCoordinatorService)
        {
            _navigationService = navigationService;
            _quoteRepository = quoteRepository;
            _quoteLineItemRepository = quoteLineItemRepository;
            _salesRepRepository = salesRepRepository;
            _quoteService = quoteService;
            _quoteLineItemService = quoteLineItemService;
            _syncCoordinatorService = syncCoordinatorService;
            _eventAggregator = eventAggregataor;

            SortQuotesCommand = new DelegateCommand<object>(SortQuotes);
            SortByAscendingCommand = new DelegateCommand<object>(SortByAscending);
            SortByDescendingCommand = new DelegateCommand<object>(SortByDescending);
            _eventAggregator.GetEvent<QuoteDataChangedEvent>().Subscribe(UpdateQuoteList, ThreadOption.UIThread);
            _eventAggregator.GetEvent<QuoteSyncChangedEvent>()
                .Subscribe(QuotesSyncIndicator, ThreadOption.UIThread);
        }

        public ICommand SortQuotesCommand { get; set; }
        public ICommand SortByAscendingCommand { get; set; }
        public ICommand SortByDescendingCommand { get; set; }
      
        /// <summary>
        ///     Checks whether grid view item is clickable or not
        /// </summary>
        public bool GridViewItemClickable
        {
            get { return _gridViewItemClickable; }
            private set { SetProperty(ref _gridViewItemClickable, value); }
        }
        /// <summary>
        ///     checks whether customer name is selected or not
        /// </summary>
        public bool CustomerNamesort
        {
            get { return _customerNamesort; }
            private set { SetProperty(ref _customerNamesort, value); }
        }
        /// <summary>
        ///     checks whether Quote date is selected or not
        /// </summary>
        public bool QuoteDate
        {
            get { return _quoteDate; }
            private set { SetProperty(ref _quoteDate, value); }
        }
        /// <summary>
        ///     checks whether Quote amount is selected or not
        /// </summary>
        public bool QuoteAmount
        {
            get { return _quoteAmount; }
            private set { SetProperty(ref _quoteAmount, value); }
        }
        /// <summary>
        ///     checks whether Quote status is selected or not
        /// </summary>
        public bool QuoteStatus
        {
            get { return _quoteStatus; }
            private set { SetProperty(ref _quoteStatus, value); }
        }
        /// <summary>
        ///     checks whether sales person is selected or not
        /// </summary>
        public bool SalesPerson
        {
            get { return _salesPerson; }
            private set { SetProperty(ref _salesPerson, value); }
        }
        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyQuotes
        {
            get { return _emptyQuotes; }
            private set { SetProperty(ref _emptyQuotes, value); }
        }

        /// <summary>
        ///     Checks which column to sort by
        /// </summary>
        public ToggleMenuFlyoutItem SelectedColumn
        {
            get { return _selectedColumn; }
            private set
            {
                SetProperty(ref _selectedColumn, value);
                OnPropertyChanged("SelectedColumn");
            }
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
        ///     Data  syncing indicator
        /// </summary>
        public bool SyncProgress
        {
            get { return _syncProgress; }
            private set
            {
                SetProperty(ref _syncProgress, value);
                OnPropertyChanged("SyncProgress");
            }
        }

        /// <summary>
        ///     checks whether Ascending MenuItem is selected
        /// </summary>
        public bool IsAscending
        {
            get { return _isAscending; }
            private set
            {
                SetProperty(ref _isAscending, value);
                OnPropertyChanged("IsAscending");
            }
        }

        /// <summary>
        ///     checks whether Descending MenuItem selected
        /// </summary>
        public bool IsDescending
        {
            get { return _isDescending; }
            private set
            {
                SetProperty(ref _isDescending, value);
                OnPropertyChanged("IsDescending");
            }
        }

        /// <summary>
        ///     Holds Customer Name
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }

        /// <summary>
        ///     Quote Page title
        /// </summary>
        public string QuotePageTitle
        {
            get { return _quotePageTitle; }
            private set { SetProperty(ref _quotePageTitle, value); }
        }

        /// <summary>
        // Quote Details
        /// </summary>
       
        public List<QuoteDetails> QuoteDetails
        {
            get { return _quoteDetails; }
            private set
            {
                SetProperty(ref _quoteDetails, value);
                OnPropertyChanged("QuoteDetails");

                if (_quoteDetails.Count > 0)
                    InProgress = false;
            }
        }
    
        public string PreviousSelectedSortType
        {
            get { return _previousSelectedSortType; }
            private set
            {
                SetProperty(ref _previousSelectedSortType, value);
                OnPropertyChanged("PreviousSelectedSortType");
            }

        }
        public string PreviousSortBy
        {
            get { return _previousSortBy; }
            private set
            {
                SetProperty(ref _previousSortBy, value);
                OnPropertyChanged("PreviousSortBy");
            }

        }
        /// <summary>
        ///   checks whether last visited page is quotedetails.
        /// </summary>
        public bool CamefromQuoteDetail
        {
            get { return _camefromQuoteDetail; }
            private set { SetProperty(ref _camefromQuoteDetail, value); }
        }      


        /// <summary>
        ///     Load Customer Quotes
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                GridViewItemClickable = true;
                ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;
                if (sortSettings.Containers.ContainsKey("SortSettingsContainer"))
                {
                    if (sortSettings.Containers["SortSettingsContainer"].Values["cameFromQuoteDetail"] != null)
                    {
                        PageUtils.CameFromQuoteDetail = Convert.ToBoolean(sortSettings.Containers["SortSettingsContainer"].Values["CamefromQuoteDetail"].ToString());
                        sortSettings.Containers["SortSettingsContainer"].Values["CamefromQuoteDetail"] = false;
                    }
                }
                InProgress = true;

                if (!Constants.QuotesSyncProgress)
                {
                    IAsyncAction asyncAction = ThreadPool.RunAsync(
                        IAsyncAction =>
                        {
                            // Data Sync will Start.
                            _syncCoordinatorService.StartQuotesSync();
                        });
                    //asyncAction.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                    //{
                    //    if (asyncStatus == AsyncStatus.Canceled)
                    //        return;

                    //    Constants.QuotesSyncProgress = false;
                    //});
                    PageUtils.asyncActionQuotes = asyncAction;
                }

                SyncProgress = Constants.QuotesSyncProgress;
                QuoteDetails = new List<QuoteDetails>();

                if (navigationParameter != null)
                {
                    _customerAddress = navigationParameter as CustomerDetails;

                    //if (!Constants.ConnectedToInternet())
                    //{
                        QuoteDetails = await _quoteRepository.GetQuotesForCustomerAsync(_customerAddress.CustomerId);
                        GetSortSetings();
                    //}
                    CustomerName = ResourceLoader.GetForCurrentView("Resources").GetString("SeperatorSymbol") +
                                   _customerAddress.CustomerName;
                }          
                else
                {
                    //if (!Constants.ConnectedToInternet())
                    //{
                        QuoteDetails = await _quoteRepository.GetQuotesListAsync(await _salesRepRepository.GetSalesRepId());
                        GetSortSetings();
                    //}

                    // QuotePageTitle = ResourceLoader.GetForCurrentView("Resources").GetString("quotePageTitle");
                }

          

                if (Constants.ConnectedToInternet())
                    await ProcessPendingRequestsForQuote();

                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
                PageUtils.ResetLocalVariables();
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }


        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewItemClick(object sender, object parameter)
        {
            GridViewItemClickable = false;
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;
            try
            {               
                var selectedQuotedetails = ((parameter as ItemClickEventArgs).ClickedItem as QuoteDetails);
                _quote = await _quoteRepository.GetQuoteAsync(selectedQuotedetails.QuoteId);

                if (_quote == null)
                {
                    _quote = await _quoteRepository.GetQuoteFromPrimaryKey(selectedQuotedetails.Id);
                    selectedQuotedetails.QuoteId = _quote.QuoteId;
                }

                if (PageUtils.SelectedProduct != null)
                {
                    if (_quote.QuoteStatus == DataAccessUtils.SubmitQuote || _quote.QuoteStatus == DataAccessUtils.Quote)
                    {
                        await ShowMessageDialog();
                    }
                    else
                    {
                        await AddQuoteLineItemToQuote();
                    }
                }
                else
                {
                    CamefromQuoteDetail = true;
                    sortSettings.Containers["SortSettingsContainer"].Values["CamefromQuoteDetail"] = CamefromQuoteDetail;
                    PageUtils.CameFromQuoteDetail = CamefromQuoteDetail;               
                    _navigationService.Navigate("QuoteDetails", _quote.QuoteId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }


        /// <summary>
        ///     Displays Messgae Dialog
        /// </summary>
        private async Task ShowMessageDialog()
        {
            var messageDialog = new MessageDialog("Change Quote status from Submit to Draft.");

            // Add commands and set their command ids
            messageDialog.Commands.Add(new UICommand("Cancel", command => { }, 0));
            messageDialog.Commands.Add(new UICommand("Change Status", CommandInvokedHandler, 1));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Show the message dialog and get the event that was invoked via the async operator
            await messageDialog.ShowAsync();
        }

        /// <summary>
        ///     Adds QuoteLineItem to selected quote
        /// </summary>
        /// <returns></returns>
        private async Task AddQuoteLineItemToQuote()
        {
            try
            {
                if (PageUtils.SelectedProduct != null)
                {
                    QuoteLineItem quoteLineItemExists =
                        await
                            _quoteLineItemRepository.GetQuoteLineItemIfExistsForQuote(_quote.QuoteId,
                                PageUtils.SelectedProduct.ProductId);

                    if (quoteLineItemExists != null)
                    {
                        quoteLineItemExists.Quantity = quoteLineItemExists.Quantity + PageUtils.SelectedProduct.Quantity;
                        await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(quoteLineItemExists);

                        _quote.Amount = _quote.Amount +
                                        Math.Round((quoteLineItemExists.Price * quoteLineItemExists.Quantity), 2);
                        await _quoteRepository.UpdateQuoteToDbAsync(_quote);

                        if (_quote.QuoteId.Contains(PageUtils.Pending))
                        {
                            if (Constants.ConnectedToInternet())
                                await _quoteService.PostDraftQuote(_quote);
                        }
                        else
                        {
                            if (quoteLineItemExists.QuoteLineItemId.Contains(PageUtils.Pending))
                            {
                                //check internet before calling
                                if (Constants.ConnectedToInternet())
                                    await _quoteLineItemService.AddQuoteLineItem(_quote, quoteLineItemExists);
                            }
                            else
                            {
                                if (Constants.ConnectedToInternet())
                                    await _quoteLineItemService.EditQuoteLineItem(_quote, quoteLineItemExists);
                            }
                        }
                    }
                    else
                    {
                        var quoteLineItem = new QuoteLineItem();
                        quoteLineItem.QuoteId = _quote.QuoteId;
                        quoteLineItem.QuoteLineItemId = PageUtils.Pending + Guid.NewGuid();
                        quoteLineItem.ProductId = PageUtils.SelectedProduct.ProductId;
                        quoteLineItem.tenantId = PageUtils.SelectedProduct.TenantId;
                        quoteLineItem.Price = PageUtils.SelectedProduct.PriceStd;
                        quoteLineItem.Quantity = PageUtils.SelectedProduct.Quantity;
                        await _quoteLineItemRepository.AddQuoteLineItemToDbAsync(quoteLineItem);
                        if (Constants.ConnectedToInternet())
                        {
                            await _quoteLineItemService.AddQuoteLineItem(_quote, quoteLineItem);
                        }
                    }
                    PageUtils.SelectedProduct = null;
                    _navigationService.Navigate("QuoteDetails", _quote.QuoteId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Callback function for the invocation of the dialog commands.
        /// </summary>
        /// <param name="command">The command that was invoked.</param>
        private async void CommandInvokedHandler(IUICommand command)
        {
            _quote.QuoteStatus = DataAccessUtils.DraftQuote;
            if (PageUtils.SelectedProduct != null)
            {
                _quote.Amount += Math.Round((PageUtils.SelectedProduct.Quantity * PageUtils.SelectedProduct.PriceStd), 2);
            }
            _quote = await _quoteRepository.UpdateQuoteToDbAsync(_quote);
            await _quoteService.RevertSubmittedQuoteToDraft(_quote);
            await AddQuoteLineItemToQuote();
            _navigationService.Navigate("QuoteDetails", _quote.QuoteId);
        }

        /// <summary>
        ///     Gets Selected column by which quotes are to be sorted
        /// </summary>
        /// <param name="sender"></param>
        public void SortQuotes(object sender)
        {
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;           
            if (selectedItem != null)
            {
                selectedItem.IsChecked = false;
                
            }
            selectedItem = sender as ToggleMenuFlyoutItem;

            if (selectedItem != null)
            {
               selectedItem.IsChecked = true;
           
                Sort(selectedItem.Text, IsAscending);
            }
          //  SelectedColumn = selectedItem;     
        }

        /// <summary>
        ///     Sorts quotes by selected column name and order
        /// </summary>
        /// <param name="sender"></param>
        private void Sort(string selectedItem, bool orderby)
        {
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;
            sortSettings.CreateContainer("SortSettingsContainer", ApplicationDataCreateDisposition.Always);
            try
            {
                if (selectedItem == PageUtils.CustomerName)
                {
                    if (IsAscending)
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderBy(sortby => sortby.CustomerName).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    else
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderByDescending(sortby => sortby.CustomerName).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;

               
                    CustomerNamesort = true;
                    QuoteDate = false;
                    QuoteAmount = false;
                    QuoteStatus = false;
                    SalesPerson = false;

                    
                }
                else if (selectedItem == PageUtils.Date)
                {
                    if (IsAscending)
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderBy(sortby => sortby.CreatedOn).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    else
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderByDescending(sortby => sortby.CreatedOn).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                 
                    QuoteDate = true;
                    CustomerNamesort = false;                   
                    QuoteAmount = false;
                    QuoteStatus = false;
                    SalesPerson = false;
                }
                else if (selectedItem == PageUtils.Status)
                {
                    if (IsAscending)
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderBy(sortby => sortby.QuoteStatus).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    else
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderByDescending(sortby => sortby.QuoteStatus).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
             
                    QuoteStatus = true;
                    QuoteDate = false;
                    CustomerNamesort = false;
                    QuoteAmount = false;
                   
                    SalesPerson = false;
                }
                else if (selectedItem == PageUtils.Amount)
                {
                    if (IsAscending)
                    {
                        List<QuoteDetails> sortedQuoteDetails = QuoteDetails.OrderBy(sortby => sortby.Amount).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    else
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderByDescending(sortby => sortby.Amount).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    QuoteAmount = true;
                    QuoteStatus = false;
                    QuoteDate = false;
                    CustomerNamesort = false;
                    SalesPerson = false;
                    _selectedItem = selectedItem;
                }
                else if (selectedItem == PageUtils.SalesPerson)
                {
                    if (IsAscending)
                    {
                        List<QuoteDetails> sortedQuoteDetails = QuoteDetails.OrderBy(sortby => sortby.RepName).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    else
                    {
                        List<QuoteDetails> sortedQuoteDetails =
                            QuoteDetails.OrderByDescending(sortby => sortby.RepName).ToList();
                        QuoteDetails = sortedQuoteDetails;
                    }
                    SalesPerson = true;              
                    QuoteStatus = false;
                    QuoteDate = false;
                    CustomerNamesort = false;
                    QuoteAmount = false;
                 
                    _selectedItem = selectedItem;
               
                }
                 PreviousSelectedSortType = _selectedItem;
                if (orderby)
                {
                    PreviousSortBy = "Ascending";
                }
                else
                {
                    PreviousSortBy = "Descending";
                }
                sortSettings.Containers["SortSettingsContainer"].Values["PreviousSelectedSortType"] = PreviousSelectedSortType;
                sortSettings.Containers["SortSettingsContainer"].Values["PreviousSortBy"] = PreviousSortBy;

            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Sorts Orders by Ascending
        /// </summary>
        /// <param name="sender"></param>
        public void SortByAscending(object sender)
        {
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;     
            _sortByAscending = sender as ToggleMenuFlyoutItem;

            IsAscending = true;
            IsDescending = false;
            PreviousSortBy = _sortByAscending.Text;
            PageUtils.QuoteSortBy = _sortByAscending.Text;
            Sort(_selectedItem, IsAscending);
        }

        /// <summary>
        ///     Sorts Orders by Descending
        /// </summary>
        /// <param name="sender"></param>
        public void SortByDescending(object sender)
        {
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;
    
            _sortByDescending = sender as ToggleMenuFlyoutItem;

            IsDescending = true;
            IsAscending = false;
            PreviousSortBy = _sortByDescending.Text;
            PageUtils.QuoteSortBy = PreviousSortBy;
            Sort(_selectedItem, IsAscending);
        }

        public void CatalogButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("CategoryLevelOne", null);
        }

        public void QuotesButton_Click(object sender, object parameter)
        {
            //_navigationService.ClearHistory();
            _navigationService.Navigate("Quotes", null);
        }

        public void OrdersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("Orders", null);
        }

        public void CustomersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("CustomersGroup", null);
        }

        public void CreateQuoteButton_Click(object sender, object parameter)
        {
            var rootFrame = Window.Current.Content as Frame;
            List<PageStackEntry> navigationHistory = rootFrame.BackStack.ToList();
            PageStackEntry pageStack = navigationHistory.LastOrDefault();
            if (pageStack != null && pageStack.SourcePageType.Name == PageUtils.CustomerDetailPage)
            {
                _navigationService.Navigate("CreateQuote", _customerAddress);
            }
            else
            {
                _navigationService.Navigate("CreateQuote", null);
            }
        }

        /// <summary>
        ///     Process all pending tasks for quotes
        /// </summary>
        /// <returns></returns>
        public async Task ProcessPendingRequestsForQuote()
        {
            await _quoteService.SyncOfflineQuotes();
            await _quoteService.SyncOfflineDeletedQuotes();
            await _quoteLineItemService.SyncOfflineQuoteLineItems();
            await _quoteService.SyncOfflineShippingAddress();
            await _quoteLineItemService.SyncOfflineQuoteLineItems();
        }

        public async void UpdateQuoteList(bool updated)
        {
            await UpdateQuoteListInfo();
        }

        public async Task UpdateQuoteListInfo()
        {           
            if (_customerAddress == null)
            {
                QuoteDetails = await _quoteRepository.GetQuotesListAsync(await _salesRepRepository.GetSalesRepId());
            }
            else
            {
                QuoteDetails = await _quoteRepository.GetQuotesForCustomerAsync(_customerAddress.CustomerId);
            }
            if (QuoteDetails.Count == 0)
            {
                InProgress = false;
                EmptyQuotes = ResourceLoader.GetForCurrentView("Resources").GetString("QuotesEmptyText");
            }
            else
            {
                EmptyQuotes = string.Empty;
            }
            GetSortSetings();
         
        }


        /// <summary>
        ///    gets sort settings in Quotes
        /// </summary>
      
        private void GetSortSetings()
        {
            ApplicationDataContainer sortSettings = ApplicationData.Current.LocalSettings;
            if (PageUtils.CameFromQuoteDetail && sortSettings.Containers.ContainsKey("SortSettingsContainer"))
            {
               
                    if (sortSettings.Containers["SortSettingsContainer"].Values["PreviousSelectedSortType"] != null && 
                        sortSettings.Containers["SortSettingsContainer"].Values["PreviousSortBy"] != null)
                    {
                        PageUtils.QuoteSortType = sortSettings.Containers["SortSettingsContainer"].Values["PreviousSelectedSortType"].ToString();
                        PageUtils.QuoteSortBy = sortSettings.Containers["SortSettingsContainer"].Values["PreviousSortBy"].ToString();
                        if (PageUtils.QuoteSortBy == "Ascending")
                            {
                                IsAscending = true;
                                Sort(PageUtils.QuoteSortType, IsDescending);
                            }
                            else
                            {
                                IsDescending = true;
                                Sort(PageUtils.QuoteSortType, IsAscending);
                            }
                    }               
            }
            else
            {
                IsDescending = true;
                Sort(PageUtils.Date, IsAscending);
            }

        }         


        public void QuotesSyncIndicator(bool sync)
        {
            SyncProgress = Constants.QuotesSyncProgress;
        }
    }
}