using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.DataAccess.Repositories;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class SearchUserControlViewModel : ViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IProductRepository _productRepository;
        private string _imageUrl = string.Empty;
        private string _log = string.Empty;
        private bool _noSuggestions;

        public SearchUserControlViewModel(INavigationService navigationService, IProductRepository productRepository,
            IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
            _productRepository = productRepository;
            _eventAggregator = eventAggregator;
            SearchCommand = new DelegateCommand<SearchBoxQuerySubmittedEventArgs>(SearchBoxQuerySubmitted);
            SearchSuggestionsCommand =
                new DelegateCommand<SearchBoxSuggestionsRequestedEventArgs>(
                    async eventArgs => { await SearchBoxSuggestionsRequested(eventArgs); });
            ResultSuggestionChosenCommand =
                new DelegateCommand<SearchBoxResultSuggestionChosenEventArgs>(OnResultSuggestionChosen);
        }

        public DelegateCommand<SearchBoxQuerySubmittedEventArgs> SearchCommand { get; set; }
        public DelegateCommand<SearchBoxSuggestionsRequestedEventArgs> SearchSuggestionsCommand { get; set; }
        public DelegateCommand<SearchBoxResultSuggestionChosenEventArgs> ResultSuggestionChosenCommand { get; set; }

        /// <summary>
        ///     Image Url
        /// </summary>
        public string ImageUrl
        {
            get { return _imageUrl; }
            private set { SetProperty(ref _imageUrl, value); }
        }

        /// <summary>
        ///     No Suggestions
        /// </summary>
        public bool NoSuggestions
        {
            get { return _noSuggestions; }
            private set { SetProperty(ref _noSuggestions, value); }
        }

        /// <summary>
        ///     Navigate to search results page on searchquery submitted
        /// </summary>
        /// <param name="eventArgs"></param>
        private void SearchBoxQuerySubmitted(SearchBoxQuerySubmittedEventArgs eventArgs)
        {
            string searchTerm = eventArgs.QueryText != null ? eventArgs.QueryText.Trim() : null;
            if (!string.IsNullOrEmpty(searchTerm) &&
                searchTerm != ResourceLoader.GetForCurrentView("Resources").GetString("NoSuggestions"))
                NoSuggestions = false;
            if (!string.IsNullOrEmpty(searchTerm) && !NoSuggestions)
            {
                _navigationService.Navigate("SearchResults", searchTerm);
            }
        }

        /// <summary>
        ///     Navigate to ItemDetail page on item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public void GridViewItemClick(object sender, object parameter)
        {
            var arg = ((parameter as ItemClickEventArgs).ClickedItem as Product);
            _navigationService.Navigate("ItemDetail", arg);
        }

        /// <summary>
        ///     Navigate to ItemDetail page on choosing result suggestion
        /// </summary>
        /// <param name="args"></param>
        private void OnResultSuggestionChosen(SearchBoxResultSuggestionChosenEventArgs eventArgs)
        {
            string productId = eventArgs.Tag != null ? eventArgs.Tag.Trim() : null;
            if (!string.IsNullOrEmpty(productId))
            {
                _navigationService.Navigate("ItemDetail", productId);
            }
        }

        /// <summary>
        ///     Display result suggestions in searchbox
        /// </summary>
        /// <param name="args"></param>
        private async Task SearchBoxSuggestionsRequested(SearchBoxSuggestionsRequestedEventArgs eventArgs)
        {
            string queryText = eventArgs.QueryText != null ? eventArgs.QueryText.Trim() : null;
            if (string.IsNullOrEmpty(queryText)) return;
            SearchSuggestionsRequestDeferral deferral = eventArgs.Request.GetDeferral();

            try
            {
                SearchSuggestionCollection suggestionCollection = eventArgs.Request.SearchSuggestionCollection;
                if (queryText == "'")
                {
                    queryText = queryText.Trim().Replace("'", "''");
                }
                else
                {
                    queryText = queryText.Trim().Replace("'", string.Empty);
                }
                List<ProductDetails> querySuggestions = await _productRepository.GetSearchSuggestionsAsync(queryText);
                if (querySuggestions != null && querySuggestions.Count > 0)
                {
                    foreach (ProductDetails suggestion in querySuggestions)
                    {
                        ImageUrl = suggestion.Url;
                        if (string.IsNullOrWhiteSpace(suggestion.ProductName))
                        {
                            // No proper suggestion item exists
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(ImageUrl))
                        {
                            var uri = new Uri("ms-appx:///Assets/searchImagePlaceholder_100x100.png", UriKind.Absolute);
                            var imageSource = RandomAccessStreamReference.CreateFromUri(uri);
                            suggestionCollection.AppendResultSuggestion(suggestion.ProductName, suggestion.Sku,
                                suggestion.ProductId, imageSource,
                                ResourceLoader.GetForCurrentView("Resources").GetString("NoImage"));
                            //suggestionCollection.AppendQuerySuggestion(suggestion.ProductName);
                        }
                        else
                        {
                            Uri uri = string.IsNullOrWhiteSpace(ImageUrl)
                                ? new Uri(ResourceLoader.GetForCurrentView("Resources").GetString("NoImageUrl"))
                                : new Uri(ImageUrl);
                            RandomAccessStreamReference imageSource = RandomAccessStreamReference.CreateFromUri(uri);
                            suggestionCollection.AppendResultSuggestion(suggestion.ProductName, suggestion.Sku,
                                suggestion.ProductId, imageSource,
                                ResourceLoader.GetForCurrentView("Resources").GetString("NoImage"));
                        }
                    }
                }
                else
                {
                    suggestionCollection.AppendQuerySuggestion(
                        ResourceLoader.GetForCurrentView("Resources").GetString("NoSuggestions"));
                    NoSuggestions = true;
                }
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }

            deferral.Complete();
        }
    }
}