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
        ///     Load dummy data
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
                if (navigationParameter != null)
                {
                    _customerId = navigationParameter as string;
                    Customer customer = await _customerRepository.GetCustomerDataAsync(_customerId);
                    OrdersList = new List<OrderDetails>();
                    OrdersList = await _orderRepository.GetOrdersForCustomerAsync(customer.CustomerId);
                    CustomerName = ResourceLoader.GetForCurrentView("Resources").GetString("DividerSymbol") +
                                   customer.CustomerName;
                }
                else
                {
                    OrdersPageTitle = "Orders";
                    OrdersList = await _orderRepository.GetOrdersListAsync(await _salesRepRepository.GetSalesRepId());
                }

                IsDescending = true;
                Sort(PageUtils.Date, IsAscending);
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
                }
                else
                {
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
                    SelectedColumn = false;
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
                    SelectedColumn = true;
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
                    SelectedColumn = false;
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
                        SelectedColumn = false;
                    }
                    _selectedItem = selectedItem;
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
                    SelectedColumn = false;
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
                    SelectedColumn = false;
                }
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
            Sort(_selectedItem, IsAscending);
        }

        public async void UpdateOrderList(bool updated)
        {
            await UpdateOrderListInfo();
        }

        public async Task UpdateOrderListInfo()
        {
            if (_customerId == null)
            {
                OrdersList = await _orderRepository.GetOrdersListAsync(await _salesRepRepository.GetSalesRepId());
            }
            else
            {
                OrdersList = await _orderRepository.GetOrdersForCustomerAsync(_customerId);
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
        }

        public void OrdersSyncIndicator(bool sync)
        {
            SyncProgress = Constants.OrdersSyncProgress;
        }
    }
}