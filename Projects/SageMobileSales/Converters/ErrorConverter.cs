using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.Converters
{
    public sealed class ErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ICollection<string> errors = value as ICollection<string>;
            return errors != null && errors.Count > 0 ? errors.ElementAt(0) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
