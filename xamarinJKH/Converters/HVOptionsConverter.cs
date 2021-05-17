using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.Converters
{
    public class HvOptionsConverter: IMultiValueConverter, IMarkupExtension
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.IsAssignableFrom(typeof(LayoutOptions)))
            {
                if (values == null)
                {
                    return LayoutOptions.EndAndExpand;
                }

                foreach (var value in values)
                {
                    string first = value?.ToString();
                    if (!string.IsNullOrWhiteSpace(first))
                    {
                        return LayoutOptions.EndAndExpand;
                    }
                }

                return LayoutOptions.StartAndExpand;
            }
            else
            {
                if (values == null)
                {
                    return false;
                }
                foreach (var value in values)
                {
                    string first = value?.ToString();
                    if (!string.IsNullOrWhiteSpace(first))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[4];
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}