using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.UILogic.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.PubSubEvents;
using Sage.Authorisation.WinRT;
using SageMobileSales.ServiceAgents;
using SageMobileSales.DataAccess.Repositories;
using Windows.Data.Json;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.DataAccess;
using SageMobileSales.ServiceAgents.Services;
using Windows.UI.ApplicationSettings;
using Windows.ApplicationModel.Resources;
using SQLite;
using SageMobileSales.DataAccess.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    public class SigninPageViewModel : ViewModel
    {

        #region Fields

        public DelegateCommand SignInCommand { get; private set; }

        private INavigationService _navigationService;

        private readonly IOAuthService _oAuthService;

        private SQLiteAsyncConnection _sageSalesDB;
        private readonly IDatabase _database;    
    

        private string _accessToken;
        private bool _inProgress;
        private bool _isSignInDisabled;
        private string _log=string.Empty;
        
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }
        public bool isSignInDisabled
        {
            get { return _isSignInDisabled; }
            private set { SetProperty(ref _isSignInDisabled, value);
          
            }
        }


        #endregion

        public SigninPageViewModel(IOAuthService oAuthService, INavigationService navigationService, IDatabase database)
        {
            
            _oAuthService = oAuthService;
            _navigationService = navigationService;
            _database = database;
            SignInCommand = DelegateCommand.FromAsyncHandler(Authorise);
        }
        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
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
        /// Checks if the User is already logged in if so we will directly navigate to MainmenuPage else
        /// makes call to OAuthService to Authorize & to get AccessToken.
        /// Saving the returned AccessToken into ApplicationData so that we can use this in the enitre allication where ever it is necessary. 
        /// </summary>
        /// <returns></returns>
        private async Task Authorise()
        {

                 
            var settingsLocal = ApplicationData.Current.LocalSettings;
              var configSettings = ApplicationData.Current.LocalSettings;
            settingsLocal.CreateContainer("SageSalesContainer", ApplicationDataCreateDisposition.Always);
            var _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised];
          
            try
            {
                if (Constants.ConnectedToInternet())
                {
                    if (_isAuthorised == null)
                    {
                       InProgress = true;
                       isSignInDisabled = false ;
                        _accessToken = await _oAuthService.Authorize();
                          
                    }
                   
                 
                    if (!string.IsNullOrEmpty(_accessToken))
                    {
                        Constants.IsDbDeleted = false;
                        _navigationService.ClearHistory();
                        if (settingsLocal.Containers.ContainsKey("ConfigurationSettingsContainer"))
                        {
                            DataAccessUtils.IsServerChanged = Convert.ToBoolean(settingsLocal.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"]);

                            if (DataAccessUtils.IsServerChanged)
                            {
                                await _database.Delete();
                                await _database.Initialize();
                                _sageSalesDB = _database.GetAsyncConnection();
                            }
                        }
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
                    Constants.ShowMessageDialog(ResourceLoader.GetForCurrentView("Resources").GetString("NoInternetConnection"));
                }
            }
                catch(UnauthorizedAccessException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
                catch(SQLiteException ex)
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

    }
}
