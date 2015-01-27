using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Windows.UI.Xaml.Controls;

namespace SageMobileSales.UILogic.ViewModels
{
    public class PlaceOrderPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IQuoteLineItemRepository _quoteLineItemRepository;
        private readonly IQuoteRepository _quoteRepository;
        private CustomerDetails _customerDtls;
        private bool _inProgress;
        private string _log = string.Empty;
        private List<PaymentType> _paymentMethods;
        private QuoteDetails _quoteDetails;
        private Quote _quoteDtls;
        private List<LineItemDetails> _quoteLineItemsList;
        private IQuoteService _quoteService;
        private ISalesRepRepository _salesRepRepository;
        private PaymentType _selectedtype;
        private ShippingAddressDetails _shippingAddressDtls;

        public PlaceOrderPageViewModel(INavigationService navigationService, ICustomerRepository customerRepository,
            IQuoteRepository quoteRepository, ISalesRepRepository salesRepRepository,
            IAddressRepository addressRepository, IQuoteService quoteService, IOrderService orderService,
            IQuoteLineItemRepository quoteLineItemRepository, IOrderRepository orderRepository)
        {
            _navigationService = navigationService;
            _customerRepository = customerRepository;
            _quoteRepository = quoteRepository;
            _quoteService = quoteService;
            _addressRepository = addressRepository;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _quoteLineItemRepository = quoteLineItemRepository;
            _salesRepRepository = salesRepRepository;
            ConfirmCommand = DelegateCommand.FromAsyncHandler(PlaceOrder);
            PaymentListViewSelectionChanged = new DelegateCommand<object>(SelectedPaymentType);
            BindItemsToListView();
        }

        public QuoteDetails QuoteDetails
        {
            get { return _quoteDetails; }
            set { SetProperty(ref _quoteDetails, value); }
        }

        public Quote QuoteDtls
        {
            get { return _quoteDtls; }
            private set { SetProperty(ref _quoteDtls, value); }
        }

        public CustomerDetails CustomerDtls
        {
            get { return _customerDtls; }
            private set { SetProperty(ref _customerDtls, value); }
        }

        public ShippingAddressDetails ShippingAddressDtls
        {
            get { return _shippingAddressDtls; }
            private set { SetProperty(ref _shippingAddressDtls, value); }
        }

        /// <summary>
        ///     List for selecting method of creating a quote
        /// </summary>
        public List<PaymentType> PaymentMethods
        {
            get { return _paymentMethods; }
            private set { SetProperty(ref _paymentMethods, value); }
        }

        /// <summary>
        ///     List for selecting method of creating a quote
        /// </summary>
        public PaymentType SelectedType
        {
            get { return _selectedtype; }
            private set { SetProperty(ref _selectedtype, value); }
        }

        public decimal Total
        {
            get { return Math.Round(CalculateTotal(), 2); }
        }

        public decimal SubTotal
        {
            get { return Math.Round(CalculateSubTotal(), 2); }
        }

