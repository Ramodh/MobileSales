﻿using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using SageMobileSales.DataAccess.Common;

namespace SageMobileSales.Behaviors
{
    public class HighlightFormFieldOnErrors : Behavior<FrameworkElement>
    {
        public static DependencyProperty PropertyErrorsProperty =
            DependencyProperty.RegisterAttached("PropertyErrors", typeof (ReadOnlyCollection<string>),
                typeof (HighlightFormFieldOnErrors),
                new PropertyMetadata(BindableValidator.EmptyErrorsCollection, OnPropertyErrorsChanged));

        // The default for this property only applies to TextBox controls.
        public static DependencyProperty HighlightStyleNameProperty =
            DependencyProperty.RegisterAttached("HighlightStyleName", typeof (string),
                typeof (HighlightFormFieldOnErrors), new PropertyMetadata("HighlightTextBoxStyle"));

        // The default for this property only applies to TextBox controls.
        protected static DependencyProperty OriginalStyleNameProperty =
            DependencyProperty.RegisterAttached("OriginalStyleName", typeof (Style), typeof (HighlightFormFieldOnErrors),
                new PropertyMetadata("AddContactTextBoxStyle"));

        public ReadOnlyCollection<string> PropertyErrors
        {
            get { return (ReadOnlyCollection<string>) GetValue(PropertyErrorsProperty); }
            set { SetValue(PropertyErrorsProperty, value); }
        }

        public string HighlightStyleName
        {
            get { return (string) GetValue(HighlightStyleNameProperty); }
            set { SetValue(HighlightStyleNameProperty, value); }
        }

        public string OriginalStyleName
        {
            get { return (string) GetValue(OriginalStyleNameProperty); }
            set { SetValue(OriginalStyleNameProperty, value); }
        }

        private static void OnPropertyErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (args == null || args.NewValue == null)
            {
                return;
            }

            FrameworkElement control = ((Behavior<FrameworkElement>) d).AssociatedObject;
            var propertyErrors = (ReadOnlyCollection<string>) args.NewValue;

            Style style = (propertyErrors.Any())
                ? (Style) Application.Current.Resources[((HighlightFormFieldOnErrors) d).HighlightStyleName]
                : (Style) Application.Current.Resources[((HighlightFormFieldOnErrors) d).OriginalStyleName];

            control.Style = style;
        }

        protected override void OnAttached()
        {
        }

        protected override void OnDetached()
        {
        }
    }
}