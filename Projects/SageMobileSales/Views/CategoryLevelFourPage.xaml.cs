using Microsoft.Practices.Prism.StoreApps;
using SageMobileSales.Common;
using System.ComponentModel;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SageMobileSales.Views
{
    /// <summary>
    ///     A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CategoryLevelFourPage : VisualStateAwarePage
    {
        private double _scrollViewerOffsetProportion;
        private bool _isPageLoading = true;
        private ScrollViewer _itemsGridViewScrollViewer;
        public CategoryLevelFourPage()
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
            if (e.PropertyName == "ProductCategoryList")
            {
                itemGridView.ItemsSource = itemsViewSource.Source;
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
            var helper = (DependencyPropertyChangedHelper)sender;

            var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemGridView);

            if (((Visibility)e.NewValue) == Visibility.Visible)
            {
                ScrollViewerUtilities.ScrollToProportion(scrollViewer, _scrollViewerOffsetProportion);
                helper.PropertyChanged -= ScrollBarVisibilityChanged;
            };

            if (_isPageLoading)
            {
                itemGridView.LayoutUpdated += itemsGridView_LayoutUpdated;
                _isPageLoading = false;
            }
        }

        protected override void SaveState(System.Collections.Generic.Dictionary<string, object> pageState)
        {
            if (pageState == null) return;

            base.SaveState(pageState);

            pageState["scrollViewerOffsetProportion"] = ScrollViewerUtilities.GetScrollViewerOffsetProportion(_itemsGridViewScrollViewer);
        }

        protected override void LoadState(object navigationParameter, System.Collections.Generic.Dictionary<string, object> pageState)
        {
            if (pageState == null) return;

            base.LoadState(navigationParameter, pageState);

            if (pageState.ContainsKey("scrollViewerOffsetProportion"))
            {
                _scrollViewerOffsetProportion = double.Parse(pageState["scrollViewerOffsetProportion"].ToString(), CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var scrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemGridView);

            if (scrollViewer != null)
            {
                if (scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible && scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    ScrollViewerUtilities.ScrollToProportion(scrollViewer, _scrollViewerOffsetProportion);
                }
                else
                {
                    DependencyPropertyChangedHelper horizontalHelper = new DependencyPropertyChangedHelper(scrollViewer, "ComputedHorizontalScrollBarVisibility");
                    horizontalHelper.PropertyChanged += ScrollBarVisibilityChanged;

                    DependencyPropertyChangedHelper verticalHelper = new DependencyPropertyChangedHelper(scrollViewer, "ComputedVerticalScrollBarVisibility");
                    verticalHelper.PropertyChanged += ScrollBarVisibilityChanged;
                }
            }
        }

        private void itemsGridView_LayoutUpdated(object sender, object e)
        {
            _scrollViewerOffsetProportion = ScrollViewerUtilities.GetScrollViewerOffsetProportion(_itemsGridViewScrollViewer);
        }

        private void itemsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            _itemsGridViewScrollViewer = VisualTreeUtilities.GetVisualChild<ScrollViewer>(itemGridView);
        }
    }
}