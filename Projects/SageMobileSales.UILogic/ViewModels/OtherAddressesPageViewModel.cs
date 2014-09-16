using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
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

namespace SageMobileSales.UILogic.ViewModels
{
    public class OtherAddressesPageViewModel : ViewModel
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private readonly IQuoteRepository _quoteRepository;
        private readonly IQuoteService _quoteService;
        private Address _address;
        private Visibility _bottomAppbarVisible;
        private Customer _customer;
        private List<Address> _customerAddresses;
        private string _customerId;
        private string _customerName;
        private List<CustomerDetails> _customerOtherAddress;
        private string _emptyAddress;
        private bool _gridViewItemClickable;
        private bool _inProgress;
        private string _log = string.Empty;
        private List<Address> _otherAddresses;
        private Quote _quote;
        private QuoteDetails _quoteDetails;

        public OtherAddressesPageViewModel(INavigationService navigationService, IAddressRepository addressRepository,
            IQuoteRepository quoteRepository, IQuoteService quoteService, ICustomerRepository customerRepository)
        {
            _navigationService = navigationService;
            _addressRepository = addressRepository;
            _quoteRepository = quoteRepository;
            _quoteService = quoteService;
            _customerRepository = customerRepository;
            _address = new Address();
        }

        /// <summary>
        ///     Checks whether grid view item is clickable or not
        /// </summary>
        public bool GridViewItemClickable
        {
            get { return _gridViewItemClickable; }
            private set { SetProperty(ref _gridViewItemClickable, value); }
        }

        /// <summary>
        ///     Checks whether Bottom Appbar is Visible
        /// </summary>
        public Visibility BottomAppbarVisible
        {
            get { return _bottomAppbarVisible; }
            private set { SetProperty(ref _bottomAppbarVisible, value); }
        }

        /// <summary>
        ///     Holds Customer Other Addresses
        /// </summary>
        public List<CustomerDetails> CustomerOtherAddress
        {
            get { return _customerOtherAddress; }
            private set
            {
                SetProperty(ref _customerOtherAddress, value);
                InProgress = false;
            }
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

        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyAddresses
        {
            get { return _emptyAddress; }
            private set { SetProperty(ref _emptyAddress, value); }
        }

        public List<Address> CustomerAddresses
        {
            get { return _customerAddresses; }
            private set
            {
                SetProperty(ref _customerAddresses, value);
                InProgress = false;
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
        ///     Loads other Addresses for customer if present
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
                var rootFrame = Window.Current.Content as Frame;
                List<PageStackEntry> navigationHistory = rootFrame.BackStack.ToList();
                PageStackEntry pageStack = navigationHistory.LastOrDefault();
                if (pageStack.SourcePageType.Name == "QuoteDetailsPage")
                {
                    _quoteDetails = navigationParameter as QuoteDetails;

                    //  _customerId = _quoteDetails.CustomerId;
                    GridViewItemClickable = true;
                    BottomAppbarVisible = Visibility.Visible;
                    CustomerAddresses = await _addressRepository.GetAddressesForCustomer(_quoteDetails.CustomerId);
                    _customer = await _customerRepository.GetCustomerDataAsync(_quoteDetails.CustomerId);
                }
                else
                {
                    _customerId = navigationParameter as string;
                    GridViewItemClickable = false;
                    BottomAppbarVisible = Visibility.Collapsed;
                    CustomerAddresses = await _addressRepository.GetOtherAddresses(_customerId);
                    _customer = await _customerRepository.GetCustomerDataAsync(_customerId);
                    //CustomerOtherAddress = await _addressRepository.GetOtherAddressesForCustomer(_customerId);
                }

                CustomerName = ResourceLoader.GetForCurrentView("Resources").GetString("SeperatorSymbol") +
                               _customer.CustomerName;

                if (!(CustomerAddresses.Count > 0))
                {
                    EmptyAddresses = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyAddressesText");
                }
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
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
        public async void GridViewItemClick(object sender, object parameter)
        {
            try
            {
                InProgress = true;
                var address = ((parameter as ItemClickEventArgs).ClickedItem as Address);

                _quote = await _quoteRepository.GetQuoteAsync(_quoteDetails.QuoteId);

                _quote.AddressId = address.AddressId;
                await _addressRepository.UpdateAddressToDbAsync(address);

                _quote.IsPending = true;
                await _quoteRepository.UpdateQuoteToDbAsync(_quote);
                if (Constants.ConnectedToInternet())
                {
                    await _quoteService.UpdateQuoteShippingAddressKey(_quote);
                }

                _navigationService.Navigate("QuoteDetails", _quoteDetails.QuoteId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Navigates to create shipping address Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void CreateShippingAddressButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("CreateShippingAddress", _quoteDetails.QuoteId);
        }
    }
}