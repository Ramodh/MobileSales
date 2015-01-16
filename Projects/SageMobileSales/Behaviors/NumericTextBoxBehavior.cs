using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace SageMobileSales.Behaviors
{
    /// Simple NumericTextBox behavior for Windows 8.1
    public sealed class NumericTextBoxBehavior
        : DependencyObject, IBehavior
    {
        /// Backing storage for the AllowDecimal property
        public static readonly DependencyProperty AllowDecimalProperty = DependencyProperty.Register(
            "AllowDecimal",
            typeof (bool),
            typeof (NumericTextBoxBehavior),
            new PropertyMetadata(false));

        /// Track the last valid text value.
        private string _lastText;

        /// True to allow a decimal point.
        public bool AllowDecimal
        {
            get { return (bool) GetValue(AllowDecimalProperty); }

            set { SetValue(AllowDecimalProperty, value); }
        }

        /// Used to attach this behavior to an element.
        /// Must be a TextBox.
        ///  
        /// TextBox to assocate this behavior with.
        public void Attach(DependencyObject associatedObject)
        {
            var tb = associatedObject as TextBox;
            if (tb == null)
            {
                throw new ArgumentException("NumericTextBoxBehavior can only be used with a TextBox.");
            }

            AssociatedObject = associatedObject;

            _lastText = tb.Text;
            tb.TextChanged += TbOnTextChanged;
            if (tb.InputScope == null)
            {
                var inputScope = new InputScope();
                inputScope.Names.Add(new InputScopeName(InputScopeNameValue.Number));
                tb.InputScope = inputScope;
            }
        }

        /// Detaches the behavior from the TextBox.
        public void Detach()
        {
            var tb = AssociatedObject as TextBox;
            if (tb != null)
            {
                tb.TextChanged -= TbOnTextChanged;
            }
        }

        /// The associated object (TextBox).
        public DependencyObject AssociatedObject { get; private set; }

        /// Handles the TextChanged event on the TextBox and watches for
        /// numeric entries.
        private void TbOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = AssociatedObject as TextBox;
            if (tb != null)
            {
                if (AllowDecimal)
                {
                    double value;
                    if (string.IsNullOrEmpty(tb.Text) ||
                        Double.TryParse(tb.Text, out value))
                    {
                        _lastText = tb.Text;
                        return;
                    }
                }
                else
                {
                    long value;
                    if (string.IsNullOrEmpty(tb.Text) ||
                        long.TryParse(tb.Text, out value))
                    {
                        _lastText = tb.Text;
                        return;
                    }
                }

                tb.Text = _lastText;
                tb.SelectionStart = tb.Text.Length;
            }
        }
    }
}