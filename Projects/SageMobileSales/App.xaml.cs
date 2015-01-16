﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SageMobileSales.Views;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace SageMobileSales
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : MvvmAppBase
    {
        // Create the singleton container that will be used for type resolution in the app
        private readonly IUnityContainer _container = new UnityContainer();
        private string _log = String.Empty;
        private NetworkStatusChangedEventHandler networkStatusCallback;
        private bool registeredNetworkStatusNotify;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            //this.Suspending += OnSuspending;
        }

        public IEventAggregator EventAggregator { get; set; }

        protected override Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            var settingsLocal = ApplicationData.Current.LocalSettings;
            settingsLocal.CreateContainer("SageSalesContainer", ApplicationDataCreateDisposition.Always);
            var _isAuthorised = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsAuthorised];
            var _isLaunched = settingsLocal.Containers["SageSalesContainer"].Values[PageUtils.IsLaunched];

            // EventListener verboseListenerevent = new LogStorageFileEventListener("MyListenerVerbose");
            EventListener informationListener = new LogStorageFileEventListener("SageMobileSalesLog");

            //   verboseListenerevent.EnableEvents(AppEventSource.Log, EventLevel.Verbose);

            informationListener.EnableEvents(AppEventSource.Log, EventLevel.Informational);
            AppEventSource.Log.Info(ResourceLoader.GetForCurrentView().GetString("AppLaunchingInfo"));
            if (_isLaunched != null)
            {
                settingsLocal.Containers["SageSalesContainer"].Values.Remove(PageUtils.IsLaunched);
            }

            if (_isAuthorised == null)
            {
                NavigationService.Navigate("Signin", null);
            }
            else
            {
                NavigationService.Navigate("LoadingIndicator", null);
            }

            Window.Current.Activate();

            return Task.FromResult<object>(null);
        }

        protected override void OnRegisterKnownTypesForSerialization()
        {
            // Set up the list of known types for the SuspensionManager
            //SessionStateService.RegisterKnownType(typeof(Address));
        }

        protected override void OnInitialize(IActivatedEventArgs args)
        {
            EventAggregator = new EventAggregator();
            //var container = Container.Instance;
            _container.RegisterInstance(NavigationService);
            _container.RegisterInstance(SessionStateService);
            _container.RegisterInstance(EventAggregator);
            _container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            // Register Repositories
            _container.RegisterType<IDatabase, Database>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDataContext, DataContext>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISalesRepRepository, SalesRepRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILocalSyncDigestRepository, LocalSyncDigestRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductCategoryRepository, ProductCategoryRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductRepository, ProductRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductAssociatedBlobsRepository, ProductAssociatedBlobsRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ICustomerRepository, CustomerRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IContactRepository, ContactRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAddressRepository, AddressRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IQuoteRepository, QuoteRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IQuoteLineItemRepository, QuoteLineItemRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IOrderRepository, OrderRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IOrderLineItemRepository, OrderLineItemRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ITenantRepository, TenantRepository>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IFrequentlyPurchasedItemRepository, FrequentlyPurchasedItemRepository>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ISalesHistoryRepository, SalesHistoryRepository>(
                new ContainerControlledLifetimeManager());


            // Register Services
            _container.RegisterType<IOAuthService, OAuthService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISyncCoordinatorService, SyncCoordinatorService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IServiceAgent, ServiceAgent>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISalesRepService, SalesRepService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILocalSyncDigestService, LocalSyncDigestService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductCategoryService, ProductCategoryService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductService, ProductService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductAssociatedBlobService, ProductAssociatedBlobService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IProductDetailsService, ProductDetailsService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ICustomerService, CustomerService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IContactService, ContactService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IQuoteService, QuoteService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IQuoteLineItemService, QuoteLineItemService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<IOrderService, OrderService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IOrderLineItemService, OrderLineItemService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ITenantService, TenantService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IFrequentlyPurchasedItemService, FrequentlyPurchasedItemService>(
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ISalesHistoryService, SalesHistoryService>(
                new ContainerControlledLifetimeManager());

            //_container.RegisterType<IAppEventSource, AppEventSource>(new ContainerControlledLifetimeManager());

            ViewModelLocator.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
            {
                var viewModelTypeName = string.Format(CultureInfo.InvariantCulture,
                    "SageMobileSales.UILogic.ViewModels.{0}ViewModel, SageMobileSales.UILogic, Version=1.0.0.0, Culture=neutral",
                    viewType.Name);
                var viewModelType = Type.GetType(viewModelTypeName);
                return viewModelType;
            });
            //var resourceLoader = _container.Resolve<IResourceLoader>();                       
        }

        protected override object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        protected override IList<SettingsCommand> GetSettingsCommands()
        {
            var eventAggregator = _container.Resolve<IEventAggregator>();
            var settingsCommands = new List<SettingsCommand>();
            var resourceLoader = _container.Resolve<IResourceLoader>();

            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(), resourceLoader.GetString("Logout"),
                c => LogoutHandler()));
            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(), resourceLoader.GetString("HelpText"),
                async c => await Launcher.LaunchUriAsync(new Uri(resourceLoader.GetString("HelpURL")))));
            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(),
                resourceLoader.GetString("PrivacyPolicy"),
                async c => await Launcher.LaunchUriAsync(new Uri(resourceLoader.GetString("PrivacyPolicyURL")))));
            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(),
                resourceLoader.GetString("CopyrightInfo"),
                async c => await Launcher.LaunchUriAsync(new Uri(resourceLoader.GetString("CopyrightURL")))));
            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(),
                resourceLoader.GetString("FeedbackLinkText"),
                async c => await Launcher.LaunchUriAsync(new Uri(resourceLoader.GetString("FeedbackMailtoURL")))));
            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(), resourceLoader.GetString("About"),
                c => new AboutSettingsFlyout(eventAggregator).Show()));
