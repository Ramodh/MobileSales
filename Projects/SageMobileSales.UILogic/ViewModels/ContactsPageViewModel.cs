using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;

namespace SageMobileSales.UILogic.ViewModels
{
    public class ContactsPageViewModel : ViewModel
    {
        private readonly IContactRepository _contactRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INavigationService _navigationService;
        private List<Contact> _customerContactList;
        private string _customerId;
        private string _customerName;
        private string _log = string.Empty;

        public ContactsPageViewModel(INavigationService navigationService, IContactRepository contactRepository,
            ICustomerRepository customerRepository)
        {
            _navigationService = navigationService;
            _contactRepository = contactRepository;
            _customerRepository = customerRepository;
        }

        public List<Contact> CustomerContactList
        {
            get { return _customerContactList; }
            private set { SetProperty(ref _customerContactList, value); }
        }

        /// <summary>
        ///     Holds Customer Name
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                _customerId = navigationParameter as string;

                CustomerContactList = await _contactRepository.GetContactDetailsAsync(_customerId);
                var customer = await _customerRepository.GetCustomerDataAsync(_customerId);
                CustomerName = ResourceLoader.GetForCurrentView("Resources").GetString("SeperatorSymbol") +
                               customer.CustomerName;
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     //Navigate to Add Contact page on appbar button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void AddContactsButton_Click(object sender, object parameter)
        {
            _navigationService.Navigate("AddContact", _customerId);
        }
    }
}