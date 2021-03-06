﻿using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.Converters
{
    public class TarifThreeConverter: IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int tarifNumber = (int) value;
            if (tarifNumber == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int tarifNumber = (int) value;
            if (tarifNumber == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}