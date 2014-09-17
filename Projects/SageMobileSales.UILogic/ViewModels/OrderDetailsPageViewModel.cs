using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;

namespace SageMobileSales.UILogic.ViewModels
{
    public class OrderDetailsPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepostiory;
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IOrderLineItemRepository _orderLineItemRepository;
        private readonly IOrderLineItemService _orderLineItemService;
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantService _tenantService;

        private CustomerDetails _customerDtls;
        private bool _inProgress;
        private string _log = string.Empty;
        private string _orderDetailsPageTitle;
        private OrderDetails _orderDtls;
        private List<LineItemDetails> _orderLineItemsList;
        private ShippingAddressDetails _shippingAddress;
        private Address _customerMailingAddress;

        private Tenant _tenant;
        private DataTransferManager dataTransferManager;

        public OrderDetailsPageViewModel(INavigationService navigationService,
            IOrderLineItemService orderLineItemService, ICustomerRepository customerRepository,
            IOrderLineItemRepository orderLineItemRepository, IAddressRepository addressRepostiory,
            ITenantService tenantService, ITenantRepository tenantRepository, IOrderRepository orderRepository)
        {
            _navigationService = navigationService;
            _orderLineItemService = orderLineItemService;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _orderLineItemRepository = orderLineItemRepository;
            _addressRepostiory = addressRepostiory;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            SendMailCommand = DelegateCommand.FromAsyncHandler(SendOrderDetailsMail);
        }


        public DelegateCommand SendMailCommand { get; private set; }

        public ShippingAddressDetails ShippingAddress
        {
            get { return _shippingAddress; }
            private set { SetProperty(ref _shippingAddress, value); }
        }

        public decimal SubTotal
        {
            get { return Math.Round(CalculateSubTotal(), 2); }
        }

        public decimal DiscountPercentageValue
        {
            get { return CalculateDiscountPercent(); }
        }

        //public decimal Total
        //{
        //    get { return Math.Round(CalculateTotal(), 2); }
        //}

        public string OrderDetailsPageTitle
        {
            get { return _orderDetailsPageTitle; }
            private set { SetProperty(ref _orderDetailsPageTitle, value); }
        }

        public CustomerDetails CustomerDtls
        {
            get { return _customerDtls; }
            private set { SetProperty(ref _customerDtls, value); }
        }

        public OrderDetails OrderDtls
        {
            get { return _orderDtls; }
            private set { SetProperty(ref _orderDtls, value); }
        }

        public List<LineItemDetails> OrderLineItemsList
        {
            get { return _orderLineItemsList; }
            private set { SetProperty(ref _orderLineItemsList, value); }
        }

        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                OrderDtls = navigationParameter as OrderDetails;
                DisplayOrderDtls();
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
                dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += OnDataRequested;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
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


        private async void DisplayOrderDtls()
        {
            InProgress = true;
            if (Constants.ConnectedToInternet())
            {
                await _orderLineItemService.SyncOrderLineItems(OrderDtls.OrderId);
            }
            if (string.IsNullOrEmpty(OrderDtls.CustomerId))
                OrderDtls = await _orderRepository.GetOrderDetailsAsync(OrderDtls.OrderId);

            OrderDetailsPageTitle = "Order " + OrderDtls.OrderNumber;
            CustomerDtls = new CustomerDetails();
            ShippingAddress = await _addressRepostiory.GetShippingAddressDetails(OrderDtls.AddressId);
            CustomerDtls = await _customerRepository.GetCustomerDtlsForOrder(OrderDtls);
            _customerMailingAddress = await _addressRepostiory.GetCustomerMailingAddress(CustomerDtls.CustomerId);
            OrderLineItemsList = await _orderLineItemRepository.GetOrderLineItemDetailsAsync(OrderDtls.OrderId);
            OnPropertyChanged("SubTotal");
            OnPropertyChanged("DiscountPercentageValue");
            InProgress = false;
        }

        private decimal CalculateSubTotal()
        {
            decimal SubTotal = 0;

            if (OrderLineItemsList != null)
            {
                foreach (LineItemDetails orderlineItem in OrderLineItemsList)
                {
                    SubTotal += Math.Round((orderlineItem.LineItemPrice*orderlineItem.LineItemQuantity), 2);
                }
            }
            return SubTotal;
        }

        private decimal CalculateDiscountPercent()
        {
            decimal discountPercentage = 0;
            if (OrderDtls != null)
            {
                if (OrderDtls.DiscountPercent != 0 && SubTotal != 0)
                {
                    // discountPercentage = Math.Round(((1 - ((SubTotal - DiscountPercentageValue) / SubTotal)) * 100), 2);
                    discountPercentage = Math.Round(((OrderDtls.DiscountPercent/100)*SubTotal), 2);
                }
            }
            return discountPercentage;
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
            try
            {
                var objMail = new MailViewModel();
                string HtmlContentString = objMail.BuildOrderEmailContent(_tenant, CustomerDtls, _customerMailingAddress, OrderDtls,
                    OrderLineItemsList, SubTotal.ToString(), OrderDtls.Amount.ToString());
                if (!String.IsNullOrEmpty(HtmlContentString))
                {
                    DataPackage requestData = request.Data;
                    requestData.Properties.Title = "Order";
                    requestData.Properties.Description = CustomerDtls.CustomerName;
                    requestData.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(HtmlContentString));
                    succeeded = true;
                }
                else
                {
                    request.FailWithDisplayText("Enter the details you would like to share and try again.");
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            return succeeded;
        }

        private async Task SendOrderDetailsMail()
        {
            try
            {
                if (!InProgress)
                {
                    //await _tenantService.SyncTenant();
                    _tenant = await _tenantRepository.GetTenantDtlsAsync(Constants.TenantId);


                    DataTransferManager.ShowShareUI();
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        //private decimal CalculateTotal()
        //{
        //    decimal Total = 0;
        //    if (OrderDtls != null)
        //    {
        //        Total += SubTotal - OrderDtls.DiscountPercent + OrderDtls.ShippingAndHandling + OrderDtls.Tax;
        //    }
        //    return Total;
        //}

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
    }
}