#if(!PRODUCTION)

            settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(),
                resourceLoader.GetString("DeploymentSettings"),
                c => new ConfigurationSettingsFlyout(eventAggregator).Show()));

#endif


            return settingsCommands;
        }

        // Logout Settings Command Handler
        private async void LogoutHandler()
        {
            try
            {
                MessageDialog msgDialog;
                var oAuthService = _container.Resolve<IOAuthService>();
                var configSettings = ApplicationData.Current.LocalSettings;
                if (configSettings.Containers.ContainsKey("ConfigurationSettingsContainer"))
                {
                    if (configSettings.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"] != null)
                    {
                        configSettings.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"] = false;
                    }
                }
                await oAuthService.Cleanup();

                var frame = Window.Current.Content as Frame;
                if (frame.SourcePageType.Name == PageUtils.SignInPage ||
                    frame.SourcePageType.Name == PageUtils.LoadingIndicatorPage)
                {
                    msgDialog = new MessageDialog(
                        ResourceLoader.GetForCurrentView("Resources").GetString("UnableToLogoutText"),
                        ResourceLoader.GetForCurrentView("Resources").GetString("UnableToLogoutTitle"));
                    msgDialog.Commands.Add(new UICommand("Ok"));
                }
                else
                {
                    msgDialog = new MessageDialog(
                        ResourceLoader.GetForCurrentView("Resources").GetString("LogoutText"),
                        ResourceLoader.GetForCurrentView("Resources").GetString("LogoutTitle"));
                    msgDialog.Commands.Add(new UICommand("Ok",
                        UICommandInvokedHandler => { NavigationService.Navigate("Signin", null); }));
                    msgDialog.Commands.Add(new UICommand("Cancel"));
                }

                await msgDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private void NetworkStatusChange()
        {
            // register for network status change notifications
            try
            {
                networkStatusCallback = OnNetworkStatusChange;
                if (!registeredNetworkStatusNotify)
                {
                    NetworkInformation.NetworkStatusChanged += networkStatusCallback;
                    registeredNetworkStatusNotify = true;
                }
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser("Unexpected exception occured: " + ex.ToString(), NotifyType.ErrorMessage);
            }
        }

        private void OnNetworkStatusChange(object sender)
        {
            try
            {
                // get the ConnectionProfile that is currently used to connect to the Internet                
                var InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

                if (InternetConnectionProfile == null)
                {
                    Debug.WriteLine("if :  Not connected to internet");
                    //await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    rootPage.NotifyUser("Not connected to Internet\n", NotifyType.StatusMessage);
                    //});
                }
                else
                {
                    //var connectionProfileInfo = GetConnectionProfile(InternetConnectionProfile);
                    var connectionProfileInfo = InternetConnectionProfile.GetNetworkConnectivityLevel();
                    Debug.WriteLine("else :  connected to internet");
                    //await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    rootPage.NotifyUser(connectionProfileInfo, NotifyType.StatusMessage);
                    //});
                }
                //internetProfileInfo = "";
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser("Unexpected exception occured: " + ex.ToString(), NotifyType.ErrorMessage);
            }
        }

        //protected override Type GetPageType(string pageToken)
        //{
        //    var assemblyQualifiedAppType = this.GetType().GetTypeInfo().AssemblyQualifiedName;
        //    var pageNameWithParameter = assemblyQualifiedAppType.Replace(this.GetType().FullName, this.GetType().Namespace + ".Pages.{0}View");
        //    var viewFullName = string.Format(CultureInfo.InvariantCulture, pageNameWithParameter, pageToken);
        //    var viewType = Type.GetType(viewFullName);
        //    return viewType;
        //}


        /*
        protected override IList<SettingsCommand> GetSettingsCommand()
        {
            var settingsCommands = new List<SettingsCommand>();
            //settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(), "Text to show in Settings pane", ActionToBePerformed));
            //settingsCommands.Add(new SettingsCommand(Guid.NewGuid().ToString(), "Custom setting", () => new CustomSettingFlyout().Show()));
            return settingsCommands;
        }
        */


        // <summary>
        //Invoked when the application is launched normally by the end user.  Other entry points
        //will be used such as when the application is launched to open a specific file.
        //</summary>
        //<param name="e">Details about the launch request and process.</param>
        //        protected override void OnLaunched(LaunchActivatedEventArgs e)
        //        {

        //#if DEBUG
        //            if (System.Diagnostics.Debugger.IsAttached)
        //            {
        //                this.DebugSettings.EnableFrameRateCounter = true;
        //            }
        //#endif

        //            Frame rootFrame = Window.Current.Content as Frame;

        //             //Do not repeat app initialization when the Window already has content,
        //             //just ensure that the window is active
        //            if (rootFrame == null)
        //            {
        //                 //Create a Frame to act as the navigation context and navigate to the first page
        //                rootFrame = new Frame();
        //              //   Set the default language
        //                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

        //           //     rootFrame.NavigationFailed += OnNavigationFailed;

        //                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        //                {
        //                  //  TODO: Load state from previously suspended application
        //                }

        //             //    Place the frame in the current Window
        //                Window.Current.Content = rootFrame;
        //            }

        //            if (rootFrame.Content == null)
        //            {
        //               // AppEventSource obj=new AppEventSource();
        //              //   When the navigation stack isn't restored navigate to the first page,
        //              //   configuring the new page by passing required information as a navigation
        //              //   parameter
        //              //  rootFrame.Navigate(typeof(MainPage), e.Arguments);
        //                  EventListener verboseListenerevent = new LogStorageFileEventListener("MyListenerVerbose");
        //                  EventListener informationListener = new LogStorageFileEventListener("MyListenerInformation");

        //                  verboseListenerevent.EnableEvents(AppEventSource.Log, EventLevel.Verbose);
        //                  informationListener.EnableEvents(AppEventSource.Log, EventLevel.Informational);

        //                // When the navigation stack isn't restored navigate to the first page,
        //                // configuring the new page by passing required information as a navigation
        //                // parameter
        //                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
        //                {
        //                    throw new Exception("Failed to create initial page");
        //                }
        //            }

        //            AppEventSource.Log.Info("Current Window is activating");

        //         //    Ensure the current window is active
        //            Window.Current.Activate();
        //  }


        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}