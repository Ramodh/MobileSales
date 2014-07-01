using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.UILogic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace SageMobileSales.UILogic.ViewModels
{
    class CategoryLevelFourPageViewModel : ViewModel
    {
        private INavigationService _navigationService;
        private IProductCategoryRepository _productCategoryRepository;
        private readonly IEventAggregator _eventAggregator;
        private bool _inProgress;
        private string _log;

        /// <summary>
        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        private bool _syncProgress;
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

        private List<ProductCategory> _productCategoryList;
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

        private string _catalogLevelFourPageTitle;
        public string CatalogLevelFourPageTitle
        {
            get { return _catalogLevelFourPageTitle; }
            private set { SetProperty(ref _catalogLevelFourPageTitle, value); }
        }

        public CategoryLevelFourPageViewModel(INavigationService navigationService, IProductCategoryRepository productCategoryRepository, IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _productCategoryRepository = productCategoryRepository;
            _eventAggregator = eventAggregator;
            ProductCategoryList = new List<ProductCategory>();
            _eventAggregator.GetEvent<ProductSyncChangedEvent>()
                .Subscribe(ProductsSyncIndicator, ThreadOption.UIThread);
        }

        /// <summary>
        /// Loading product categories level four
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            InProgress = true;
            SyncProgress = Constants.ProductsSyncProgress;
            ProductCategory productCategory = navigationParameter as ProductCategory;
            CatalogLevelFourPageTitle = productCategory.CategoryName;

            try
            {
                //ProductCategoryCollection = new ProductCategoryCollection()
                //{
                //    ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(productCategory.CategoryId)
                //};
                ProductCategoryList = await _productCategoryRepository.GetProductCategoryListDtlsAsync(productCategory.CategoryId);
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
        /// Grid View Item Click 
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
    }
}