        public decimal DiscountPercentageValue
        {
            get { return CalculateDiscountPercent(); }
        }

        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public DelegateCommand ConfirmCommand { get; private set; }
        public DelegateCommand<object> ShippingAndHandlingTextChangedCommand { get; private set; }
        public DelegateCommand<object> DiscountTextChangedCommand { get; private set; }
        public DelegateCommand<object> PaymentListViewSelectionChanged { get; set; }

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            QuoteDtls = navigationParameter as Quote;
            BindItemsToListView();
            DisplayQuoteDetails();
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        private async void DisplayQuoteDetails()
        {
            try
            {
                InProgress = true;
                QuoteDetails = await _quoteRepository.GetQuoteDetailsAsync(QuoteDtls.QuoteId);
                _quoteLineItemsList = await _quoteLineItemRepository.GetQuoteLineItemDetailsAsync(QuoteDtls.QuoteId);
                CustomerDtls =
                    await _customerRepository.GetCustomerDtlsForQuote(QuoteDtls.CustomerId, QuoteDtls.AddressId);
                ShippingAddressDtls = await _addressRepository.GetShippingAddressDetails(QuoteDtls.AddressId);

                OnPropertyChanged("SubTotal");
                OnPropertyChanged("DiscountPercentageValue");
                OnPropertyChanged("Total");
                InProgress = false;
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
            if (_quoteLineItemsList != null)
            {
                foreach (var quoteLineItem in _quoteLineItemsList)
                {
                    SubTotal += Math.Round((quoteLineItem.LineItemQuantity * quoteLineItem.LineItemPrice), 2);
                }
            }
            return SubTotal;
        }

        private decimal CalculateDiscountPercent()
        {
            decimal discountPercentage = 0;
            if (QuoteDetails != null)
            {
                if (QuoteDetails.DiscountPercent != 0)
                {
                    // discountPercentage = Math.Round(((1 - ((SubTotal - DiscountPercentageValue) / SubTotal)) * 100), 2);
                    discountPercentage = Math.Round(((QuoteDetails.DiscountPercent / 100) * SubTotal), 2);
                }
            }
            return discountPercentage;
        }

        private decimal CalculateTotal()
        {
            decimal Total = 0;
            if (QuoteDtls != null)
            {
                Total += SubTotal - DiscountPercentageValue + QuoteDtls.ShippingAndHandling + QuoteDtls.Tax;
            }
            return Total;
        }

        private async Task PlaceOrder()
        {
            try
            {
                InProgress = true;
                if (Constants.ConnectedToInternet())
                {
                    var order = await _orderService.PostOrder(QuoteDtls);
                    if (order != null)
                    {
                        InProgress = false;
                        _navigationService.ClearHistory();
                        var orderDtls = await _orderRepository.GetOrderDetailsAsync(order.OrderId);
                        _navigationService.Navigate("OrderDetails", orderDtls);
                    }
                    else
                    {
                        InProgress = false;
                        var msgDialog = new MessageDialog(
                            ResourceLoader.GetForCurrentView("Resources").GetString("PlaceOrderErrorText"),
                            ResourceLoader.GetForCurrentView("Resources").GetString("PlaceOrderErrorTitle"));
                        msgDialog.Commands.Add(new UICommand("Ok"));
                        await msgDialog.ShowAsync();
                    }
                }
                else
                {
                    Constants.ShowMessageDialog(
                        ResourceLoader.GetForCurrentView("Resources").GetString("NoInternetConnection"));
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private void SelectedPaymentType(object args)
        {
            var paymentTypeListView = ((ListView)args);
            if (paymentTypeListView != null && paymentTypeListView.SelectedItem != null)
            {
                SelectedType = (PaymentType)paymentTypeListView.SelectedItem;
                //if (SelectedType.payFrom == "On Account")
                //    if (SelectedType.payFrom == "On Account with deposit")
                //        if (SelectedType.payFrom == "Buy now")
            }
        }

        /// <summary>
        ///     Bind Items for Listview to select method for Place Order
        /// </summary>
        private void BindItemsToListView()
        {
            PaymentMethods = new List<PaymentType>();
            PaymentMethods.Add(new PaymentType
            {
                payFrom = ResourceLoader.GetForCurrentView("Resources").GetString("OnAccount"),
                payFromText = ResourceLoader.GetForCurrentView("Resources").GetString("OnAccountSubText")
            });
            PaymentMethods.Add(new PaymentType() { payFrom = ResourceLoader.GetForCurrentView("Resources").GetString("OnAccountWithdeposit"), payFromText = ResourceLoader.GetForCurrentView("Resources").GetString("OnAccountWithdepositSubText") });
            PaymentMethods.Add(new PaymentType() { payFrom = ResourceLoader.GetForCurrentView("Resources").GetString("Buynow"), payFromText = ResourceLoader.GetForCurrentView("Resources").GetString("BuynowSubText") });
        }
    }

    /// <summary>
    ///     class for method of creating quote
    /// </summary>
    public class PaymentType
    {
        public string payFrom { get; set; }
        public string payFromText { get; set; }
    }
}