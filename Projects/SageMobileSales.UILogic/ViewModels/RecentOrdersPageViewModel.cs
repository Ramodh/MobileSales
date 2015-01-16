using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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
    internal class RecentOrdersPageViewModel : ViewModel
    {
        private readonly CustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;
        private readonly QuoteRepository _quoteRepository;
        private readonly SalesHistoryRepository _salesHistoryRepository;
        private readonly ISalesHistoryService _salesHistoryService;
        private readonly IServiceAgent _serviceAgent;
        private string _customerName;
        private string _imageUri;
        private bool _inProgress;
        private bool _isAscending;
        private bool _isDescending;
        private QuoteLineItemViewModel _lineItemDetail;
        private decimal _lineItemPrice;
        private string _log = string.Empty;
        private Product _productDetail;
        private List<ProductAssociatedBlob> _productImage;
        private string _productName = string.Empty;
        private int _productQuantity;
        private string _productSku = string.Empty;
        private QuoteDetails _quoteDetails;
        private List<SalesHistory> _salesHistoryList;
        private bool _selectedColumn;
        private string _selectedItem;
        private ToggleMenuFlyoutItem _sortByAscending;
        private ToggleMenuFlyoutItem _sortByDescending;
        private ToggleMenuFlyoutItem selectedItem;

        public RecentOrdersPageViewModel(ISalesHistoryService salesHistoryService, IServiceAgent serviceAgent,
            QuoteRepository quoteRepository, CustomerRepository customerRepository,
            INavigationService navigationService, IProductAssociatedBlobsRepository productAssociatedBlobsRepository,
            SalesHistoryRepository salesHistoryRepository)
        {
            _navigationService = navigationService;
            _salesHistoryService = salesHistoryService;
            _serviceAgent = serviceAgent;
            _salesHistoryRepository = salesHistoryRepository;
            _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
            _quoteRepository = quoteRepository;
            _customerRepository = customerRepository;
            SortRecentOrdersCommand = new DelegateCommand<object>(SortRecentOrders);
            SortByAscendingCommand = new DelegateCommand<object>(SortByAscending);
            SortByDescendingCommand = new DelegateCommand<object>(SortByDescending);
        }

        public ICommand SortRecentOrdersCommand { get; set; }
        public ICommand SortByAscendingCommand { get; set; }
        public ICommand SortByDescendingCommand { get; set; }

        public List<SalesHistory> SalesHistoryList
        {
            get { return _salesHistoryList; }
            private set { SetProperty(ref _salesHistoryList, value); }
        }

        public QuoteLineItemViewModel LineItemDetail
        {
            get { return _lineItemDetail; }
            private set { SetProperty(ref _lineItemDetail, value); }
        }

        public Product ProductDetail
        {
            get { return _productDetail; }
            private set { SetProperty(ref _productDetail, value); }
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
        ///     Product Name
        /// </summary>
        public string ProductName
        {
            get { return _productName; }
            private set { SetProperty(ref _productName, value); }
        }

        /// <summary>
        ///     Product Name
        /// </summary>
        public string ProductSku
        {
            get { return _productSku; }
            private set { SetProperty(ref _productSku, value); }
        }

        /// <summary>
        ///     Product Name
        /// </summary>
        public decimal LineItemPrice
        {
            get { return _lineItemPrice; }
            private set { SetProperty(ref _lineItemPrice, value); }
        }

        /// <summary>
        ///     Product Name
        /// </summary>
        public int ProductQuantity
        {
            get { return _productQuantity; }
            private set { SetProperty(ref _productQuantity, value); }
        }

        /// <summary>
        ///     Product Name
        /// </summary>
        public string ImageUri
        {
            get { return _imageUri; }
            private set { SetProperty(ref _imageUri, value); }
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

        public string CustomerName
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }

        /// checks whether Ascending MenuItem is selected
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

        public List<ProductAssociatedBlob> ProductImages
        {
            get { return _productImage; }
            private set { SetProperty(ref _productImage, value); }
        }

        //public RecentOrdersPageViewModel(INavigationService navigationService, IProductAssociatedBlobsRepository productAssociatedBlobsRepository)        
        //{
        //     _navigationService = navigationService;
        //     _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
        //     SortRecentOrdersCommand = new DelegateCommand<object>(SortRecentOrders);
        //     SortByAscendingCommand = new DelegateCommand<object>(SortByAscending);
        //     SortByDescendingCommand = new DelegateCommand<object>(SortByDescending);
        // }
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            var rootFrame = Window.Current.Content as Frame;
            var navigationHistory = rootFrame.BackStack.ToList();
            var pageStack = navigationHistory.LastOrDefault();

            InProgress = true;
            if (navigationParameter != null && pageStack.SourcePageType.Name == PageUtils.ItemDetailPage)
            {
                ProductDetail = navigationParameter as Product;
                ProductImages =
                    await _productAssociatedBlobsRepository.GetProductAssociatedBlobsAsync(ProductDetail.ProductId);
                if (ProductImages != null)
                {
                    ImageUri = ProductImages.FirstOrDefault().Url;
                }

                ProductQuantity = ProductDetail.Quantity;
                ProductName = ProductDetail.ProductName;
                LineItemPrice = ProductDetail.PriceStd;
                ProductSku = ProductDetail.Sku;
                ImageUri = ProductDetail.PhotoUrl;
                if (!string.IsNullOrEmpty(PageUtils.SelectedQuoteId))
                {
                    _quoteDetails = await _quoteRepository.GetQuoteDetailsAsync(PageUtils.SelectedQuoteId);
                    //await _salesHistoryService.SyncSalesHistory(_quoteDetails.CustomerId, ProductDetail.ProductId);
                    SalesHistoryList =
                        await
                            _salesHistoryRepository.GetCustomerProductSalesHistory(_quoteDetails.CustomerId,
                                ProductDetail.ProductId);
                }
            }
            else if (navigationParameter != null && pageStack.SourcePageType.Name == PageUtils.QuoteDetailsPage)
            {
                LineItemDetail = navigationParameter as QuoteLineItemViewModel;
                ProductQuantity = Convert.ToInt32(LineItemDetail.LineItemQuantity);
                ProductName = LineItemDetail.ProductName;
                LineItemPrice = LineItemDetail.LineItemPrice;
                ProductSku = LineItemDetail.ProductSku;
                ImageUri = LineItemDetail.ImageUri;
            }
            if (LineItemDetail != null)
            {
                var quoteDetails = await _quoteRepository.GetQuoteDetailsAsync(LineItemDetail.QuoteId);
                LineItemDetail.CustomerId = quoteDetails.CustomerId;
                await _salesHistoryService.SyncSalesHistory(LineItemDetail.CustomerId, LineItemDetail.ProductId);
                var customer = await _customerRepository.GetCustomerDataAsync(quoteDetails.CustomerId);
                CustomerName = customer.CustomerName;
            }

            SalesHistoryList =
                await
                    _salesHistoryRepository.GetCustomerProductSalesHistory(LineItemDetail.CustomerId,
                        LineItemDetail.ProductId);
            InProgress = false;

            //TODO
            // Need to be replaced with real data once Recent Orders service calls are done
            //ProductRecentOrders = new List<RecentOrders>();
            //for (int i = 0; i < 4; i++)
            //{
            //    RecentOrders recentOrder = new RecentOrders();
            //    recentOrder.Date = Convert.ToDateTime("05/09/2014");
            //    recentOrder.Invoice = "#1234567";
            //    recentOrder.Quantity = 9;
            //    recentOrder.UnitPrice = Convert.ToDecimal("369.89");
            //    recentOrder.Total = Convert.ToDecimal("3329.01");
            //    ProductRecentOrders.Add(recentOrder);
            //}
            //CustomerName = "Actwin Co. Ltd";


            IsDescending = true;
            Sort(PageUtils.Date, IsAscending);

            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        /// <summary>
        ///     Gets Selected column by which quotes are to be sorted
        /// </summary>
        /// <param name="sender"></param>
        public void SortRecentOrders(object sender)
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
        ///     Sorts recent orders by selected column name and order
        /// </summary>
        /// <param name="sender"></param>
        private void Sort(string selectedItem, bool orderby)
        {
            try
            {
                if (selectedItem == PageUtils.Invoice)
                {
                    if (IsAscending)
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderBy(sortby => sortby.InvoiceNumber).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    else
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderByDescending(sortby => sortby.InvoiceNumber).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    _selectedItem = selectedItem;
                    SelectedColumn = false;
                }
                else if (selectedItem == PageUtils.Date)
                {
                    if (IsAscending)
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderBy(sortby => sortby.InvoiceDate).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    else
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderByDescending(sortby => sortby.InvoiceDate).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    _selectedItem = selectedItem;
                    SelectedColumn = true;
                }
                else if (selectedItem == PageUtils.Quantity)
                {
                    if (IsAscending)
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderBy(sortby => sortby.Quantity).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    else
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderByDescending(sortby => sortby.Quantity).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    _selectedItem = selectedItem;
                    SelectedColumn = false;
                }
                else if (selectedItem == PageUtils.Amount)
                {
                    if (IsAscending)
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderBy(sortby => sortby.Total).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    else
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderByDescending(sortby => sortby.Total).ToList();
                        SalesHistoryList = sortedRecentOrders;
                        SelectedColumn = false;
                    }
                    _selectedItem = selectedItem;
                }
                else if (selectedItem == PageUtils.UnitPrice)
                {
                    if (IsAscending)
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderBy(sortby => sortby.Price).ToList();
                        SalesHistoryList = sortedRecentOrders;
                    }
                    else
                    {
                        var sortedRecentOrders =
                            SalesHistoryList.OrderByDescending(sortby => sortby.Price).ToList();
                        SalesHistoryList = sortedRecentOrders;
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
    }
}