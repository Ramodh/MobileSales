using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SageMobileSales.UILogic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using SageMobileSales.DataAccess.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    public class CustomerDetailPageViewModel : ViewModel
    {
        public ICommand ContactsNavigationCommand { get; set; }
        public ICommand OtherAddressesNavigationCommand { get; set; }
        public ICommand QuotesNavigationCommand { get; set; }
        public ICommand OrdersNavigationCommand { get; set; }
        

        private INavigationService _navigationService;
        private ICustomerRepository _customerRepository;
        private IContactService _contactService;
        private IAddressRepository _addressRepository;
        private IContactRepository _contactRepository;
        private IQuoteRepository _quoteRepository;
        private IOrderRepository _orderRepository;
        private string _log = string.Empty;
        #region Properties

        private string _customerDetailPageTitle;   
        private string _phone;
        private string _salesThisMonth;
        private string _salesYTD;    
        private bool _inProgress;
        private List<Address> _otherAddresses;
        private Visibility _isOtherAddressesVisible;
        private Visibility _isContactsVisible;
        private Visibility _isOrdersVisible;
        private Visibility _isQuotesVisible;
        private CustomerDetails _customerDtls;
        private List<OrderDetails> _customerOrders;
        private List<QuoteDetails> _customerQuotes;     
        private List<Contact> _customerContactList;
        private List<CustomerDetails> _customerOtherAddress;
        private ISalesRepRepository _salesRepRepository;


        public CustomerDetails CustomerDtls
        {
            get { return _customerDtls; }
            private set 
            {
                SetProperty(ref _customerDtls, value);
            }
        }
        public List<Contact> CustomerContactList
        {
            get { return _customerContactList; }
            private set
            {
                SetProperty(ref _customerContactList, value);               
            }
        }

        public List<Address> OtherAddresses
        {

            get { return _otherAddresses; }
            private set
            {
                SetProperty(ref  _otherAddresses, value);
                //  InProgress = false;

            }
        }


        public string CustomerDetailPageTitle
        {
            get { return _customerDetailPageTitle; }
            private set { SetProperty(ref _customerDetailPageTitle, value); }
        }
        
        public string SalesThisMonth
        {
            get { return _salesThisMonth; }
            private set { SetProperty(ref _salesThisMonth, value); }
        }

        public string SalesYTD
        {
            get { return _salesYTD; }
            private set { SetProperty(ref _salesYTD, value); }
        }
     
        public List<OrderDetails> CustomerOrders
        {
            get { return _customerOrders; }
            private set { SetProperty(ref _customerOrders, value);
         
            }
        }

        public List<QuoteDetails> CustomerQuotes
        {
            get { return _customerQuotes; }
            private set { SetProperty(ref _customerQuotes, value);
           
            }
        }

        public List<CustomerDetails> CustomerOtherAddress
        {
            get { return _customerOtherAddress; }
            private set { SetProperty(ref _customerOtherAddress, value); }

        }
        /// <summary>
        ///checks whether OtherAddresses textblock should be visible or not
        /// </summary>
        public Visibility IsOtherAddressesVisible
        {
            get { return _isOtherAddressesVisible; }
            private set { SetProperty(ref _isOtherAddressesVisible, value); }

        }
        
         /// <summary>
        ///checks whether Contacts textblock should be visible or not
        /// </summary>
        public Visibility IsContactsVisible
        {
            get { return _isContactsVisible; }
            private set { SetProperty(ref _isContactsVisible, value); }

        }
           
         /// <summary>
        ///checks whether Orders textblock should be visible or not
        /// </summary>
        public Visibility IsOrdersVisible
        {
            get { return _isOrdersVisible; }
            private set { SetProperty(ref _isOrdersVisible, value); }

        }
        
         /// <summary>
        ///checks whether Quotes textblock should be visible or not
        /// </summary>
        public Visibility IsQuotesVisible
        {
            get { return _isQuotesVisible; }
            private set { SetProperty(ref _isQuotesVisible, value); }

        }
        #endregion           

        /// <summary>
        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public CustomerDetailPageViewModel(INavigationService navigationService,ICustomerRepository customerRepository, IContactService contactService,
            IContactRepository contactRepository, IAddressRepository addressRepository, IQuoteRepository quoteRepository, ISalesRepRepository salesRepRepository,IOrderRepository orderRepository)
        {
            _navigationService = navigationService;
            _customerRepository = customerRepository;
            _contactService = contactService;
            _contactRepository = contactRepository;
            _addressRepository = addressRepository;
            _quoteRepository = quoteRepository;
            _salesRepRepository = salesRepRepository;
            _orderRepository=orderRepository;            
            ContactsNavigationCommand = new DelegateCommand(NavigateToContacts);
            OtherAddressesNavigationCommand = new DelegateCommand(NavigateToOtherAddresses);
            QuotesNavigationCommand = new DelegateCommand(NavigateToQuotes);
            OrdersNavigationCommand = new DelegateCommand(NavigateToOrders);
        }

        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            CustomerDtls = navigationParameter as CustomerDetails;
            CustomerDetailPageTitle = CustomerDtls.CustomerName;
            DisplayCustomerDetails();
          
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

        }

        private async void DisplayCustomerDetails()
        {
            try
            {
                InProgress = true;                
                OtherAddresses = await _addressRepository.GetOtherAddressesForCustomers(CustomerDtls.CustomerId);
                if (OtherAddresses.Count <= 0)
                {
                    IsOtherAddressesVisible = Visibility.Collapsed;
                }
                else
                {
                    if (OtherAddresses.Count > 10)
                    {
                        OtherAddresses = OtherAddresses.GetRange(0, 10);
                    }
                }
                if (Constants.ConnectedToInternet())
                {
                    await _contactService.SyncOfflineContacts(CustomerDtls.CustomerId);
                    await _contactService.SyncContacts(CustomerDtls.CustomerId);
                }

                CustomerContactList = await _contactRepository.GetContactDetailsAsync(CustomerDtls.CustomerId);

                if (CustomerContactList.Count <= 0)
                {
                    IsContactsVisible = Visibility.Collapsed;
                }
                if (CustomerContactList.Count > 10)
                    CustomerContactList = CustomerContactList.GetRange(0, 10);

                CustomerQuotes = new List<QuoteDetails>();
                CustomerQuotes = await _quoteRepository.GetQuotesForCustomerAsync(CustomerDtls.CustomerId);
                if (CustomerQuotes.Count <= 0)
                {
                    IsQuotesVisible = Visibility.Collapsed;
                }
                if (CustomerQuotes.Count > 10)
                    CustomerQuotes = CustomerQuotes.GetRange(0, 10);

                CustomerOrders = new List<OrderDetails>();
                CustomerOrders = await _orderRepository.GetOrdersForCustomerAsync(CustomerDtls.CustomerId);
                if (CustomerOrders.Count <= 0)
                {
                    IsOrdersVisible = Visibility.Collapsed;
                }
                if (CustomerOrders.Count > 10)
                    CustomerOrders = CustomerOrders.GetRange(0, 10);
                PageUtils.ResetLocalVariables();
                InProgress = false;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        /// Grid View Item Click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewQuoteItemClick(object sender, object parameter)
        {
            try
            {
                QuoteDetails selectedQuotedetails = ((parameter as ItemClickEventArgs).ClickedItem as QuoteDetails);

                if (selectedQuotedetails != null)
                {
                    var quote = await _quoteRepository.GetQuoteAsync(selectedQuotedetails.QuoteId);

                    if (quote != null)
                    {
                        _navigationService.Navigate("QuoteDetails", quote.QuoteId);
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
        /// Grid View Item Click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewOrderItemClick(object sender, object parameter)
        {
            var arg = (parameter as ItemClickEventArgs).ClickedItem as OrderDetails;

            if (arg != null)
                _navigationService.Navigate("OrderDetails", arg);


        }
        /// <summary>
        /// Navigate to Contacts Page where we are displaying all contacts for that Customer.
        /// </summary>
        public void NavigateToContacts()
        {
            _navigationService.Navigate("Contacts", CustomerDtls.CustomerId);
        }

        /// <summary>
        /// Navigate to OtherAddresses Page where we are displaying all OtherAddresses for that Customer.
        /// </summary>
        public void NavigateToOtherAddresses()
        {
            _navigationService.Navigate("OtherAddresses", CustomerDtls.CustomerId);
        }

        
         /// <summary>
        /// Navigate to Quotes Page where we are displaying all Quotes for that Customer.
        /// </summary>
        public void NavigateToQuotes()
        {
            _navigationService.Navigate("Quotes", CustomerDtls);
        }
        /// <summary>
        /// Navigate to Orders Page where we are displaying all Quotes for that Customer.
        /// </summary>
        public void NavigateToOrders()
        {
            _navigationService.Navigate("Orders", CustomerDtls.CustomerId);
        }
        /// <summary>
        ///  //Navigate to Add Contact page on appbar button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void AddContactsButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("AddContact", CustomerDtls.CustomerId);
        }

        /// <summary>
        ///  //Navigate to create quote page on appbar button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void CreateQuoteButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("CreateQuote", CustomerDtls);
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

    //    // TODO
    //    // Replace dummy data with real data.
    //    private void BindCustomerDetails()
    //    {

    //        Address = _customerAddress.Street1 + "\r\n" + _customerAddress.City + " " + _customerAddress.StateProvince 
    //                                           + " " + _customerAddress.PostalCode + "\r\n" + _customerAddress.Phone;
    //        //SalesThisMonth = "600";
    //        //SalesYTD = "9990";
    //        Terms = _customerAddress.PaymentTerms;
    //        CreditLimit = _customerAddress.CreditLimit;
    //        Availablecredit = _customerAddress.CreditAvailable;
    //}

   
    }
}
