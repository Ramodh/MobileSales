using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml;
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

namespace SageMobileSales.UILogic.ViewModels
{
    internal class OrdersPageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IOrderRepository _orderRepository;
        private readonly ISalesRepRepository _salesRepRepository;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private string _customerId;
        private string _customerName;
        private string _emptyOrders;
        private bool _inProgress;
        private bool _isAscending;
        private bool _isDescending;
        private string _log = string.Empty;
        private List<OrderDetails> _ordersList;
        private string _ordersPageTitle;
        private bool _selectedColumn;
        private string _selectedItem;
        private ToggleMenuFlyoutItem _sortByAscending;
        private ToggleMenuFlyoutItem _sortByDescending;
        private bool _syncProgress;
        private ToggleMenuFlyoutItem selectedItem;
        private bool _camefromOrderDetail;
        private string _previousSelectedSortType;
        private string _previousSortBy;
        private bool _camefromQuoteDetail;
        private bool _customerNamesort;
        private bool _orderDate;
        private bool _orderAmount;
        private bool _orderStatus;
        private bool _salesPerson;
        private bool _orderNumber;
        private string _sortType;
        private string _sortBy;
        private string _cameFrom = string.Empty;

        public OrdersPageViewModel(INavigationService navigationService, ISalesRepRepository salesRepRepository,
            IOrderRepository orderRepository, ICustomerRepository customerRepository, IEventAggregator eventAggregator,
            ISyncCoordinatorService syncCoordinatorService)
        {
            _navigationService = navigationService;
            _orderRepository = orderRepository;
            _salesRepRepository = salesRepRepository;
            _customerRepository = customerRepository;
            _syncCoordinatorService = syncCoordinatorService;
            _eventAggregator = eventAggregator;
            SortOrdersCommand = new DelegateCommand<object>(SortOrders);
            SortByAscendingCommand = new DelegateCommand<object>(SortByAscending);
            SortByDescendingCommand = new DelegateCommand<object>(SortByDescending);
            _eventAggregator.GetEvent<OrderDataChangedEvent>().Subscribe(UpdateOrderList, ThreadOption.UIThread);
            _eventAggregator.GetEvent<OrderSyncChangedEvent>()
                .Subscribe(OrdersSyncIndicator, ThreadOption.UIThread);
        }

