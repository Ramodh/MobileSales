using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SQLite;
using SageMobileSales.DataAccess.Repositories;

namespace SageMobileSales.UILogic.ViewModels
{
    public class SigninPageViewModel : ViewModel
    {
        #region Fields

        private readonly IDatabase _database;
        private readonly INavigationService _navigationService;
        private readonly ISalesRepService _salesRepService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ILocalSyncDigestRepository _localSyncDigestRepository;
        private readonly ITenantService _tenantService;
        private readonly IOAuthService _oAuthService;


        private string _accessToken;
        private bool _inProgress;
        private bool _isSignInDisabled;
        private string _log = string.Empty;
        private SQLiteAsyncConnection _sageSalesDB;
        public DelegateCommand SignInCommand { get; private set; }

        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public bool isSignInDisabled
        {
            get { return _isSignInDisabled; }
            private set { SetProperty(ref _isSignInDisabled, value); }
        }

        #endregion

        public SigninPageViewModel(IOAuthService oAuthService, INavigationService navigationService, IDatabase database,
            ISalesRepService salesRepService,
            ITenantRepository tenantRepository, ITenantService tenantService,
            ILocalSyncDigestRepository localSyncDigestRepository)
        {
            _oAuthService = oAuthService;
            _navigationService = navigationService;
            _database = database;
            _salesRepService = salesRepService;
            _tenantService = tenantService;
            _tenantRepository = tenantRepository;
            _localSyncDigestRepository = localSyncDigestRepository;
            SignInCommand = DelegateCommand.FromAsyncHandler(Authorise);
        }

        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            try
            {
                PageUtils.GetConfigurationSettings();
                isSignInDisabled = true;
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #region Command Implementations

        /// <summary>
        ///     Checks if the User is already logged in if so we will directly navigate to MainmenuPage else
        ///     makes call to OAuthService to Authorize & to get AccessToken.
        ///     Saving the returned AccessToken into ApplicationData so that we can use this in the enitre allication where ever it
        ///     is necessary.
        /// </summary>
        /// <returns></returns>
        private async Task Authorise()
        {
            ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer configSettings = ApplicationData.Current.LocalSettings;
            settingsLocal.CreateContainer("SageSalesContainer", ApplicationDataCreateDisposition.Always);
            object _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised];

            try
            {
                if (Constants.ConnectedToInternet())
                {
                    if (_isAuthorised == null)
                    {
                        InProgress = true;
                        isSignInDisabled = false;
                        _accessToken = await _oAuthService.Authorize();
                    }

                    if (!string.IsNullOrEmpty(_accessToken))
                    {
                        settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised] = true;
                        Constants.IsDbDeleted = false;
                        _navigationService.ClearHistory();
                        if (settingsLocal.Containers.ContainsKey("ConfigurationSettingsContainer"))
                        {
                            DataAccessUtils.IsServerChanged =
                                Convert.ToBoolean(
                                    settingsLocal.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"]);

                            if (DataAccessUtils.IsServerChanged)
                            {
                                await _database.Delete();
                                await _database.Initialize();
                                _sageSalesDB = _database.GetAsyncConnection();
                            }
                        }
                        //Sync current user/tenant info
                        await SyncUserData();

                        InProgress = false;
                        isSignInDisabled = true;
                        _navigationService.Navigate("CustomersGroup", null);
                    }
                    else
                    {
                        settingsLocal = ApplicationData.Current.LocalSettings;
                        settingsLocal.DeleteContainer("SageSalesContainer");
                        _navigationService.Navigate("Signin", null);
                        InProgress = false;
                        isSignInDisabled = true;
                    }
                }
                else
                {
                    Constants.ShowMessageDialog(
                        ResourceLoader.GetForCurrentView("Resources").GetString("NoInternetConnection"));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        #endregion

        #region private methods
        private async Task SyncUserData()
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
        #endregion
    }
}