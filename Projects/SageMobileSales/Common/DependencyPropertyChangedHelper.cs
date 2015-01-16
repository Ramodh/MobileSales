using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.Common
{
    public class DependencyPropertyChangedHelper : DependencyObject
    {
        /// <summary>
        ///     Constructor for the helper.
        /// </summary>
        /// <param name="source">Source object that exposes the DependencyProperty you wish to monitor.</param>
        /// <param name="propertyPath">The name of the property on that object that you want to monitor.</param>
        public DependencyPropertyChangedHelper(DependencyObject source, string propertyPath)
        {
            // Set up a binding that flows changes from the source DependencyProperty through to a DP contained by this helper 
            var binding = new Binding
            {
                Source = source,
                Path = new PropertyPath(propertyPath)
            };
            BindingOperations.SetBinding(this, HelperProperty, binding);
        }

        /// <summary>
        ///     Wrapper property for a helper DependencyProperty used by this class. Only public because the DependencyProperty
        ///     syntax requires it.
        ///     DO NOT use this property directly.
        /// </summary>
        public object Helper
        {
            get { return GetValue(HelperProperty); }
            set { SetValue(HelperProperty, value); }
        }

        // When our dependency property gets set by the binding, trigger the property changed event that the user of this helper can subscribe to
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helper = (DependencyPropertyChangedHelper) d;
            helper.PropertyChanged(d, e);
        }

        /// <summary>
        ///     This event will be raised whenever the source object property changes, and carries along the before and after
        ///     values
        /// </summary>
        public event EventHandler<DependencyPropertyChangedEventArgs> PropertyChanged = delegate { };

        /// <summary>
        ///     Dependency property that is used to hook property change events when an internal binding causes its value to
        ///     change.
        ///     This is only public because the DependencyProperty syntax requires it to be, do not use this property directly in
        ///     your code.
        /// </summary>
        public static DependencyProperty HelperProperty =
            DependencyProperty.Register("Helper", typeof (object), typeof (DependencyPropertyChangedHelper),
                new PropertyMetadata(null, OnPropertyChanged));
    }
}