using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SageMobileSales.UILogic.Model;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CustomersGroupPageViewModel : ViewModel
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        //private CustomerCollection _customerCollection;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ITenantService _tenantService;
        private string _emptyCustomers;
        private List<CustomerGroupByAlphabet> _groupedCustomerList;
        private bool _inProgress;
        private string _log = string.Empty;
        private bool _syncProgress;

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
            _eventAggregator.GetEvent<CustomerDataChangedEvent>().Subscribe(UpdateCustomerList, ThreadOption.UIThread);
            _eventAggregator.GetEvent<CustomerSyncChangedEvent>()
                .Subscribe(CustomersSyncIndicator, ThreadOption.UIThread);
        }

        /// <summary>
        ///     Collection to support incremental scrolling
        /// </summary>
        //public CustomerCollection CustomerCollection
        //{
        //    get { return _customerCollection; }
        //    private set
        //    {
        //        SetProperty(ref _customerCollection, value);
        //        InProgress = false;
        //    }
        //}

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
        ///     Data  syncing indicator
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
                InProgress = true;

                ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
                object _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised];
                object _isLaunched = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsLaunched];

                PageUtils.GetConfigurationSettings();

                if (_isLaunched == null)
                {
                    //Change by Ramodh - Confirm if works fine and also to add it in seperate Thread
                    await SyncUserData();

                    if (_isAuthorised == null)
                    {
                        // Adding ISAuthorised variable to Appliaction Data.
                        // So that we can use this for the next logins whether user already Authorised or not.
                        settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised] = true;

                        //IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                        //                           (IAsyncAction) =>
                        //                           {

                        //// Sync SalesRep(Loggedin User) data
                        //await _salesRepService.SyncSalesRep();

                        ////Constants.TenantId = "F200AC19-1BE6-48C5-B604-2D322020F48E";
                        //Constants.TenantId = await _tenantRepository.GetTenantId();
                        ////Company Settings/SalesTeamMember
                        //await _tenantService.SyncTenant();

                        //});
                        //PageUtils.asyncActionSalesRep = asyncAction;

                        //asyncAction.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                        //{
                        //    if (asyncStatus == AsyncStatus.Canceled)
                        //        return;
                        //});
                    }
                    else
                    {
                        PageUtils.GetApplicationData();
                    }

                    //if (settingsLocal.Containers["SageSalesContainer"].Values.ContainsKey(PageUtils.IsLaunched))
                    //{
                    settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsLaunched] = true;
                    //}
                }

                //if (string.IsNullOrEmpty(Constants.TenantId))
                //{
                //    //Constants.TenantId = "F200AC19-1BE6-48C5-B604-2D322020F48E";
                //    Constants.TenantId = await _tenantRepository.GetTenantId();
                //    string test = await _tenantRepository.GetTenantId();
                //}

                if (!Constants.SyncProgress)
                {
                    IAsyncAction asyncActionCommon = ThreadPool.RunAsync(
                        IAsyncAction => { _syncCoordinatorService.StartSync(); });

                    //asyncActionCommon.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                    //{
                    //    if (asyncStatus == AsyncStatus.Canceled)
                    //        return;

                    //    Constants.SyncProgress = false;
                    //});
                    PageUtils.asyncActionCommon = asyncActionCommon;
                }

                SyncProgress = Constants.CustomersSyncProgress;
                List<CustomerDetails> CustomerAdressList = await _customerRepository.GetCustomerListDtlsAsync();

                //var queryFirstLetters = from item in CustomerAdressList
                //                        orderby ((CustomerAddress)item).CustomerName
                //                        group item by item.CustomerName[0];                   

                //var query = from item in CustomerAdressList
                //            orderby ((CustomerAddress)item).CustomerName
                //            group item by new { ((CustomerAddress)item).CustomerName } into g
                //            select new { GroupName = g.Key.CustomerName[0], Items = g };

                List<CustomerGroupByAlphabet> sortedCustomerAdressList = CustomerAdressList
                    .GroupBy(alphabet => alphabet.CustomerName[0])
                    .OrderBy(g => g.Key)
                    .Select(g => new CustomerGroupByAlphabet { GroupName = g.Key, CustomerAddressList = g.ToList() })
                    .ToList();

                GroupedCustomerList = sortedCustomerAdressList;

                //CustomerCollection = new CustomerCollection()
                //{
                //    CustomerAdressList = sortedCustomerAdressList
                //};

                //if (!(CustomerCollection.CustomerAdressList.Count > 0))
                //{
                //    EmptyCustomers = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyProductsText");
                //}            

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
            List<CustomerDetails> CustomerAdressList = await _customerRepository.GetCustomerListDtlsAsync();

            List<CustomerGroupByAlphabet> sortedCustomerAdressList = CustomerAdressList
                .GroupBy(alphabet => alphabet.CustomerName[0])
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

        public async Task SyncUserData()
        {
            // Sync SalesRep(Loggedin User) data
            await _salesRepService.SyncSalesRep();

            Constants.TenantId = await _tenantRepository.GetTenantId();

            //Company Settings/SalesTeamMember
            bool salesPersonChanged = await _tenantService.SyncTenant();

            //Delete localSyncDigest for Customer and set all customers isActive to false
            if (salesPersonChanged)
                await _localSyncDigestRepository.DeleteLocalSyncDigestForCustomer();
        }
    }
}