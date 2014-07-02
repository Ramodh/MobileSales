using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SageMobileSales.UILogic.ViewModels
{
    class ConfigurationSettingsFlyoutViewModel : Microsoft.Practices.Prism.StoreApps.BindableBase, IFlyoutViewModel
    {
        private bool _isOpened;

        private readonly IUnityContainer _container = new UnityContainer();
        private string _selectedtype;
        private Action _closeFlyout;
        private INavigationService _navigationService;
        private readonly IOAuthService _oAuthService;
        public DelegateCommand<object> SelectionChangedCommand { get; set; }

        public DelegateCommand LogOutInCommand { get; private set; }
        private string _clientId;

        private string _scope;
        private string _url;
        //private  string _Server;
        private string _redirectUrl;
        private bool _isSageIdProduction;
        private List<string> _servers;
        private string _log;

        private string _previousSelectedType;

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


        public Action CloseFlyout
        {
            get { return _closeFlyout; }
            set { SetProperty(ref _closeFlyout, value); }
        }
        /// <summary>
        ///List for selecting method of creating a quote
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


        public ConfigurationSettingsFlyoutViewModel(INavigationService navigationService, IEventAggregator eventAggregator, IOAuthService oAuthService)
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
            Servers.Add("Local Machine");
            SelectedType = Constants.SelectedType;
        }

        private void Close()
        {
            IsOpened = false;
        }

        private void ServerSelectionchnaged(object args)
        {
            var configSettings = ApplicationData.Current.LocalSettings;
            configSettings.CreateContainer("ConfigurationSettingsContainer", ApplicationDataCreateDisposition.Always);

            var selected = ((ComboBox)args);

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
                ClientId = @"HGr7ysGn2MO1KfrB2f7V3UlJDYIyjhZ0";
                Url = "http://ashplatformsdc.cloudapp.net";
                Scope = @"6o6x4swc();";
                IsSageIdProduction = false;
                SelectedType = "Ash";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Local Machine")
            {
                ClientId = @"HGr7ysGn2MO1KfrB2f7V3UlJDYIyjhZ0";
                Url = "http://10.150.204.34:81";
                Scope = @"6o6x4swc();";
                IsSageIdProduction = false;
                SelectedType = "Local Machine";
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

            var configSettings = ApplicationData.Current.LocalSettings;
            configSettings.Containers["ConfigurationSettingsContainer"].Values["_previousSelectedType"] = _previousSelectedType;
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

