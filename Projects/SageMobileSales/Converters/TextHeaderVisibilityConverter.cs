using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.Converters
{
    public class TextHeaderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value is string)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                return value.ToString();
            }
            return null;
        }
    }
}