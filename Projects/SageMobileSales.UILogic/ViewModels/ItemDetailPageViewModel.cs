using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class ItemDetailPageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IProductAssociatedBlobsRepository _productAssociatedBlobsRepository;
        private readonly IProductDetailsService _productDetailsService;
        private readonly IProductRepository _productRepository;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteLineItemService _quoteLineItemService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private readonly SalesHistoryRepository _salesHistoryRepository;
        private readonly SalesHistoryService _salesHistoryService;
        private IContactRepository _contactRepository;
        private List<Customer> _customerList;
        private string _customerName;
        private bool _emptyText;
        private Visibility _isRecentOrdersVisible;
        private List<ProductDetails> _otherProduct;
        private List<ProductAssociatedBlob> _productImage;
        private List<SalesHistory> _salesHistoryList;

        public ItemDetailPageViewModel(INavigationService navigationService, IProductRepository productRepository,
            IProductAssociatedBlobsRepository productAssociatedBlobsRepository,
            IProductDetailsService productDetailsService,
            IContactRepository contactRepository, ICustomerRepository customerRepository, IQuoteService quoteService,
            IQuoteLineItemRepository quoteLineItemRepository, IQuoteRepository quoteRepository,
            SalesHistoryRepository salesHistoryRepository, SalesHistoryService salesHistoryService,
            IQuoteLineItemService quoteLineItemService)
        {
            _navigationService = navigationService;
            _productRepository = productRepository;
            _productAssociatedBlobsRepository = productAssociatedBlobsRepository;
            _productDetailsService = productDetailsService;
            _contactRepository = contactRepository;
            _customerRepository = customerRepository;
            _quoteService = quoteService;
            _quoteLineItemService = quoteLineItemService;
            _quoteLineItemRepository = quoteLineItemRepository;
            _quoteRepository = quoteRepository;
            _salesHistoryService = salesHistoryService;
            _salesHistoryRepository = salesHistoryRepository;
            //TextChangedCommand = new DelegateCommand<object>(TextBoxTextChanged);
            TextGotFocusCommand = new DelegateCommand<object>(QunatityTextBoxGotFocus);
            TextLostFocusCommand = new DelegateCommand<object>(QunatityTextBoxLostFocus);
            IncrementCountCommand = DelegateCommand.FromAsyncHandler(IncrementCount);
            DecrementCountCommand = DelegateCommand.FromAsyncHandler(DecrementCount);
        }

        //public DelegateCommand<object> TextChangedCommand { get; set; }
        public DelegateCommand<object> TextGotFocusCommand { get; set; }
        public DelegateCommand<object> TextLostFocusCommand { get; set; }
        public DelegateCommand IncrementCountCommand { get; private set; }

        public DelegateCommand DecrementCountCommand { get; private set; }

        #region Properties

        private Visibility _camefromCatalog;
        private Visibility _camefromCreateQuote;
        private string _enteredQuantity;
        private bool _inProgress;
        private string _log = string.Empty;
        private Product _productDetail;
        private string _productId;
        private string _productName;
        private string _productPrice;
        private string _productSKU;
        private string _productStock;
        private QuoteDetails _quoteDetails;
        private string _unitOfMeasure;
        private bool _isAddToQuoteEnabled;
        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public string ProductName
        {
            get { return _productName; }
            private set { SetProperty(ref _productName, value); }
        }

        public string ProductPrice
        {
            get { return _productPrice; }
            private set { SetProperty(ref _productPrice, value); }
        }

        public string ProductSKU
        {
            get { return _productSKU; }
            private set { SetProperty(ref _productSKU, value); }
        }

        public string ProductStock
        {
            get { return _productStock; }
            private set { SetProperty(ref _productStock, value); }
        }

        public string UnitOfMeasure
        {
            get { return _unitOfMeasure; }
            private set { SetProperty(ref _unitOfMeasure, value); }
        }

        public Product ProductDetails
        {
            get { return _productDetail; }
            private set { SetProperty(ref _productDetail, value); }
        }

        public List<ProductAssociatedBlob> ProductImages
        {
            get { return _productImage; }
            private set { SetProperty(ref _productImage, value); }
        }

        public List<ProductDetails> OtherProducts
        {
            get { return _otherProduct; }
            private set { SetProperty(ref _otherProduct, value); }
        }

        public List<Customer> CustomerList
        {
            get { return _customerList; }
            private set { SetProperty(ref _customerList, value); }
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public bool EmptyText
        {
            get { return _emptyText; }
            private set { SetProperty(ref _emptyText, value); }
        }

        public Visibility CamefromCreateQuote
        {
            get { return _camefromCreateQuote; }
            private set { SetProperty(ref _camefromCreateQuote, value); }
        }

        public Visibility CamefromCatalog
        {
            get { return _camefromCatalog; }
            private set { SetProperty(ref _camefromCatalog, value); }
        }

        public string EnteredQuantity
        {
            get { return _enteredQuantity; }
            private set { SetProperty(ref _enteredQuantity, value); }
        }
        public Visibility IsRecentOrdersVisible
        {
            get { return _isRecentOrdersVisible; }
            private set { SetProperty(ref _isRecentOrdersVisible, value); }
        }

        public string CustomerName
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }

        public List<SalesHistory> SalesHistoryList
        {
            get { return _salesHistoryList; }
            private set { SetProperty(ref _salesHistoryList, value); }
        }

        public bool IsAddToQuoteEnabled
        {
            get { return _isAddToQuoteEnabled; }
            private set { SetProperty(ref _isAddToQuoteEnabled, value); }
        }

        #endregion

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            _productId = navigationParameter as string;

            if (PageUtils.CamefromQuoteDetails)
            {
                IsAddToQuoteEnabled = true;
                CamefromCreateQuote = Visibility.Visible;
                CamefromCatalog = Visibility.Collapsed;
            }
            else
            {
                CamefromCreateQuote = Visibility.Collapsed;
                CamefromCatalog = Visibility.Visible;
            }
            InProgress = true;
            //Display data from LocalDB
            DisplayProductDetails(_productId);

            //TODO
            // Need to Change Code 
            if (!string.IsNullOrEmpty(PageUtils.SelectedQuoteId))
            {
                _quoteDetails = await _quoteRepository.GetQuoteDetailsAsync(PageUtils.SelectedQuoteId);
                await _salesHistoryService.SyncSalesHistory(_quoteDetails.CustomerId, _productId);
                SalesHistoryList =
                    await _salesHistoryRepository.GetCustomerProductSalesHistory(_quoteDetails.CustomerId, _productId);
                if (SalesHistoryList != null)
                {
                    IsRecentOrdersVisible = Visibility.Visible;
                    if (SalesHistoryList.Count > 7)
                    {
                        SalesHistoryList = SalesHistoryList.GetRange(0, 7);
                        SalesHistoryList.Add(new SalesHistory {InvoiceNumber = DataAccessUtils.SeeMore});
                    }
                }
                Customer customer = await _customerRepository.GetCustomerDataAsync(_quoteDetails.CustomerId);
                if (customer != null)
                {
                    CustomerName = customer.CustomerName;
                }
            }

            if (SalesHistoryList != null)
            {
                if (SalesHistoryList.Count > 0)
                {
                    IsRecentOrdersVisible = Visibility.Visible;
                }
                else
                {
                    IsRecentOrdersVisible = Visibility.Collapsed;
                }
            }
            else
            {
                IsRecentOrdersVisible = Visibility.Collapsed;
            }

            // Making Service request to get complete details- images, product, other products
            await _productDetailsService.SyncProductDetails(_productId);

            //Need to implement caching for images
            //Display Updated from Web Service
            DisplayProductDetails(_productId);
            CustomerList = await _customerRepository.GetCustomerList();
            EmptyText = true;
            PageUtils.ResetLocalVariables();
            PageUtils.SelectedProduct = null;

            InProgress = false;
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
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
            try
            {
                var selectedProduct= (parameter as ItemClickEventArgs).ClickedItem as ProductDetails;
                if(selectedProduct!=null)
                {
                _productId = selectedProduct.ProductId;

                //Display data from LocalDB
                DisplayProductDetails(_productId);
                // Making Service request to get complete details- images, product, other products
                await _productDetailsService.SyncProductDetails(_productId);

                //Need to implement caching for images
                //Display Updated from Web Service
                DisplayProductDetails(_productId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private async void DisplayProductDetails(string productId)
        {
            // Code enhancement need to be done.
            try
            {
                //ProductImages = new List<ProductAssociatedBlob>();
                //To retrieve Images
                ProductImages = await _productAssociatedBlobsRepository.GetProductAssociatedBlobsAsync(productId);

                //if (ProductImages == null)
                //{
                //    ProductImages = new List<ProductAssociatedBlob>();
                //    ProductAssociatedBlob productAssociatedBlob = new ProductAssociatedBlob();
                //    productAssociatedBlob.Url = string.Empty;
                //    ProductImages.Add(productAssociatedBlob);
                //}
                //To get product details
                ProductDetails = await _productRepository.GetProductdetailsAsync(productId);
                if (ProductDetails != null)
                {
                    ProductName = ProductDetails.ProductName != null ? ProductDetails.ProductName : string.Empty;
                    //Need to implement currency formatter
                    ProductPrice = "$ " + ProductDetails.PriceStd;
                    ProductSKU = ProductDetails.Sku;
                    ProductStock = ProductDetails.Quantity.ToString();
                    UnitOfMeasure = ProductDetails.UnitOfMeasure;

                    //To get other product related information
                    OtherProducts = await _productRepository.GetProductRelatedItems(productId);
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Navigate to Quotes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void AddToExistingQuoteButton_Click(object sender, object parameter)
        {
            //ProductDetails.Quantity = EnteredQuantity;
            ProductDetails.Quantity = Convert.ToInt32(EnteredQuantity);
            PageUtils.SelectedProduct = ProductDetails;
            _navigationService.Navigate("Quotes", null);
        }

        public void addToNewQuoteButton_Click(object sender, object parameter)
        {
            //ProductDetails.Quantity = EnteredQuantity;
            ProductDetails.Quantity = Convert.ToInt32(EnteredQuantity);
            PageUtils.SelectedProduct = ProductDetails;
            _navigationService.Navigate("CreateQuote", null);
        }

        public async void AddToQuoteButton_Click(object sender, object parameter)
        {
            try
            {
                if (IsAddToQuoteEnabled)
                {
                    IsAddToQuoteEnabled = false;
                    //InProgress = true;
                    Quote quote = await _quoteRepository.GetQuoteAsync(PageUtils.SelectedQuoteId);
                    QuoteLineItem quoteLineItemExists =
                        await _quoteLineItemRepository.GetQuoteLineItemIfExistsForQuote(quote.QuoteId, _productId);

                    if (quoteLineItemExists != null)
                    {
                        quoteLineItemExists.Quantity = quoteLineItemExists.Quantity + Convert.ToInt32(EnteredQuantity);
                        await _quoteLineItemRepository.UpdateQuoteLineItemToDbAsync(quoteLineItemExists);

                        quote.Amount = quote.Amount +
                                       Math.Round((quoteLineItemExists.Price * quoteLineItemExists.Quantity), 2);
                        await _quoteRepository.UpdateQuoteToDbAsync(quote);

                        if (quote.QuoteId.Contains(PageUtils.Pending))
                        {
                            if (Constants.ConnectedToInternet())
                                await _quoteService.PostDraftQuote(quote);
                        }
                        else
                        {
                            if (quoteLineItemExists.QuoteLineItemId.Contains(PageUtils.Pending))
                            {
                                //check internet before calling
                                if (Constants.ConnectedToInternet())
                                    await _quoteLineItemService.AddQuoteLineItem(quote, quoteLineItemExists);
                            }
                            else
                            {
                                if (Constants.ConnectedToInternet())
                                    await _quoteLineItemService.EditQuoteLineItem(quote, quoteLineItemExists);
                            }
                        }
                    }
                    else
                    {
                        var quoteLineItem = new QuoteLineItem();
                        quoteLineItem.QuoteId = PageUtils.SelectedQuoteId;
                        quoteLineItem.QuoteLineItemId = PageUtils.Pending + Guid.NewGuid();
                        quoteLineItem.ProductId = _productId;
                        quoteLineItem.tenantId = ProductDetails.TenantId;
                        quoteLineItem.Price = ProductDetails.PriceStd;
                        //quoteLineItem.Quantity = EnteredQuantity;
                        quoteLineItem.Quantity = Convert.ToInt32(EnteredQuantity);
                        quoteLineItem.IsPending = true;

                        await _quoteLineItemRepository.AddQuoteLineItemToDbAsync(quoteLineItem);

                        quote.Amount = quote.Amount + Math.Round((quoteLineItem.Price * quoteLineItem.Quantity), 2);
                        await _quoteRepository.UpdateQuoteToDbAsync(quote);

                        if (quote.QuoteId.Contains(PageUtils.Pending))
                        {
                            if (Constants.ConnectedToInternet())
                                await _quoteService.PostDraftQuote(quote);
                        }
                        else
                        {
                            //check internet before calling
                            if (Constants.ConnectedToInternet())
                                await _quoteLineItemService.AddQuoteLineItem(quote, quoteLineItem);
                        }
                    }


                    // Need to Make Post Service call to send quote lineitems to server.
                    //quote = await _quoteService.PostQuote(quote);
                    //_navigationService.Navigate("QuoteDetails", quoteLineItem.QuoteId);

                    PageUtils.SelectedQuoteId = string.Empty;
                    PageUtils.CamefromQuoteDetails = false;

                    var Frame = Window.Current.Content as Frame;
                    //InProgress = false;
                    if (Frame != null)
                    {
                        while (Frame.CanGoBack)
                        {
                            if (Frame.CurrentSourcePageType.Name == "QuoteDetailsPage")
                                break;
                            Frame.GoBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewSeeMoreItemClick(object sender, object parameter)
        {
              var arg = (parameter as ItemClickEventArgs).ClickedItem as SalesHistory;

              if (arg != null)
              {
                  if (arg.InvoiceNumber == DataAccessUtils.SeeMore)
                  {
                      _navigationService.Navigate("RecentOrders", _productDetail);
                  }
              }
        }

        ///// <summary>
        ///// Navigate to QuoteDetails on customer selection
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="parameter"></param>
        //public void ListViewItemClick(object sender, object parameter)
        //{
        //    var customerId = (((parameter as ItemClickEventArgs).ClickedItem as Customer).CustomerId);
        //    _navigationService.Navigate("QuoteDetails", customerId);

        //}

        // Need to remove commented code.
        ///// <summary>
        /////     TextChanged event to get entered quantity
        ///// </summary>
        ///// <param name="args"></param>
        //private void TextBoxTextChanged(object args)
        //{
        //    EmptyText = false;
        //    if (((TextBox) args).Text != null && ((TextBox) args).Text != string.Empty)
        //    {
        //       // EnteredQuantity = Convert.ToInt32(((TextBox) args).Text.Trim());
        //        EnteredQuantity = ((TextBox)args).Text.Trim();
        //        ProductDetails.Quantity = Convert.ToInt32(EnteredQuantity);
        //    }
        //}

        /// <summary>
        ///     TextChanged event to get entered quantity
        /// </summary>
        /// <param name="args"></param>
        private void QunatityTextBoxGotFocus(object args)
        {
            EmptyText = false;
            if (((TextBox)args).Text != null && ((TextBox)args).Text != string.Empty)
            {
                int enteredQnty = Convert.ToInt32(((TextBox)args).Text.Trim());
                if (enteredQnty > 0)
                {
                    //EnteredQuantity = Convert.ToInt32(((TextBox)args).Text.Trim());
                    //ProductDetails.Quantity = EnteredQuantity;

                    EnteredQuantity = ((TextBox)args).Text.Trim();
                    ProductDetails.Quantity = Convert.ToInt32(EnteredQuantity);
                }
                else
                {
                    EnteredQuantity = string.Empty;
                }
            }
            else
            {
                EnteredQuantity = string.Empty;
            }
        }
        
              /// <summary>
        ///     TextChanged event to get entered quantity
        /// </summary>
        /// <param name="args"></param>
        private void QunatityTextBoxLostFocus(object args)
        {
            EmptyText = false;
            if (((TextBox)args).Text != null && ((TextBox)args).Text != string.Empty)
            {
            //    EnteredQuantity = Convert.ToInt32(((TextBox)args).Text.Trim());
            //    ProductDetails.Quantity = EnteredQuantity;
                EnteredQuantity = ((TextBox)args).Text.Trim();
                ProductDetails.Quantity = Convert.ToInt32(EnteredQuantity);
            }
            else
            {
                EnteredQuantity = "0";
            }
        }

        private async Task DecrementCount()
        {
            try
            {
                if (!string.IsNullOrEmpty(EnteredQuantity))
                {
                    int enteredQnty = Convert.ToInt32(EnteredQuantity);
                    if (enteredQnty > 0)
                    {
                        enteredQnty -= 1;
                        EnteredQuantity = enteredQnty.ToString();
                    }
                    DecrementCountCommand.RaiseCanExecuteChanged();
                }
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
                int enteredQnty = Convert.ToInt32(EnteredQuantity);
                enteredQnty += 1;
                EnteredQuantity = enteredQnty.ToString();
                DecrementCountCommand.RaiseCanExecuteChanged();
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