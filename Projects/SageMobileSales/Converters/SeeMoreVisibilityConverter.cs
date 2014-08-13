using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SageMobileSales.Converters
{
    public class SeeMoreVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            string val = value.ToString();

            if (val.Equals("See More"))
            {
                return new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-Appx:///Assets/img_see_more.png", UriKind.RelativeOrAbsolute)) };
            }
            return new SolidColorBrush(Color.FromArgb(255, 53, 91, 78));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
