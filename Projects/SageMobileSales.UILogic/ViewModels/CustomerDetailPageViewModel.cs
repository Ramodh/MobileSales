using System;
using System.Collections.Generic;
using System.Windows.Input;
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
    public class CustomerDetailPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IContactService _contactService;
        private readonly INavigationService _navigationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IFrequentlyPurchasedItemService _frequentlyPurchasedItemService;
        private readonly IFrequentlyPurchasedItemRepository _frequentlyPurchasedItemRepository;
        private string _log = string.Empty;
        private List<FrequentlyPurchasedItem> _frequentlyPurchasedItems;


        #region Properties

        private List<Contact> _customerContactList;
        private string _customerDetailPageTitle;
        private CustomerDetails _customerDtls;
        private List<OrderDetails> _customerOrders;
        private List<CustomerDetails> _customerOtherAddress;
        private List<QuoteDetails> _customerQuotes;
        private bool _inProgress;
        private Visibility _isContactsVisible;
        private Visibility _isOrdersVisible;
        private Visibility _isOtherAddressesVisible;
        private Visibility _isQuotesVisible;
        private List<Address> _otherAddresses;
        private string _phone;
        private ISalesRepRepository _salesRepRepository;
        private string _salesThisMonth;
        private string _salesYTD;
        private Visibility _isFrequentlyPurchasedItemsVisible;

        public CustomerDetails CustomerDtls
        {
            get { return _customerDtls; }
            private set { SetProperty(ref _customerDtls, value); }
        }

        /// <summary>
        ///     checks whether Contacts textblock should be visible or not
        /// </summary>
        public Visibility IsFrequentlyPurchasedItemsVisible
        {
            get { return _isFrequentlyPurchasedItemsVisible; }
            private set { SetProperty(ref _isFrequentlyPurchasedItemsVisible, value); }
        }
        public List<Contact> CustomerContactList
        {
            get { return _customerContactList; }
            private set { SetProperty(ref _customerContactList, value); }
        }

        public List<Address> OtherAddresses
        {
            get { return _otherAddresses; }
            private set
            {
                SetProperty(ref _otherAddresses, value);
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
            private set { SetProperty(ref _customerOrders, value); }
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
        ///     checks whether OtherAddresses textblock should be visible or not
        /// </summary>
        public Visibility IsOtherAddressesVisible
        {
            get { return _isOtherAddressesVisible; }
            private set { SetProperty(ref _isOtherAddressesVisible, value); }
        }

        /// <summary>
        ///     checks whether Contacts textblock should be visible or not
        /// </summary>
        public Visibility IsContactsVisible
        {
            get { return _isContactsVisible; }
            private set { SetProperty(ref _isContactsVisible, value); }
        }

        /// <summary>
        ///     checks whether Orders textblock should be visible or not
        /// </summary>
        public Visibility IsOrdersVisible
        {
            get { return _isOrdersVisible; }
            private set { SetProperty(ref _isOrdersVisible, value); }
        }

        /// <summary>
        ///     checks whether Quotes textblock should be visible or not
        /// </summary>
        public Visibility IsQuotesVisible
        {
            get { return _isQuotesVisible; }
            private set { SetProperty(ref _isQuotesVisible, value); }
        }

        #endregion

        public CustomerDetailPageViewModel(INavigationService navigationService, ICustomerRepository customerRepository,
            IContactService contactService, IContactRepository contactRepository,
            IAddressRepository addressRepository, IQuoteRepository quoteRepository,
            ISalesRepRepository salesRepRepository, IOrderRepository orderRepository,
            IFrequentlyPurchasedItemService frequentlyPurchasedItemService,
            IFrequentlyPurchasedItemRepository frequentlyPurchasedItemRepository)
        {
            _navigationService = navigationService;
            _customerRepository = customerRepository;
            _contactService = contactService;            
            _contactRepository = contactRepository;
            _addressRepository = addressRepository;
            _quoteRepository = quoteRepository;
            _salesRepRepository = salesRepRepository;
            _orderRepository = orderRepository;
            _frequentlyPurchasedItemService = frequentlyPurchasedItemService;
            _frequentlyPurchasedItemRepository = frequentlyPurchasedItemRepository;
            ContactsNavigationCommand = new DelegateCommand(NavigateToContacts);
            OtherAddressesNavigationCommand = new DelegateCommand(NavigateToOtherAddresses);
            QuotesNavigationCommand = new DelegateCommand(NavigateToQuotes);
            OrdersNavigationCommand = new DelegateCommand(NavigateToOrders);
            FrequentlyPurchasedItemsNavigationCommand = new DelegateCommand(NavigateToFrequentlyPurchasedItems);
        }

        public ICommand ContactsNavigationCommand { get; set; }
        public ICommand OtherAddressesNavigationCommand { get; set; }
        public ICommand QuotesNavigationCommand { get; set; }
        public ICommand OrdersNavigationCommand { get; set; }
        public ICommand FrequentlyPurchasedItemsNavigationCommand { get; set; }

        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public List<FrequentlyPurchasedItem> FrequentlyPurchasedItems
        {
            get { return _frequentlyPurchasedItems; }
            private set
            {
                SetProperty(ref _frequentlyPurchasedItems, value);
                OnPropertyChanged("FrequentlyPurchasedItems");
            }
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            CustomerDtls = navigationParameter as CustomerDetails;
            CustomerDetailPageTitle = CustomerDtls.CustomerName;
            PageUtils.CamefromCustomerDetails = true;
            DisplayCustomerDetails();
            //GetFrequentlyPurchasedItems();
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        private async void DisplayCustomerDetails()
        {
            try
            {
                InProgress = true;
              List<Address> otherAddresses = await _addressRepository.GetOtherAddressesForCustomers(CustomerDtls.CustomerId);
              if (otherAddresses.Count <= 0)
                {
                    IsOtherAddressesVisible = Visibility.Collapsed;
                }
              if (otherAddresses.Count > 7)
              {
                  otherAddresses = otherAddresses.GetRange(0, 7);
                  otherAddresses.Add(new Address() { PostalCode = DataAccessUtils.SeeMore });
            
              }
              OtherAddresses = otherAddresses;
               

                if (Constants.ConnectedToInternet())
                {
                    await _contactService.SyncOfflineContacts(CustomerDtls.CustomerId);
                    await _contactService.SyncContacts(CustomerDtls.CustomerId);

                    //FrequentlyPurchasedItems
                    await _frequentlyPurchasedItemService.SyncFrequentlyPurchasedItems(CustomerDtls.CustomerId);
                }

               List<Contact> customerContactList = await _contactRepository.GetContactDetailsAsync(CustomerDtls.CustomerId);
               if (customerContactList.Count <= 0)
               {
                   IsContactsVisible = Visibility.Collapsed;
               }
               if (customerContactList.Count > 7)
               {
                   customerContactList = customerContactList.GetRange(0, 7);
                   customerContactList.Add(new Contact() { EmailPersonal = DataAccessUtils.SeeMore });
                  
               }
               CustomerContactList = customerContactList;
               List<FrequentlyPurchasedItem> frequentlyPurchasedItems = await _frequentlyPurchasedItemRepository.GetFrequentlyPurchasedItems(CustomerDtls.CustomerId);
               if (frequentlyPurchasedItems.Count <= 0)
                {
                    IsFrequentlyPurchasedItemsVisible = Visibility.Collapsed;
                }
               if (frequentlyPurchasedItems.Count > 7)
               {
                   frequentlyPurchasedItems = frequentlyPurchasedItems.GetRange(0, 7);
                   frequentlyPurchasedItems.Add(new FrequentlyPurchasedItem() { ItemDescription = DataAccessUtils.SeeMore });
                  
               }
               FrequentlyPurchasedItems = frequentlyPurchasedItems;

               // CustomerQuotes = new List<QuoteDetails>();
                List<QuoteDetails> customerQuotes = await _quoteRepository.GetQuotesForCustomerAsync(CustomerDtls.CustomerId);
                if (customerQuotes.Count <= 0)
                {
                    IsQuotesVisible = Visibility.Collapsed;
                }
                if (customerQuotes.Count > 7)
                {
                    customerQuotes = customerQuotes.GetRange(0, 7);
                    customerQuotes.Add(new QuoteDetails() { QuoteStatus = DataAccessUtils.SeeMore });
                }
                CustomerQuotes = customerQuotes;

               List<OrderDetails> customerOrders = new List<OrderDetails>();
                customerOrders = await _orderRepository.GetOrdersForCustomerAsync(CustomerDtls.CustomerId);
                if (customerOrders.Count <= 0)
                {
                    IsOrdersVisible = Visibility.Collapsed;
                }
                if (customerOrders.Count > 7)
                {
                    customerOrders = customerOrders.GetRange(0, 7);
                    customerOrders.Add(new OrderDetails() { OrderDescription = DataAccessUtils.SeeMore });
                  
                }
                CustomerOrders = customerOrders;
             
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
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewQuoteItemClick(object sender, object parameter)
        {

             try
            {
                var selectedQuotedetails = ((parameter as ItemClickEventArgs).ClickedItem as QuoteDetails);

                if (selectedQuotedetails != null)
                {
                    if (selectedQuotedetails.QuoteStatus == DataAccessUtils.SeeMore)
                    {
                        NavigateToQuotes();
                    }
                    else
                    {
                        Quote quote = await _quoteRepository.GetQuoteAsync(selectedQuotedetails.QuoteId);

                        if (quote != null)
                        {
                            _navigationService.Navigate("QuoteDetails", quote.QuoteId);
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
        public void GridViewOrderItemClick(object sender, object parameter)
        {
            var arg = (parameter as ItemClickEventArgs).ClickedItem as OrderDetails;

            if (arg != null)
            {
                if (arg.OrderDescription == DataAccessUtils.SeeMore)
                {
                    NavigateToOrders();
                }
                else
                {
                    _navigationService.Navigate("OrderDetails", arg);
                }
            }
        }

        /// <summary>
        ///     Navigate to Contacts Page where we are displaying all contacts for that Customer.
        /// </summary>
        public void NavigateToContacts()
        {
            _navigationService.Navigate("Contacts", CustomerDtls.CustomerId);
        }

        /// <summary>
        ///     Navigate to OtherAddresses Page where we are displaying all OtherAddresses for that Customer.
        /// </summary>
        public void NavigateToOtherAddresses()
        {
            _navigationService.Navigate("OtherAddresses", CustomerDtls.CustomerId);
        }


        /// <summary>
        ///     Navigate to Quotes Page where we are displaying all Quotes for that Customer.
        /// </summary>
        public void NavigateToQuotes()
        {
            _navigationService.Navigate("Quotes", CustomerDtls);
        }

        /// <summary>
        ///     Navigate to Orders Page where we are displaying all Quotes for that Customer.
        /// </summary>
        public void NavigateToOrders()
        {
            _navigationService.Navigate("Orders", CustomerDtls.CustomerId);
        }
        /// <summary>
        ///     Navigate To Frequently Purchased Items where we are displaying Frequently Purchased Items for that Customer.
        /// </summary>
        public void NavigateToFrequentlyPurchasedItems()
        {
            _navigationService.Navigate("FrequentlyPurchasedItems", CustomerDtls.CustomerId);
        }


        /// <summary>
        ///     //Navigate to Add Contact page on appbar button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void AddContactsButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("AddContact", CustomerDtls.CustomerId);
        }

        /// <summary>
        ///     //Navigate to create quote page on appbar button click
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


      

        /// <summary>
        ///Navigate to Item Detail page on grid view item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewFrequentlyPurchasedItemClick(object sender, object parameter)
        {
            try
            {
                var arg = ((parameter as ItemClickEventArgs).ClickedItem as FrequentlyPurchasedItem);

                _navigationService.Navigate("ItemDetail", arg.ItemId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
        /// <summary>
        ///     Grid View Item Click for contacts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewContactItemClick(object sender, object parameter)
        {
            var arg = (parameter as ItemClickEventArgs).ClickedItem as Contact;

            if (arg != null)
                if (arg.EmailPersonal == DataAccessUtils.SeeMore)
                {
                    NavigateToContacts();
                }
        }
        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewOtherAddresstItemClick(object sender, object parameter)
        {
            var arg = (parameter as ItemClickEventArgs).ClickedItem as Address;

            if (arg != null)
                if (arg.AddressName == DataAccessUtils.SeeMore)
                {
                    NavigateToOtherAddresses();
                }
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