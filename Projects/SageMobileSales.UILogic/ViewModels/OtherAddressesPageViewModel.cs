using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SageMobileSales.UILogic.ViewModels
{
    public class OtherAddressesPageViewModel : ViewModel
    {
        private INavigationService _navigationService;
        private IAddressRepository _addressRepository;
        private bool _gridViewItemClickable;
        private List<CustomerDetails> _customerOtherAddress;
        private Visibility _bottomAppbarVisible;
        private string _emptyAddress;
        private QuoteDetails _quoteDetails;
        private string _customerId;
        private bool _inProgress;
        private Quote _quote;
        private Address _address;
        private IQuoteRepository _quoteRepository;
        private List<Address> _customerAddresses;
        private IQuoteService _quoteService;
        private List<Address> _otherAddresses;
        private string _log = string.Empty;

        /// <summary>
        /// Checks whether grid view item is clickable or not
        /// </summary>
        public bool GridViewItemClickable
        {
            get { return _gridViewItemClickable; }
            private set { SetProperty(ref _gridViewItemClickable, value); }
        }
        /// <summary>
        /// Checks whether Bottom Appbar is Visible 
        /// </summary>
        public Visibility BottomAppbarVisible
        {
            get { return _bottomAppbarVisible; }
            private set { SetProperty(ref _bottomAppbarVisible, value); }

        }

        /// <summary>
        /// Holds Customer Other Addresses 
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
                SetProperty(ref  _otherAddresses, value);
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
        /// Display empty results text
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

        public OtherAddressesPageViewModel(INavigationService navigationService, IAddressRepository addressRepository, IQuoteRepository quoteRepository, IQuoteService quoteService)
        {
            _navigationService = navigationService;
            _addressRepository = addressRepository;
            _quoteRepository = quoteRepository;
            _quoteService = quoteService;
            _address = new Address();
        }

        /// <summary>
        ///Loads other Addresses for customer if present
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public async override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            try
            {
                InProgress = true;
                Frame rootFrame = Window.Current.Content as Frame;
                List<PageStackEntry> navigationHistory = rootFrame.BackStack.ToList();
                PageStackEntry pageStack = navigationHistory.LastOrDefault();
                if (pageStack.SourcePageType.Name.ToString() == "QuoteDetailsPage")
                {
                    _quoteDetails = navigationParameter as QuoteDetails;

                    //  _customerId = _quoteDetails.CustomerId;
                    GridViewItemClickable = true;
                    BottomAppbarVisible = Visibility.Visible;
                    CustomerAddresses = await _addressRepository.GetAddressesForCustomer(_quoteDetails.CustomerId);
                }
                else
                {
                    _customerId = navigationParameter as string;
                    GridViewItemClickable = false;
                    BottomAppbarVisible = Visibility.Collapsed;
                    CustomerAddresses = await _addressRepository.GetOtherAddressesForCustomers(_customerId);

                    //CustomerOtherAddress = await _addressRepository.GetOtherAddressesForCustomer(_customerId);
                }

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
        /// Grid View Item Click 
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
        /// Navigates to create shipping address Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void CreateShippingAddressButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("CreateShippingAddress", _quoteDetails.QuoteId);
        }
    }
}
