using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;

namespace xamarinJKH.Converters
{
    public class StringEmptyConverters: IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = (string) value;
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return false;
            }
            else
            {
                return Settings.Person.IsDispatcher;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = (string) value;
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}