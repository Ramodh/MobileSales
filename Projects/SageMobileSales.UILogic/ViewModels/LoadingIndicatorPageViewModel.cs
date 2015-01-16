using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class LoadingIndicatorPageViewModel : ViewModel
    {
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantService _tenantService;
        private bool _inProgress;
        private string _log = string.Empty;

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

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                InProgress = true;

                //Loading all global variables
                var settingsLocal = ApplicationData.Current.LocalSettings;
                var _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values["IsAuthorised"];
                var _isLaunched = settingsLocal.Containers["SageSalesContainer"].Values["IsLaunched"];

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
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        /// <summary>
        ///     Sync user data/tenant info
        /// </summary>
        /// <returns></returns>
        public async Task SyncUserData()
        {
            try
            {
                // Sync SalesRep(Loggedin User) data
                await _salesRepService.SyncSalesRep();

                Constants.TenantId = await _tenantRepository.GetTenantId();

                //Company Settings/SalesTeamMember
                var salesPersonChanged = await _tenantService.SyncTenant();

                //Delete localSyncDigest for Customer and set all customers isActive to false
                if (salesPersonChanged)
                    await _localSyncDigestRepository.DeleteLocalSyncDigestForCustomer();

                InProgress = false;
                _navigationService.ClearHistory();
                _navigationService.Navigate("CustomersGroup", null);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }
}