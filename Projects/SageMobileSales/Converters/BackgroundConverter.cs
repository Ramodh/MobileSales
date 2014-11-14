using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SageMobileSales.Converters
{
    class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var msg = value as string;
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg.Equals(PageUtils.PreviousOrderText) || msg.Equals(PageUtils.PreviousPurchasedItemsText) || msg.Equals(PageUtils.ScratchText))
                    return new SolidColorBrush(Color.FromArgb(255, 53, 91, 78));
                else
                    return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Color.FromArgb(255, 53, 91, 78));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
