using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Repositories;

namespace SageMobileSales.UILogic.ViewModels
{
    public class ContactsPageViewModel : ViewModel
    {
        private readonly IContactRepository _contactRepository;
        private readonly INavigationService _navigationService;


        private List<Contact> _customerContactList;
        private string _customerId;
        private string _log = string.Empty;

        public ContactsPageViewModel(INavigationService navigationService, IContactRepository contactRepository)
        {
            _navigationService = navigationService;
            _contactRepository = contactRepository;
        }

        public List<Contact> CustomerContactList
        {
            get { return _customerContactList; }
            private set { SetProperty(ref _customerContactList, value); }
        }

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                _customerId = navigationParameter as string;
                CustomerContactList = await _contactRepository.GetContactDetailsAsync(_customerId);
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