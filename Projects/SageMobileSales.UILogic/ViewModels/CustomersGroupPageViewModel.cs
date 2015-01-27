using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SageMobileSales.UILogic.Model;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CustomersGroupPageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        //private CustomerCollection _customerCollection;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantService _tenantService;
        private string _emptyCustomers;
        private List<CustomerGroupByAlphabet> _groupedCustomerList;
        private bool _inProgress;
        private string _log = string.Empty;
        private bool _syncProgress;
        private bool _tenantSync = true;

        public CustomersGroupPageViewModel(INavigationService navigationService, ICustomerRepository customerRepository,
            ISyncCoordinatorService syncCoordinatorService, IEventAggregator eventAggregator,
            ISalesRepService salesRepService, ITenantRepository tenantRepository, ITenantService tenantService,
            ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _navigationService = navigationService;
            _customerRepository = customerRepository;
            _eventAggregator = eventAggregator;
            _syncCoordinatorService = syncCoordinatorService;
            _salesRepService = salesRepService;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            _localSyncDigestRepository = localSyncDigestRepository;
            StartSyncCommand = DelegateCommand.FromAsyncHandler(StartSync);
            _eventAggregator.GetEvent<CustomerDataChangedEvent>().Subscribe(UpdateCustomerList, ThreadOption.UIThread);
            _eventAggregator.GetEvent<CustomerSyncChangedEvent>()
                .Subscribe(CustomersSyncIndicator, ThreadOption.UIThread);
        }

        public DelegateCommand StartSyncCommand { get; private set; }

        /// <summary>
        ///     Collection to support incremental scrolling
        /// </summary>
        /// <summary>
        ///     Customer list
        /// </summary>
        public List<CustomerGroupByAlphabet> GroupedCustomerList
        {
            get { return _groupedCustomerList; }
            private set
            {
                SetProperty(ref _groupedCustomerList, value);
                OnPropertyChanged("GroupedCustomerList");

                if (_groupedCustomerList.Count > 0)
                    InProgress = false;
            }
        }

        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        /// <summary>
        ///     Data syncing indicator
        /// </summary>
        public bool SyncProgress
        {
            get { return _syncProgress; }
            private set
            {
                SetProperty(ref _syncProgress, value);
                OnPropertyChanged("SyncProgress");
            }
        }

        /// <summary>
        ///     Tenant syncing
        /// </summary>
        public bool IsTenantSyncing
        {
            get { return _tenantSync; }
            private set
            {
                SetProperty(ref _tenantSync, value);
                OnPropertyChanged("IsTenantSyncing");
            }
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyCustomers
        {
            get { return _emptyCustomers; }
            private set { SetProperty(ref _emptyCustomers, value); }
        }

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                await StartSync();
                //InProgress = true;

                //if (!Constants.SyncProgress)
                //{
                //    IAsyncAction asyncActionCommon = ThreadPool.RunAsync(
                //        IAsyncAction => { _syncCoordinatorService.StartSync(); });

                //    //asyncActionCommon.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                //    //{
                //    //    if (asyncStatus == AsyncStatus.Canceled)
                //    //        return;

                //    //    Constants.SyncProgress = false;
                //    //});
                //    PageUtils.asyncActionCommon = asyncActionCommon;
                //}

                //SyncProgress = Constants.CustomersSyncProgress;
                //List<CustomerDetails> CustomerAdressList = await _customerRepository.GetCustomerListDtlsAsync();

                ////var queryFirstLetters = from item in CustomerAdressList
                ////                        orderby ((CustomerAddress)item).CustomerName
                ////                        group item by item.CustomerName[0];                   

                ////var query = from item in CustomerAdressList
                ////            orderby ((CustomerAddress)item).CustomerName
                ////            group item by new { ((CustomerAddress)item).CustomerName } into g
                ////            select new { GroupName = g.Key.CustomerName[0], Items = g };

                ////var query = from customer in CustomerAdressList
                ////            let c = customer.CustomerName[0]
                ////            group customer by c >= '0' && c <= '9' ? '0' : char.ToUpper(c);

                ////List<CustomerGroupByAlphabet> sortedCustomerAdressList = CustomerAdressList
                ////    .GroupBy(alphabet => alphabet.CustomerName[0])
                ////    .OrderBy(g => g.Key)
                ////    .Select(g => new CustomerGroupByAlphabet { GroupName = g.Key, CustomerAddressList = g.ToList() })
                ////    .ToList();

                //List<CustomerGroupByAlphabet> sortedCustomerAdressList = CustomerAdressList
                //    .GroupBy(alphabet => char.ToUpper(alphabet.CustomerName[0]))
                //    .OrderBy(g => g.Key)
                //    .Select(g => new CustomerGroupByAlphabet { GroupName = g.Key, CustomerAddressList = g.ToList() })
                //    .ToList();

                //GroupedCustomerList = sortedCustomerAdressList;

                ////CustomerCollection = new CustomerCollection()
                ////{
                ////    CustomerAdressList = sortedCustomerAdressList
                ////};

                ////if (!(CustomerCollection.CustomerAdressList.Count > 0))
                ////{
                ////    EmptyCustomers = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyProductsText");
                ////}            

                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
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
            //_navigationService.ClearHistory();
            //_navigationService.Navigate("Customers", null);
        }

        public void GridViewItemClick(object sender, object parameter)
        {
            var arg = ((parameter as ItemClickEventArgs).ClickedItem as CustomerDetails);
            _navigationService.Navigate("CustomerDetail", arg);
        }

        public async void UpdateCustomerList(bool updated)
        {
            await UpdateCustomerListInfo();
        }

        public async Task UpdateCustomerListInfo()
        {
            var CustomerAdressList = await _customerRepository.GetCustomerListDtlsAsync();

            var sortedCustomerAdressList = CustomerAdressList
                .GroupBy(alphabet => alphabet.CustomerName != null ? char.ToUpper(alphabet.CustomerName[0]) : '#')
                .OrderBy(g => g.Key)
                .Select(g => new CustomerGroupByAlphabet { GroupName = g.Key, CustomerAddressList = g.ToList() })
                .ToList();

            GroupedCustomerList = sortedCustomerAdressList;
            if (GroupedCustomerList.Count == 0)
            {
                InProgress = false;
                EmptyCustomers = ResourceLoader.GetForCurrentView("Resources").GetString("CustomersEmptyText");
            }
            else
            {
                EmptyCustomers = string.Empty;
            }
        }

        public void CustomersSyncIndicator(bool sync)
        {
            SyncProgress = Constants.CustomersSyncProgress;
        }

        private async Task StartSync()
        {
            try
            {
                InProgress = true;

                if (!Constants.SyncProgress)
                {
                    var asyncActionCommon = ThreadPool.RunAsync(
                        IAsyncAction => { _syncCoordinatorService.StartSync(); });

                    PageUtils.asyncActionCommon = asyncActionCommon;
                }

                SyncProgress = Constants.CustomersSyncProgress;
                var CustomerAdressList = await _customerRepository.GetCustomerListDtlsAsync();

                var sortedCustomerAdressList = CustomerAdressList
                    .GroupBy(alphabet => alphabet.CustomerName != null ? char.ToUpper(alphabet.CustomerName[0]) : '#')
                    .OrderBy(g => g.Key)
                    .Select(g => new CustomerGroupByAlphabet { GroupName = g.Key, CustomerAddressList = g.ToList() })
                    .ToList();

                GroupedCustomerList = sortedCustomerAdressList;
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }
}