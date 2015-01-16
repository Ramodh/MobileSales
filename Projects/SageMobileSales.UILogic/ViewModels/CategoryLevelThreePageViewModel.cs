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
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CategoryLevelThreePageViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private string _catalogLevelThreePageTitle;
        private bool _inProgress;
        private string _log;
        private List<ProductCategory> _productCategoryList;
        private bool _syncProgress;
        private ProductCategory ProductCategory;

        public CategoryLevelThreePageViewModel(INavigationService navigationService,
            IProductCategoryRepository productCategoryRepository, IEventAggregator eventAggregator,
            ISyncCoordinatorService syncCoordinatorService)
        {
            _navigationService = navigationService;
            _productCategoryRepository = productCategoryRepository;
            _syncCoordinatorService = syncCoordinatorService;
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

        public string CatalogLevelThreePageTitle
        {
            get { return _catalogLevelThreePageTitle; }
            private set { SetProperty(ref _catalogLevelThreePageTitle, value); }
        }

        /// <summary>
        ///     Loading product categories level three
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
            CatalogLevelThreePageTitle = ProductCategory.CategoryName;

            try
            {
                //ProductCategoryCollection = new ProductCategoryCollection()
                //{
                //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(productCategory.CategoryId)
                //};
                ProductCategoryList =
                    await _productCategoryRepository.GetProductCategoryListDtlsAsync(ProductCategory.CategoryId);
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
        public async void GridViewItemClick(object sender, object parameter)
        {
            var arg = ((parameter as ItemClickEventArgs).ClickedItem as ProductCategory).CategoryId;
            var moreLevels = await _productCategoryRepository.GetProductCategoryLevel(arg);

            if (moreLevels)
                _navigationService.Navigate("CategoryLevelFour", (parameter as ItemClickEventArgs).ClickedItem);
            else
                _navigationService.Navigate("Items", arg);
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
    }
}