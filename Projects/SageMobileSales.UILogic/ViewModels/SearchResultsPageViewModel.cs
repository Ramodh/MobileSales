using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class SearchResultsPageViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IProductRepository _productRepository;
        private bool _inProgress;
        private string _log = string.Empty;
        private bool _noSearchResults;
        private List<ProductDetails> _productsList;
        private string _queryString;
        private List<ProductDetails> _searchResults;
        private string _searchTerm;
        private int _totalCount;

        public SearchResultsPageViewModel(INavigationService navigationService, IProductRepository productRepository)
        {
            _navigationService = navigationService;
            _productRepository = productRepository;
        }

        /// <summary>
        ///     Holds previous Search term
        /// </summary>
        public List<ProductDetails> ProductsList
        {
            get { return _productsList; }
            private set { SetProperty(ref _productsList, value); }
        }

        /// <summary>
        ///     Holds previous Search term
        /// </summary>
        [RestorableState]
        public static string PreviousSearchTerm { get; private set; }

        /// <summary>
        ///     Holds previous Search Results
        /// </summary>
        [RestorableState]
        public static List<ProductDetails> PreviousSearchResults { get; private set; }

        /// <summary>
        ///     Holds Searched term
        /// </summary>
        public string QueryText
        {
            get { return _queryString; }
            private set { SetProperty(ref _queryString, value); }
        }

        /// <summary>
        ///     Searched term
        /// </summary>
        public string SearchTerm
        {
            get { return _searchTerm; }
            private set { SetProperty(ref _searchTerm, value); }
        }

        /// <summary>
        ///     Load Search Results
        /// </summary>
        public List<ProductDetails> SearchResults
        {
            get { return _searchResults; }
            private set
            {
                SetProperty(ref _searchResults, value);
                InProgress = false;
            }
        }

        /// <summary>
        ///     Check Total Count of Search Results
        /// </summary>
        [RestorableState]
        public int TotalCount
        {
            get { return _totalCount; }
            private set { SetProperty(ref _totalCount, value); }
        }

        /// <summary>
        ///     Check Search Results if any
        /// </summary>
        public bool NoSearchResults
        {
            get { return _noSearchResults; }
            private set { SetProperty(ref _noSearchResults, value); }
        }

        /// <summary>
        ///     Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        /// <summary>
        ///     GridView on itemclick navigate to ItemDetail Page
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
        }

        /// <summary>
        ///     Loading search results
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// ///
        /// <param name="viewModelState"></param>
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode,
            Dictionary<string, object> viewModelState)
        {
            InProgress = true;

            var queryText = navigationParameter as String;
            //   string errorMessage = string.Empty;
            SearchTerm = queryText;
            QueryText = '\u201c' + queryText + '\u201d';
            try
            {
                if (queryText == PreviousSearchTerm)
                {
                    ProductsList = PreviousSearchResults;
                    TotalCount = ProductsList.Count();
                }
                else
                {
                    if (queryText == "'")
                    {
                        queryText = queryText.Trim().Replace("'", "''");
                    }
                    else
                    {
                        queryText = queryText.Trim().Replace("'", string.Empty);
                    }
                    var searchResults = await _productRepository.GetSearchSuggestionsAsync(queryText);
                    ProductsList = searchResults;
                    PreviousSearchResults = ProductsList;
                    TotalCount = ProductsList.Count();
                }


                var productDetails = new List<ProductDetails>();
                foreach (var product in ProductsList)
                {
                    productDetails.Add(product);
                }
                SearchResults = new List<ProductDetails>(productDetails);
                NoSearchResults = !SearchResults.Any();
                PreviousSearchTerm = SearchTerm;
                base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }
    }
}