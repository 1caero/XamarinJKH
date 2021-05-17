using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.Converters
{
    public class EmptyVisibleConverter: IMultiValueConverter, IMarkupExtension
    {
       

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return Color.Transparent; 
                // Alternatively, return BindableProperty.UnsetValue to use the binding FallbackValue
            }

            string first = values[0]?.ToString();
            string second = values[1]?.ToString();
            if ((string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second))
                || (!string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second)))
            {
                return Color.Transparent;
            }
            else
            {
                return Color.Black;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[2];
        }
    }
}