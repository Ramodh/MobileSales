using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.ServiceAgents.Common;
using SageMobileSales.UILogic.Helpers;
using Windows.UI.Xaml;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class ItemsPageViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IProductRepository _productRepository;
        private bool _emptyFilteredProductList;
        private string _emptyProducts;
        private bool _emptyText;
        private ProductCollection _filteredProductList;
        private bool _inProgress;
        private string _log = string.Empty;
        private ProductCollection _productCollection;
        private ProductCollection _productsList;
        private bool _syncProgress;
        private Visibility _isTextChanged;

        public ItemsPageViewModel(INavigationService navigationService, IProductRepository productRepository,
            IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _productRepository = productRepository;
            _eventAggregator = eventAggregator;
            TextChangedCommand = new DelegateCommand<object>(TextBoxTextChanged);
            _eventAggregator.GetEvent<ProductSyncChangedEvent>()
                .Subscribe(ProductsSyncIndicator, ThreadOption.UIThread);
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public string EmptyProducts
        {
            get { return _emptyProducts; }
            private set { SetProperty(ref _emptyProducts, value); }
        }

        /// <summary>
        ///     Display empty results text
        /// </summary>
        public bool EmptyText
        {
            get { return _emptyText; }
            private set { SetProperty(ref _emptyText, value); }
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
        /// Visibility property for Filter icon in Filetritems textbox
        /// </summary>
        public Visibility IsTextChanged
        {
            get { return _isTextChanged; }
            private set { SetProperty(ref _isTextChanged, value); }
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


        public bool EmptyFilteredProductList
        {
            get { return _emptyFilteredProductList; }
            private set { SetProperty(ref _emptyFilteredProductList, value); }
        }

        public DelegateCommand<object> TextChangedCommand { get; set; }

        /// <summary>
        ///     Collection to support incremental scrolling
        /// </summary>
        public ProductCollection ProductCollection
        {
            get { return _productCollection; }
            private set
            {
                SetProperty(ref _productCollection, value);
                InProgress = false;
            }
        }

        /// <summary>
        ///     Collection to Filter items based on search text
        /// </summary>
        public ProductCollection FilteredProductList
        {
            get { return _filteredProductList; }
            private set { SetProperty(ref _filteredProductList, value); }
        }

        /// <summary>
        ///     Collection to hold products
        /// </summary>
        public ProductCollection ProductsList
        {
            get { return _productsList; }
            private set { SetProperty(ref _productsList, value); }
        }

        /// <summary>
        ///     Loading products
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            EmptyFilteredProductList = false;
            EmptyText = true;
            var categoryId = navigationParameter as string;
            InProgress = true;
            SyncProgress = Constants.ProductsSyncProgress;
            ProductCollection = new ProductCollection
            {
                ProductList = await _productRepository.GetCategoryProductsAsync(categoryId)
            };

            if (!(ProductCollection.ProductList.Count > 0))
            {
                Debug.WriteLine("Items Not Present");
                EmptyProducts = ResourceLoader.GetForCurrentView("Resources").GetString("EmptyProductsText");
            }
            ProductsList = ProductCollection;
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
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
                var arg = ((parameter as ItemClickEventArgs).ClickedItem as ProductDetails);

                _navigationService.Navigate("ItemDetail", arg.ProductId);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            //var arg = ((parameter as ItemClickEventArgs).ClickedItem as ProductCategory).CategoryId;
            //bool moreLevels = await _productCategoryRepository.GetProductCategoryLevel(arg);

            //if (moreLevels)
            //    _navigationService.Navigate("CategoryLevelTwo", (parameter as ItemClickEventArgs).ClickedItem);
            //else
            //    _navigationService.Navigate("Items", arg);
        }

        /// <summary>
        ///     Filter items in within the List
        /// </summary>
        /// <param name="searchText"></param>
        public ProductCollection FilterSearchItems(string searchText)
        {
            try
            {
                FilteredProductList = new ProductCollection();
                FilteredProductList.ProductList = new List<ProductDetails>();
                if (!InProgress)
                {
                    if (ProductsList.ProductList != null)
                    {
                        foreach (ProductDetails item in ProductsList.ProductList)
                        {
                            if (item.ProductName != null)
                            {
                                if (item.ProductName.ToLower().Contains(searchText.ToLower()))

                                    FilteredProductList.ProductList.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            return FilteredProductList;
        }

        /// <summary>
        ///     TextChanged event to filter items
        /// </summary>
        /// <param name="searchText"></param>
        private void TextBoxTextChanged(object args)
       {
            EmptyText = false;
            IsTextChanged = Visibility.Collapsed;
            if (!InProgress)
            {
                if (((TextBox) args).Text != null&&((TextBox) args).Text!=string.Empty)
                {
                    string searchTerm = ((TextBox) args).Text.Trim();                  
                    EmptyText = !searchTerm.Any();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        ProductCollection = FilterSearchItems(searchTerm);
                    }
                    else
                    {
                        ProductCollection = ProductsList;
                    }
                    if (ProductsList.ProductList.Count > 0)

                        EmptyFilteredProductList = !ProductCollection.ProductList.Any();
                }
                else
                {
                    ProductCollection = ProductsList;
                    IsTextChanged = Visibility.Visible;

                }
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