using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SageMobileSales.Common;
using Microsoft.Practices.Prism.StoreApps;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace SageMobileSales.Views
{
    /// <summary>
    ///     A page that displays a collection of item previews.  In the Split Application this page
    ///     is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class QuotesPage : VisualStateAwarePage
    {
        private bool _isPageLoading = true;
        private ScrollViewer _itemsGridViewScrollViewer;
        private double _scrollViewerOffsetProportion;

        public QuotesPage()
        {
            InitializeComponent();
            this.SizeChanged += Page_SizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var viewModel = DataContext as INotifyPropertyChanged;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += viewModel_PropertyChanged;
            }
            base.OnNavigatedTo(e);
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "QuoteDetails")
            {
                itemsGridView.ItemsSource = itemsViewSource.Source;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var sageMobileSales = Application.Current as App;
            var viewModel = DataContext as INotifyPropertyChanged;
            if (sageMobileSales != null && !sageMobileSales.IsSuspending && viewModel != null)
            {
                viewModel.PropertyChanged -= viewModel_PropertyChanged;
            }
            base.OnNavigatedFrom(e);
        }

        private void ScrollBarVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var helper = (DependencyPropertyChangedHelper) sender;

            var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemsGridView);

            if (((Visibility) e.NewValue) == Visibility.Visible)
            {
                ScrollViewerUtilities.ScrollToProportion(scrollViewer, _scrollViewerOffsetProportion);
                helper.PropertyChanged -= ScrollBarVisibilityChanged;
            }
            ;

            if (_isPageLoading)
            {
                itemsGridView.LayoutUpdated += itemsGridView_LayoutUpdated;
                _isPageLoading = false;
            }
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            if (pageState == null) return;

            base.SaveState(pageState);

            pageState["scrollViewerOffsetProportion"] =
                ScrollViewerUtilities.GetScrollViewerOffsetProportion(_itemsGridViewScrollViewer);
        }

        protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
        {
            if (pageState == null) return;

            base.LoadState(navigationParameter, pageState);

            if (pageState.ContainsKey("scrollViewerOffsetProportion"))
            {
                _scrollViewerOffsetProportion = double.Parse(pageState["scrollViewerOffsetProportion"].ToString(),
                    CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemsGridView);

            if (scrollViewer != null)
            {
                if (scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible &&
                    scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    ScrollViewerUtilities.ScrollToProportion(scrollViewer, _scrollViewerOffsetProportion);
                }
                else
                {
                    var horizontalHelper = new DependencyPropertyChangedHelper(scrollViewer,
                        "ComputedHorizontalScrollBarVisibility");
                    horizontalHelper.PropertyChanged += ScrollBarVisibilityChanged;

                    var verticalHelper = new DependencyPropertyChangedHelper(scrollViewer,
                        "ComputedVerticalScrollBarVisibility");
                    verticalHelper.PropertyChanged += ScrollBarVisibilityChanged;
                }
            }
        }

        private void itemsGridView_LayoutUpdated(object sender, object e)
        {
            _scrollViewerOffsetProportion =
                ScrollViewerUtilities.GetScrollViewerOffsetProportion(_itemsGridViewScrollViewer);
        }

        private void itemsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            _itemsGridViewScrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemsGridView);
        }
    }
}