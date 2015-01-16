using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.Converters
{
    internal class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var msg = value as string;
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg.Equals(PageUtils.PreviousOrderText) || msg.Equals(PageUtils.PreviousPurchasedItemsText) ||
                    msg.Equals(PageUtils.ScratchText))
                    return new SolidColorBrush(Color.FromArgb(255, 53, 91, 78));
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