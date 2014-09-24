using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class LoadingIndicatorPageViewModel : ViewModel
    {

        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ITenantService _tenantService;
        private bool _inProgress;

        public LoadingIndicatorPageViewModel(INavigationService navigationService, ISalesRepService salesRepService,
            ITenantRepository tenantRepository, ITenantService tenantService,
            ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _navigationService = navigationService;
            _salesRepService = salesRepService;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            _localSyncDigestRepository = localSyncDigestRepository;
        }


        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public async override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            InProgress = true;

            //Loading all global variables
            ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
            object _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values["IsAuthorised"];
            object _isLaunched = settingsLocal.Containers["SageSalesContainer"].Values["IsLaunched"];

            PageUtils.GetConfigurationSettings();
            PageUtils.GetApplicationData();

            if (_isLaunched == null)
            {
                if (_isAuthorised == null)
                {
                    // Adding ISAuthorised variable to Appliaction Data.
                    // So that we can use this for the next logins whether user already Authorised or not.
                    settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised] = true;
                }
                settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsLaunched] = true;
            }

            await SyncUserData();

            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        /// <summary>
        /// Sync user data/tenant info
        /// </summary>
        /// <returns></returns>
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

            InProgress = false;
            _navigationService.ClearHistory();
            _navigationService.Navigate("CustomersGroup", null);
        }
    }
}
