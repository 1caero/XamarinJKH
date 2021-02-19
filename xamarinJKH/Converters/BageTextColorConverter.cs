using System;
using System.Globalization;
using Xamarin.Forms;

namespace xamarinJKH.Converters
{
    public class BageTextColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            return val > 0 ? Color.White : Color.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}