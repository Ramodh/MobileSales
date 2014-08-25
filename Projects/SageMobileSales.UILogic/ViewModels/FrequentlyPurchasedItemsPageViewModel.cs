﻿using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SageMobileSales.UILogic.ViewModels
{
    class FrequentlyPurchasedItemsPageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private readonly ITenantRepository _tenantRepository;
        private List<FrequentlyPurchasedItems> _frequentlyPurchasedItems;
        private string _customerId;
        private string _customerName;
        private string _log = string.Empty;
        private string _frequentlyPurchasedItemsPageTitle = string.Empty;

        public FrequentlyPurchasedItemsPageViewModel(INavigationService navigationService, ICustomerRepository customerRepository,
            ISyncCoordinatorService syncCoordinatorService, IEventAggregator eventAggregator,
            ISalesRepService salesRepService, ITenantRepository tenantRepository)
        {
            _navigationService = navigationService;
            _customerRepository = customerRepository;
            _eventAggregator = eventAggregator;
            _syncCoordinatorService = syncCoordinatorService;
            _salesRepService = salesRepService;
            _tenantRepository = tenantRepository;

        }

        public List<FrequentlyPurchasedItems> FrequentlyPurchasedItems
        {
            get { return _frequentlyPurchasedItems; }
            private set
            {
                SetProperty(ref _frequentlyPurchasedItems, value);
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
        ///     Holds Customer Name
        /// </summary>
        public string FrequentlyPurchasedItemsPageTitle
        {
            get { return _customerName; }
            private set { SetProperty(ref _customerName, value); }
        }


        public async override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
          Dictionary<string, object> viewModelState)
        {
            try
            {

                _customerId = navigationParameter as string;
                
                GetFrequentlyPurchasedItems();

                Customer customer = await _customerRepository.GetCustomerDataAsync(_customerId);
                CustomerName = customer.CustomerName;
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///Navigate to Item Detail page on grid view item click
        ///TODO 
        ///To be replaced by real data(productId)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewItemClick(object sender, object parameter)
        {
            try
            {
                var arg = ((parameter as ItemClickEventArgs).ClickedItem as FrequentlyPurchasedItems);

                _navigationService.Navigate("ItemDetail", "3ae2e78d-4dfc-4441-b41c-2487cbef3561");
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
            _navigationService.Navigate("Customers", null);
        }

        /// TODO
        /// Replace dummy data with real data.
        private void GetFrequentlyPurchasedItems()
        {
            FrequentlyPurchasedItems = new List<FrequentlyPurchasedItems>();

            for (int i = 0; i < 20; i++)
            {
                FrequentlyPurchasedItems obj = new FrequentlyPurchasedItems();
                obj.ItemNo = "1" + i;
                obj.ItemName = "ProductName" + i;
                obj.QuantityYTD = 334 + i;
                obj.PriorYTD = 1 + i;
                FrequentlyPurchasedItems.Add(obj);
            }
            OnPropertyChanged("FrequentlyPurchasedItems");
        }
    }

}