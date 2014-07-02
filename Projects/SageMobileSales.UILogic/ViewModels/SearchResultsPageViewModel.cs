using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;
using SageMobileSales.UILogic.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SageMobileSales.UILogic.ViewModels
{
    class SearchResultsPageViewModel : ViewModel
    {
          private INavigationService _navigationService;
          private IProductRepository _productRepository;        
          private string _searchTerm;
          private string _queryString;
          private bool _noSearchResults;
          private List<ProductDetails> _searchResults;
          private int _totalCount;       
          private List<ProductDetails> _productsList;
          private bool _inProgress;
          private string _log=string.Empty;

          /// <summary>
          /// Holds previous Search term
          /// </summary>
        public List<ProductDetails> ProductsList
        {
            get { return _productsList; }
            private set { SetProperty(ref _productsList, value); }
        }

        public SearchResultsPageViewModel(INavigationService navigationService, IProductRepository productRepository)
        {
            _navigationService = navigationService;
            _productRepository = productRepository;         
        }
  

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }

        /// <summary>
        ///GridView on itemclick navigate to ItemDetail Page
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
        /// Holds previous Search term
        /// </summary>
        [RestorableState]
        public static string PreviousSearchTerm { get; private set; }

        /// <summary>
        /// Holds previous Search Results
        /// </summary>
        [RestorableState]
        public static List<ProductDetails> PreviousSearchResults { get; private set; }

        /// <summary>
        /// Holds Searched term 
        /// </summary>
        public string QueryText
        {
            get { return _queryString; }
            private set { SetProperty(ref this._queryString, value); }
        }
        /// <summary>
        /// Searched term 
        /// </summary>
        public string SearchTerm
        {
            get { return _searchTerm; }
            private set { SetProperty(ref this._searchTerm, value); }
        }
        /// <summary>
        /// Load Search Results
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
        /// Check Total Count of Search Results
        /// </summary>
        [RestorableState]
        public int TotalCount
        {
            get { return _totalCount; }
            private set { SetProperty(ref _totalCount, value); }
        }

        /// <summary>
        /// Check Search Results if any
        /// </summary>
        public bool NoSearchResults
        {
            get { return _noSearchResults; }
            private set { SetProperty(ref _noSearchResults, value); }
        }

        /// <summary>
        /// Data loading indicator
        /// </summary>
        public bool InProgress
        {
            get { return _inProgress; }
            private set { SetProperty(ref _inProgress, value); }
        }

        /// <summary>
        ///Loading search results
        /// </summary>
        /// <param name="navigationParameter"></param>
        /// <param name="navigationMode"></param>
        /// /// <param name="viewModelState"></param>
        public async override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            InProgress = true;
           
            var queryText = navigationParameter as String;          
         //   string errorMessage = string.Empty;
            this.SearchTerm = queryText;
            this.QueryText = '\u201c' + queryText + '\u201d';     
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
                this.SearchResults = new List<ProductDetails>(productDetails);
                this.NoSearchResults = !this.SearchResults.Any();             
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
