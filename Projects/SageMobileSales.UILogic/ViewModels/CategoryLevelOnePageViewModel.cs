using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using SQLite;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class CategoryLevelOnePageViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IProductCategoryRepository _productCategoryRepository;
        //private ProductCategoryCollection _productCategoryCollection;
        private readonly ISyncCoordinatorService _syncCoordinatorService;
        private string _emptyCategories;
        private bool _inProgress;
        private string _log = string.Empty;
        private List<ProductCategory> _productCategoryList;
        private ISalesRepService _salesRepService;
        private bool _syncProgress;
        public DelegateCommand StartSyncCommand { get; private set; }

        public CategoryLevelOnePageViewModel(INavigationService navigationService, ISalesRepService salesRepService,
            ISyncCoordinatorService syncCoordinatorService, IProductCategoryRepository productCategoryRepository,
            IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _salesRepService = salesRepService;
            _syncCoordinatorService = syncCoordinatorService;
            _productCategoryRepository = productCategoryRepository;
            _eventAggregator = eventAggregator;
            ProductCategoryList = new List<ProductCategory>();
            StartSyncCommand = DelegateCommand.FromAsyncHandler(StartSync);
            _eventAggregator.GetEvent<ProductDataChangedEvent>()
                .Subscribe(UpdateProductCategoryList, ThreadOption.UIThread);
            _eventAggregator.GetEvent<ProductSyncChangedEvent>()
                .Subscribe(ProductsSyncIndicator, ThreadOption.UIThread);
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyCategories
        {
            get { return _emptyCategories; }
            private set { SetProperty(ref _emptyCategories, value); }
        }

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
        //public ProductCategoryCollection ProductCategoryCollection
        //{
        //    get { return _productCategoryCollection; }
        //    private set
        //    {
        //        SetProperty(ref _productCategoryCollection, value);
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

        /// <summary>
        ///     Loading product categories level one
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            //InProgress = true;
            try
            {
            //    if (!Constants.ProductsSyncProgress)
            //    {
            //        IAsyncAction asyncAction = ThreadPool.RunAsync(
            //            IAsyncAction =>
            //            {
            //                // Data Sync will Start.
            //                _syncCoordinatorService.StartProductsSync();
            //            });

            //        //    asyncAction.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
            //        //    {
            //        //        if (asyncStatus == AsyncStatus.Canceled)
            //        //            return;

            //        //Constants.ProductsSyncProgress = false;
            //        //    });
            //        PageUtils.asyncActionProducts = asyncAction;
            //    }

            //    SyncProgress = Constants.ProductsSyncProgress;
            //    ////ISupport Scroll incrementing
            //    //ProductCategoryCollection = new ProductCategoryCollection()
            //    //{
            //    //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null)
            //    //};

            //    //ProductCategoryCollection = new ProductCategoryCollection();
            //    //ProductCategoryCollection.ProductCategoryList = new List<ProductCategory>();
            //    //ProductCategoryCollection.ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);

            //    //if (ProductCategoryCollection.ProductCategoryList.Count > 0)
            //    //    InProgress = false;

            //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);
            //    //if (ProductCategoryList.Count > 0)
            //    //    InProgress = false;
                await StartSync();
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                _navigationService.Navigate("Signin", null);
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
            string arg = ((parameter as ItemClickEventArgs).ClickedItem as ProductCategory).CategoryId;
            bool moreLevels = await _productCategoryRepository.GetProductCategoryLevel(arg);

            if (moreLevels)
                _navigationService.Navigate("CategoryLevelTwo", (parameter as ItemClickEventArgs).ClickedItem);
            else
                _navigationService.Navigate("Items", arg);
        }

        public void CatalogButton_Click(object sender, object parameter)
        {
        }

        public void QuotesButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("Quotes", null);
        }

        public void OrdersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            // Change it to Orders
            _navigationService.Navigate("Orders", null);
        }

        public void CustomersButton_Click(object sender, object parameter)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("CustomersGroup", null);
        }

        public async void UpdateProductCategoryList(bool updated)
        {
            await UpdateProductCategoryListInfo();
        }

        public async Task UpdateProductCategoryListInfo()
        {
            ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);
            List<ProductCategory> _totalProductsList = await _productCategoryRepository.GetProductCategoryListAsync();
            if (ProductCategoryList.Count == 0 && _totalProductsList.Count == 0)
            {
                InProgress = false;
                EmptyCategories = ResourceLoader.GetForCurrentView("Resources").GetString("CategoriesEmptyText");
            }
            else
            {
                EmptyCategories = string.Empty;
            }
        }

        public void ProductsSyncIndicator(bool sync)
        {
            SyncProgress = Constants.ProductsSyncProgress;
        }

        //private async Task DisplayProductCategories()
        //{
        //    try
        //    {
        //        int TotalCount = 0;
        //        ProductCategoryCollection = new ProductCategoryCollection();

        //        ProductCategoryCollection.ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);
        //        TotalCount += ProductCategoryCollection.ProductCategoryList.Count;

        //        if (ProductCategoryCollection.ProductCategoryList.Count > 0)
        //        {
        //            OnPropertyChanged("ProductCategoryCollection");
        //        }
        //        if (ProductCategoryCollection.ProductCategoryList.Count == 0 )
        //        {
        //            await DisplayProductCategories();
        //        }
        //        if (ProductCategoryCollection.ProductCategoryList.Count < TotalCount || ProductCategoryCollection.ProductCategoryList.Count > TotalCount)
        //        {
        //            await DisplayProductCategories();
        //        }
        //        else
        //            return;

        //    }
        //    catch (Exception e)
        //    {
        //        throw (e);
        //    }
        //}

        private async Task StartSync()
        {
            InProgress = true;
            try
            {
                if (!Constants.ProductsSyncProgress)
                {
                    IAsyncAction asyncAction = ThreadPool.RunAsync(
                        IAsyncAction =>
                        {
                            // Data Sync will Start.
                            _syncCoordinatorService.StartProductsSync();
                        });

                    //    asyncAction.Completed = new AsyncActionCompletedHandler((IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                    //    {
                    //        if (asyncStatus == AsyncStatus.Canceled)
                    //            return;

                    //Constants.ProductsSyncProgress = false;
                    //    });
                    PageUtils.asyncActionProducts = asyncAction;
                }

                SyncProgress = Constants.ProductsSyncProgress;
                ////ISupport Scroll incrementing
                //ProductCategoryCollection = new ProductCategoryCollection()
                //{
                //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null)
                //};

                //ProductCategoryCollection = new ProductCategoryCollection();
                //ProductCategoryCollection.ProductCategoryList = new List<ProductCategory>();
                //ProductCategoryCollection.ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);

                //if (ProductCategoryCollection.ProductCategoryList.Count > 0)
                //    InProgress = false;

                ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(null);
                //if (ProductCategoryList.Count > 0)
                //    InProgress = false;

            }
            catch (SQLiteException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                _navigationService.Navigate("Signin", null);
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