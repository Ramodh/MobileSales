using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using BindableBase = Microsoft.Practices.Prism.StoreApps.BindableBase;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class ConfigurationSettingsFlyoutViewModel : BindableBase, IFlyoutViewModel
    {
        private readonly IUnityContainer _container = new UnityContainer();
        private readonly INavigationService _navigationService;
        private readonly IOAuthService _oAuthService;
        private string _clientId;
        private Action _closeFlyout;
        private bool _isOpened;

        private bool _isSageIdProduction;
        private string _log;

        private string _previousSelectedType;
        private string _redirectUrl;
        private string _scope;
        private string _selectedtype;
        private List<string> _servers;
        private string _url;

        public ConfigurationSettingsFlyoutViewModel(INavigationService navigationService,
            IEventAggregator eventAggregator, IOAuthService oAuthService)
        {
            _oAuthService = oAuthService;
            _navigationService = navigationService;
            SelectionChangedCommand = new DelegateCommand<object>(ServerSelectionchnaged);
            LogOutInCommand = DelegateCommand.FromAsyncHandler(LogoutHandler);
            Servers = new List<string>();
            Servers.Add("Mobile Sales");
            Servers.Add("Master");
            Servers.Add("Release");
            Servers.Add("Staging");
            Servers.Add("Production");
            Servers.Add("SharedComponents");
            Servers.Add("CE Nephos QA");
            Servers.Add("Performance1");
            Servers.Add("Performance2");
            Servers.Add("End to End Testing");
            Servers.Add("Ash");
            Servers.Add("Local Machine 1");
            Servers.Add("Local Machine 2");

            SelectedType = Constants.SelectedType;
        }

        public DelegateCommand<object> SelectionChangedCommand { get; set; }

        public DelegateCommand LogOutInCommand { get; private set; }

        public string ClientId
        {
            get { return _clientId; }
            private set { SetProperty(ref _clientId, value); }
        }

        public string Scope
        {
            get { return _scope; }
            private set { SetProperty(ref _scope, value); }
        }

        public string Url
        {
            get { return _url; }
            private set { SetProperty(ref _url, value); }
        }

        public string RedirectUrl
        {
            get { return _redirectUrl; }
            private set { SetProperty(ref _redirectUrl, value); }
        }

        public bool IsSageIdProduction
        {
            get { return _isSageIdProduction; }
            private set { SetProperty(ref _isSageIdProduction, value); }
        }


        public List<string> Servers
        {
            get { return _servers; }
            private set { SetProperty(ref _servers, value); }
        }


        /// <summary>
        ///     List for selecting method of creating a quote
        /// </summary>
        public string SelectedType
        {
            get { return _selectedtype; }
            private set
            {
                SetProperty(ref _selectedtype, value);
                OnPropertyChanged("SelectedType");
            }
        }

        [RestorableState]
        public bool IsOpened
        {
            get { return _isOpened; }
            private set { SetProperty(ref _isOpened, value); }
        }

        public Action CloseFlyout
        {
            get { return _closeFlyout; }
            set { SetProperty(ref _closeFlyout, value); }
        }


        private void Close()
        {
            IsOpened = false;
        }

        private void ServerSelectionchnaged(object args)
        {
            ApplicationDataContainer configSettings = ApplicationData.Current.LocalSettings;
            configSettings.CreateContainer("ConfigurationSettingsContainer", ApplicationDataCreateDisposition.Always);

            var selected = ((ComboBox) args);

            if (selected.SelectedItem.ToString() == "Master")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Scope = @"gvb7lu14();";
                Url = "https://mobilesales-master.sagenephos.com/sdata/api/dynamic/-/";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "Master";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "Mobile Sales")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Scope = @"gvb7lu14();";
                Url = "https://mobilesales.sagenephos.com/sdata/api/dynamic/-/";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "Mobile Sales";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Release")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Url = "https://mobilesales-release.sagenephos.com/sdata/api/dynamic/-/";
                Scope = @"gvb7lu14();";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "Release";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "Staging")
            {
                ClientId = @"j2sak5nIN2YZ6szKaqx0hv66tLW0Mku0";
                Url = "https://mobilesales-staging.na.sage.com/sdata/api/dynamic/-/";
                Scope = @"kbyemk8m();";
                RedirectUrl = "https://signon.sso.services.sage.com/oauth/native";
                IsSageIdProduction = true;
                SelectedType = "Staging";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "Production")
            {
                ClientId = @"7XtkBANAnzQ9a1aRqspNlHR5UtSRUP0J";
                Url = "https://mobilesales.na.sage.com/sdata/api/dynamic/-/";
                Scope = @"hrixtfgl();";
                RedirectUrl = "https://signon.sso.services.sage.com/oauth/native";
                IsSageIdProduction = true;
                SelectedType = "Production";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "SharedComponents")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Url = "https://shared-mobilesales.sagenephos.com/sdata/api/dynamic/-/";
                Scope = @"gvb7lu14();";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "SharedComponents";
                SetConfigurationValues();
            }

            if (selected.SelectedItem.ToString() == "CE Nephos QA")
            {
                ClientId = @"8zEOHQQ8HdGzLzx6ZnlV64vJi3tUqnPr";
                Url = "https://mobilesales-ceqa.sagenephos.com/sdata/api/dynamic/-/";
                Scope = @"sm9euv5c();";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "CE Nephos QA";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "Performance1")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Url = "https://mobilesales-perf.sagenephos.com/sdata/api/dynamic/-/";
                Scope = @"gvb7lu14();";
                IsSageIdProduction = false;
                SelectedType = "Performance1";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "Performance2")
            {
                ClientId = @"TO3afnij1xMZrsH8akholwxvcJFlFc1N";
                Url = "https://mobilesales-perf-p2.sagenephos.com/sdata/api/dynamic/-/";
                Scope = @"gvb7lu14();";
                IsSageIdProduction = false;
                SelectedType = "Performance2";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }


            if (selected.SelectedItem.ToString() == "End to End Testing")
            {
                ClientId = @"CnBPqowEZXRMShWKMHnXoXTiylLRsDL7";
                Url = "https://mobilesales-e2e.na.sage.com/sdata/api/dynamic/-/";
                Scope = @"fuz0r3lt();";
                IsSageIdProduction = false;
                SelectedType = "End to End Testing";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Ash")
            {
                ClientId = @"xeQaIFwQvbDjKZvwzb92kFOLzDCd9CCc";
                Url = "http://ashmsalessdcweb.cloudapp.net/sdata/api/msales/1.0/";
                Scope = @"k1mcudfb();";
                IsSageIdProduction = false;
                SelectedType = "Ash";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }

            if (selected.SelectedItem.ToString() == "Local Machine 1")
            {
                ClientId = @"FhVDZU7p11pFRItTweZsh8XSGdEeFZ0g";
                Url = "http://172.25.19.12:8080/sdata/api/msales/1.0/";
                Scope = @"kipgf40h();";
                IsSageIdProduction = false;
                SelectedType = "Local Machine 1";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Local Machine 2")
            {
                ClientId = @"FhVDZU7p11pFRItTweZsh8XSGdEeFZ0g";
                Url = "http://172.25.19.4:8080/sdata/api/msales/1.0/";
                Scope = @"kipgf40h();";
                IsSageIdProduction = false;
                SelectedType = "Local Machine 2";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }

        }

        private async Task LogoutHandler()
        {
            try
            {
                if (PageUtils.asyncActionCommon != null)
                {
                    PageUtils.asyncActionCommon.Cancel();
                }
                if (PageUtils.asyncActionSalesRep != null)
                {
                    PageUtils.asyncActionSalesRep.Cancel();
                }
                if (PageUtils.asyncActionProducts != null)
                {
                    PageUtils.asyncActionProducts.Cancel();
                }
                if (PageUtils.asyncActionQuotes != null)
                {
                    PageUtils.asyncActionQuotes.Cancel();
                }
                if (PageUtils.asyncActionOrders != null)
                {
                    PageUtils.asyncActionOrders.Cancel();
                }
                await _oAuthService.Cleanup();

                // await _database.Delete();
                _navigationService.Navigate("Signin", null);
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (ArgumentException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                _navigationService.Navigate("Signin", null);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }


        private async void SetConfigurationValues()
        {
            _previousSelectedType = Constants.SelectedType;

            ApplicationDataContainer configSettings = ApplicationData.Current.LocalSettings;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["_previousSelectedType"] =
                _previousSelectedType;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["ClientId"] = ClientId;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["Scope"] = Scope;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["Url"] = Url;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["RedirectUrl"] = RedirectUrl;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["IsSageProduction"] = IsSageIdProduction;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["SelectedType"] = SelectedType;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"] = true;
            Constants.ClientId = ClientId;
            Constants.Url = Url;
            Constants.Scope = Scope;
            Constants.RedirectUrl = RedirectUrl;
            Constants.IsSageIdProduction = IsSageIdProduction;
            Constants.SelectedType = SelectedType;
            DataAccessUtils.SelectedServerType = SelectedType;
            OnPropertyChanged("SelectedType");
            await LogoutHandler();
        }
    }
}