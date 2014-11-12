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
            Servers.Add("Preview");
            Servers.Add("Production");
            Servers.Add("Ash");
            Servers.Add("Local Machine 1");
            Servers.Add("Local Machine 2");
            Servers.Add("Willow");
            Servers.Add("Elm");
            Servers.Add("Teak");
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

            var selected = ((ComboBox)args);

            if (selected.SelectedItem.ToString() == "Preview")
            {
                ClientId = @"RAHGBa7mZz5BZB5JmJqn42Ydl1IuHn5Q";
                Scope = @"bgtvnjmd();";
                Url = "https://PreviewMSales.sagedatacloud.com";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                IsSageIdProduction = false;
                SelectedType = "Preview";
                SetConfigurationValues();
            }

            if (selected.SelectedItem.ToString() == "Production")
            {
                ClientId = @"fRt7TgcUAzq9y0b3BLCTUM4Y0wwcWC51";
                Url = "https://sagemobilesales.na.sage.com/";
                Scope = @"kmt6bhzz();";
                RedirectUrl = "https://signon.sso.services.sage.com/oauth/native";
                IsSageIdProduction = true;
                SelectedType = "Production";
                SetConfigurationValues();
            }

            if (selected.SelectedItem.ToString() == "Ash")
            {
                //ClientId = @"xeQaIFwQvbDjKZvwzb92kFOLzDCd9CCc";
                ClientId = @"HObO8ZbQtT6yt6c8YRmDHBBu2GCQ7U6p";
                Url = "https://ashMSales.sagedatacloud.com/sdata/api/msales/1.0/";
                Scope = @"k1mcudfb();";
                IsSageIdProduction = false;
                SelectedType = "Ash";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }

            if (selected.SelectedItem.ToString() == "Local Machine 1")
            {
                ClientId = @"FhVDZU7p11pFRItTweZsh8XSGdEeFZ0g";
                Url = "http://172.29.59.122:8080/sdata/api/msales/1.0/";
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
            if (selected.SelectedItem.ToString() == "Willow")
            {
                ClientId = @"8iP7gBk2buVFtY1ymKNYkghS44f408sZ";
                Url = "https://willowmsales.sagedatacloud.com/sdata/api/msales/1.0/";
                Scope = @"ie1nah68();";
                IsSageIdProduction = false;
                SelectedType = "Willow";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Elm")
            {
                //ClientId = @"Wt8uPEZAcc7x2XqdnLX1VwcU49FNklhN";
                ClientId = @"98tyCStiJrMC2Wlu05waxy1HvFM3C2Qj";
                Url = "https://elmmsales.sagedatacloud.com/sdata/api/msales/1.0/";
                Scope = @"elcv5viz();";
                //Scope = @"gvb7lu14();";
                IsSageIdProduction = false;
                SelectedType = "Elm";
                RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";
                SetConfigurationValues();
            }
            if (selected.SelectedItem.ToString() == "Teak")
            {                
                ClientId = @"BmQhbGdK1YO5AQ0fsdndlR1GUlfP7EH3";
                Url = "https://TeakMSales.sagedatacloud.com/sdata/api/msales/1.0/";
                Scope = @"s99fetln();";                
                IsSageIdProduction = false;
                SelectedType = "Teak";
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