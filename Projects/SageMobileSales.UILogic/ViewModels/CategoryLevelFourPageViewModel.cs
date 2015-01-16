using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CategoryLevelFourPageViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private string _catalogLevelFourPageTitle;
        private bool _inProgress;
        private string _log;
        private List<ProductCategory> _productCategoryList;
        private bool _syncProgress;
        private ProductCategory ProductCategory;

        public CategoryLevelFourPageViewModel(INavigationService navigationService,
            IProductCategoryRepository productCategoryRepository, IEventAggregator eventAggregator,
            ISyncCoordinatorService syncCoordinatorService)
        {
            _navigationService = navigationService;
            _syncCoordinatorService = syncCoordinatorService;
            _productCategoryRepository = productCategoryRepository;
            _eventAggregator = eventAggregator;
            StartSyncCommand = DelegateCommand.FromAsyncHandler(StartSync);
            ProductCategoryList = new List<ProductCategory>();
            _eventAggregator.GetEvent<ProductSyncChangedEvent>()
                .Subscribe(ProductsSyncIndicator, ThreadOption.UIThread);
        }

        public DelegateCommand StartSyncCommand { get; private set; }

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

        ///// <summary>
        ///// Collection to support incremental scrolling
        ///// </summary>
        //private ProductCategoryCollection _productCategoryCollection;
        //public ProductCategoryCollection ProductCategoryCollection
        //{
        //    get { return _productCategoryCollection; }
        //    private set
        //    {
        //        SetProperty(ref _productCategoryCollection, value);
        //        InProgress = false;
        //    }
        //}

        public List<ProductCategory> ProductCategoryList
        {
            get { return _productCategoryList; }
            private set
            {
                SetProperty(ref _productCategoryList, value);
                OnPropertyChanged("ProductCategoryList");

                if (_productCategoryList.Count > 0)
                    InProgress = false;
            }
        }

        public string CatalogLevelFourPageTitle
        {
            get { return _catalogLevelFourPageTitle; }
            private set { SetProperty(ref _catalogLevelFourPageTitle, value); }
        }

        /// <summary>
        ///     Loading product categories level four
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            InProgress = true;
            SyncProgress = Constants.ProductsSyncProgress;
            ProductCategory = navigationParameter as ProductCategory;
            CatalogLevelFourPageTitle = ProductCategory.CategoryName;

            try
            {
                //ProductCategoryCollection = new ProductCategoryCollection()
                //{
                //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(productCategory.CategoryId)
                //};
                ProductCategoryList =
                    await _productCategoryRepository.GetProductCategoryListDtlsAsync(ProductCategory.CategoryId);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewItemClick(object sender, object parameter)
        {
            try
            {
                var arg = ((parameter as ItemClickEventArgs).ClickedItem as ProductCategory).CategoryId;
                //bool moreLevels = await _productCategoryRepository.GetProductCategoryLevel(arg);

                //if (moreLevels)
                //    _navigationService.Navigate("CategoryLevelFour", arg);
                //else
                _navigationService.Navigate("Items", arg);
            }
            catch (NullReferenceException ex)
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
            _navigationService.ClearHistory();
            _navigationService.Navigate("CustomersGroup", null);
        }

        public void ProductsSyncIndicator(bool sync)
        {
            SyncProgress = Constants.ProductsSyncProgress;
        }

        private async Task StartSync()
        {
            try
            {
                InProgress = true;
                if (!Constants.ProductsSyncProgress)
                {
                    var asyncAction = ThreadPool.RunAsync(
                        IAsyncAction =>
                        {
                            // Data Sync will Start.
                            _syncCoordinatorService.StartProductsSync();
                        });

                    PageUtils.asyncActionProducts = asyncAction;
                }
                SyncProgress = Constants.ProductsSyncProgress;

                ProductCategoryList =
                    await _productCategoryRepository.GetProductCategoryListDtlsAsync(ProductCategory.CategoryId);
            }

            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }
}