using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.Common
{
    public sealed class FlipViewIndicator : ListView
    {
        /// <summary>
        ///     Identifies the "FlipView" dependency property
        /// </summary>
        public static readonly DependencyProperty FlipViewProperty =
            DependencyProperty.Register("FlipView", typeof (FlipView), typeof (FlipViewIndicator),
                new PropertyMetadata(null, (depobj, args) =>
                {
                    var fvi = (FlipViewIndicator) depobj;
                    var fv = (FlipView) args.NewValue;

                    fv.SelectionChanged += (s, e) => { fvi.ItemsSource = fv.ItemsSource; };

                    fvi.ItemsSource = fv.ItemsSource;

                    // create the element binding source
                    var eb = new Binding();
                    eb.Mode = BindingMode.TwoWay;
                    eb.Source = fv;
                    eb.Path = new PropertyPath("SelectedItem");

                    // set the element binding to change selection when the FlipView changes
                    fvi.SetBinding(SelectedItemProperty, eb);
                }));

        /// <summary>
        ///     Initializes a new instance of the "FlipViewIndicator" class.
        /// </summary>
        public FlipViewIndicator()
        {
            DefaultStyleKey = typeof (FlipViewIndicator);
        }

        /// <summary>
        ///     Gets or sets the flip view.
        /// </summary>
        public FlipView FlipView
        {
            get { return (FlipView) GetValue(FlipViewProperty); }
            set { SetValue(FlipViewProperty, value); }
        }
    }
}