        public ICommand SortOrdersCommand { get; set; }
        public ICommand SortByAscendingCommand { get; set; }
        public ICommand SortByDescendingCommand { get; set; }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyOrders
        {
            get { return _emptyOrders; }
            private set { SetProperty(ref _emptyOrders, value); }
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
        ///     Checks which column to sort by
        /// </summary>
        public bool SelectedColumn
        {
            get { return _selectedColumn; }
            private set
            {
                SetProperty(ref _selectedColumn, value);
                OnPropertyChanged("SelectedColumn");
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
        ///     Orders Page title
        /// </summary>
        public string OrdersPageTitle
        {
            get { return _ordersPageTitle; }
            private set { SetProperty(ref _ordersPageTitle, value); }
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
        ///     Dummy Quotes Data
        /// </summary>
        public List<OrderDetails> OrdersList
        {
            get { return _ordersList; }
            private set
            {
                SetProperty(ref _ordersList, value);
                OnPropertyChanged("OrdersList");

                if (_ordersList.Count > 0)
                    InProgress = false;
            }
        }
        /// <summary>
        ///   checks whether last visited page is quotedetails.
        /// </summary>
        public bool cameFromOrderDetail
        {
            get { return _camefromOrderDetail; }
            private set { SetProperty(ref _camefromOrderDetail, value); }
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
        public bool OrderDate
        {
            get { return _orderDate; }
            private set { SetProperty(ref _orderDate, value); }
        }
        /// <summary>
        ///     checks whether Quote amount is selected or not
        /// </summary>
        public bool OrderAmount
        {
            get { return _orderAmount; }
            private set { SetProperty(ref _orderAmount, value); }
        }
        /// <summary>
        ///     checks whether Quote status is selected or not
        /// </summary>
        public bool OrderStatus
        {
            get { return _orderStatus; }
            private set { SetProperty(ref _orderStatus, value); }
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
        ///     checks whether sales person is selected or not
        /// </summary>
        public bool OrderNumber
        {
            get { return _orderNumber; }
            private set { SetProperty(ref _orderNumber, value); }
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
        ///    
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                InProgress = true;
                ApplicationDataContainer orderSortSettings = ApplicationData.Current.LocalSettings;
                if (orderSortSettings.Containers.ContainsKey("OrderSortSettingsContainer"))
                {
                    if (orderSortSettings.Containers["OrderSortSettingsContainer"].Values["cameFromOrderDetail"] != null)
                    {
                        PageUtils.CameFromOrderDetail = Convert.ToBoolean(orderSortSettings.Containers["OrderSortSettingsContainer"].Values["cameFromOrderDetail"].ToString());
                        orderSortSettings.Containers["OrderSortSettingsContainer"].Values["cameFromOrderDetail"] = false;
                    }
                }
                if (!Constants.OrdersSyncProgress)
                {
                    IAsyncAction asyncAction = ThreadPool.RunAsync(
                        IAsyncAction =>
                        {
                            // Data Sync will Start.
                            _syncCoordinatorService.StartOrdersSync();
                        });
                    //asyncAction.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                    //{
                    //    if (asyncStatus == AsyncStatus.Canceled)
                    //        return;

                    //    Constants.OrdersSyncProgress = false;
                    //});
                    PageUtils.asyncActionOrders = asyncAction;
                }

                SyncProgress = Constants.OrdersSyncProgress;

                //var Frame = Window.Current.Content as Frame;
                ////InProgress = false;
                //if (Frame != null)
                //{
                //    List<PageStackEntry> navigationHistory = Frame.BackStack.ToList();
                //    if (navigationHistory.Count > 0)
                //    {
                //        PageStackEntry pageStack = navigationHistory.LastOrDefault();
                //        _cameFrom = pageStack.SourcePageType.Name;
                //    }

                //}

                if (navigationParameter != null)
                {
                    _customerId = navigationParameter as string;
                    Customer customer = await _customerRepository.GetCustomerDataAsync(_customerId);
                    OrdersList = new List<OrderDetails>();
                    //if (_cameFrom == PageUtils.CreateQuotePage)
                    //{
                    //    OrdersList = await _orderRepository.GetOrderStatusListForCustomerAsync(_customerId);
                    //    //GetSortSetings();
                    //}
                    //else
                    //{
                    //    //if (!Constants.ConnectedToInternet())
                    //    //{
                    //    OrdersList = await _orderRepository.GetOrdersForCustomerAsync(customer.CustomerId);

                    //    //GetSortSetings();
                    //}
                    OrdersList = await _orderRepository.GetOrdersForCustomerAsync(customer.CustomerId);
                    //}
                    CustomerName = ResourceLoader.GetForCurrentView("Resources").GetString("SeperatorSymbol") +
                                   customer.CustomerName;
                }
                //else if (_cameFrom == PageUtils.CreateQuotePage)
                //{
                //    OrdersList = await _orderRepository.GetOrdersStatusListForSalesRepAsync(await _salesRepRepository.GetSalesRepId());
                //    // GetSortSetings();
                //}
                else
                {
                    OrdersPageTitle = "Orders";

                    //if (!Constants.ConnectedToInternet())
                    //{
                    OrdersList = await _orderRepository.GetOrdersListAsync(await _salesRepRepository.GetSalesRepId());

                    //GetSortSetings();
                    //}                        
                }
                GetSortSetings();
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
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
        public void GridViewItemClick(object sender, object parameter)
        {
            try
            {
                ApplicationDataContainer orderSortSettings = ApplicationData.Current.LocalSettings;

                var arg = (parameter as ItemClickEventArgs).ClickedItem as OrderDetails;

                var rootFrame = Window.Current.Content as Frame;
                List<PageStackEntry> navigationHistory = rootFrame.BackStack.ToList();
                PageStackEntry pageStack = navigationHistory.LastOrDefault();

                if (pageStack != null)
                {
                    if (pageStack.SourcePageType.Name == PageUtils.CreateQuotePage)
                    {
                        _navigationService.Navigate("CreateQuote", arg.OrderId);
                    }
                    else
                    {
                        cameFromOrderDetail = true;
                        orderSortSettings.Containers["OrderSortSettingsContainer"].Values["cameFromOrderDetail"] = cameFromOrderDetail;
                        PageUtils.CameFromOrderDetail = cameFromOrderDetail;
                        _navigationService.Navigate("OrderDetails", arg);
                    }
                }
                else
                {
                    cameFromOrderDetail = true;
                    orderSortSettings.Containers["OrderSortSettingsContainer"].Values["cameFromOrderDetail"] = cameFromOrderDetail;
                    PageUtils.CameFromOrderDetail = cameFromOrderDetail;
                    _navigationService.Navigate("OrderDetails", arg);
                }
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
        ///     Gets Selected column by which Orders are to be sorted
        /// </summary>
        /// <param name="sender"></param>
        public void SortOrders(object sender)
        {
            if (selectedItem != null)
            {
                selectedItem.IsChecked = false;
            }
            selectedItem = sender as ToggleMenuFlyoutItem;

            if (selectedItem != null)
            {
                selectedItem.IsChecked = true;
                //  OrderDate = false;

                Sort(selectedItem.Text, IsAscending);
            }
        }

        /// <summary>
        ///     Sorts Orders by selected column name and order
        /// </summary>
        /// <param name="sender"></param>
        private void Sort(string selectedItem, bool orderby)
        {
            try
            {
                ApplicationDataContainer orderSortSettings = ApplicationData.Current.LocalSettings;
                orderSortSettings.CreateContainer("OrderSortSettingsContainer", ApplicationDataCreateDisposition.Always);
                if (selectedItem == PageUtils.CustomerName)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderBy(sortby => sortby.CustomerName).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.CustomerName).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = true;
                    OrderDate = false;
                    OrderAmount = false;
                    OrderStatus = false;
                    SalesPerson = false;
                    OrderNumber = false;
                    // SelectedColumn = false;
                }
                else if (selectedItem == PageUtils.Date)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails = OrdersList.OrderBy(sortby => sortby.CreatedOn).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.CreatedOn).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = false;
                    OrderDate = true;
                    OrderAmount = false;
                    OrderStatus = false;
                    SalesPerson = false;
                    OrderNumber = false;
                    // SelectedColumn = true;
                }
                else if (selectedItem == PageUtils.Status)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderBy(sortby => sortby.OrderStatus).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.OrderStatus).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = false;
                    OrderDate = false;
                    OrderAmount = false;
                    OrderStatus = true;
                    SalesPerson = false;
                    OrderNumber = false;
                    // SelectedColumn = false;
                }
                else if (selectedItem == PageUtils.Amount)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails = OrdersList.OrderBy(sortby => sortby.Amount).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.Amount).ToList();
                        OrdersList = sortedQuoteDetails;
                        //SelectedColumn = false;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = false;
                    OrderDate = false;
                    OrderAmount = true;
                    OrderStatus = false;
                    SalesPerson = false;
                    OrderNumber = false;
                }
                else if (selectedItem == PageUtils.SalesPerson)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails = OrdersList.OrderBy(sortby => sortby.RepName).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.RepName).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = false;
                    OrderDate = false;
                    OrderAmount = false;
                    OrderStatus = false;
                    SalesPerson = true;
                    OrderNumber = false;
                    //   SelectedColumn = false;
                }
                else if (selectedItem == PageUtils.OrderNumber)
                {
                    if (IsAscending)
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderBy(sortby => sortby.OrderNumber).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    else
                    {
                        List<OrderDetails> sortedQuoteDetails =
                            OrdersList.OrderByDescending(sortby => sortby.OrderNumber).ToList();
                        OrdersList = sortedQuoteDetails;
                    }
                    _selectedItem = selectedItem;
                    CustomerNamesort = false;
                    OrderDate = false;
                    OrderAmount = false;
                    OrderStatus = false;
                    SalesPerson = false;
                    OrderNumber = true;
                    //  SelectedColumn = false;
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
                orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSelectedSortType"] = PreviousSelectedSortType;
                orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSortBy"] = PreviousSortBy;
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
            _sortByAscending = sender as ToggleMenuFlyoutItem;

            IsAscending = true;
            IsDescending = false;
            PreviousSortBy = _sortByAscending.Text;
            PageUtils.OrderSortBy = _sortByAscending.Text;

            Sort(_selectedItem, IsAscending);
        }

        /// <summary>
        ///     Sorts Orders by Descending
        /// </summary>
        /// <param name="sender"></param>
        public void SortByDescending(object sender)
        {
            _sortByDescending = sender as ToggleMenuFlyoutItem;

            IsDescending = true;
            IsAscending = false;
            PreviousSortBy = _sortByDescending.Text;
            PageUtils.OrderSortBy = PreviousSortBy;

            Sort(_selectedItem, IsAscending);
        }

        public async void UpdateOrderList(bool updated)
        {
            await UpdateOrderListInfo();
        }

        public async Task UpdateOrderListInfo()
        {
            if (_customerId != null)
            {
                //if (_cameFrom == PageUtils.CreateQuotePage)
                //{
                //    OrdersList = await _orderRepository.GetOrderStatusListForCustomerAsync(_customerId);
                //}
                //else
                //{
                    OrdersList = await _orderRepository.GetOrdersForCustomerAsync(_customerId);
                //}
            }          
            else
            {
                OrdersList = await _orderRepository.GetOrdersListAsync(await _salesRepRepository.GetSalesRepId());
            }
            if (OrdersList.Count == 0)
            {
                InProgress = false;
                EmptyOrders = ResourceLoader.GetForCurrentView("Resources").GetString("OrdersEmptyText");
            }
            else
            {
                EmptyOrders = string.Empty;
            }
            GetSortSetings();
        }
        private void GetSortSetings()
        {
            ApplicationDataContainer orderSortSettings = ApplicationData.Current.LocalSettings;
            if (PageUtils.CameFromOrderDetail && orderSortSettings.Containers.ContainsKey("OrderSortSettingsContainer"))
            {

                if (orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSelectedSortType"] != null &&
                    orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSortBy"] != null)
                {
                    PageUtils.OrderSortType = orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSelectedSortType"].ToString();
                    PageUtils.OrderSortBy = orderSortSettings.Containers["OrderSortSettingsContainer"].Values["PreviousSortBy"].ToString();
                    if (PageUtils.OrderSortBy == "Ascending")
                    {
                        IsAscending = true;
                        Sort(PageUtils.OrderSortType, IsDescending);
                    }
                    else
                    {
                        IsDescending = true;
                        Sort(PageUtils.OrderSortType, IsAscending);
                    }
                }
                // Constants.CameFromQuoteDetail = false;
            }
            else
            {
                IsDescending = true;
                Sort(PageUtils.Date, IsAscending);
            }


        }

        public void OrdersSyncIndicator(bool sync)
        {
            SyncProgress = Constants.OrdersSyncProgress;
        }
    }
}