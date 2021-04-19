using System;
using System.Globalization;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Mask;

namespace xamarinJKH.Converters
{
    public class PancakeBorderConverter: IValueConverter, IMarkupExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"> забиндил IsChangeTheme в это поле везде при открытии формы его значение меняем что бы конвертер запускался</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            Color resource = (Color) Application.Current.Resources["MainColor"];
            // если передаем параметры цвет рамки системный
            if (parameter != null)
            {
                return new Border {Thickness = 1, Color = resource};
            }
            // Если тип value Color то такой цвет рамки и устанавливается 
            try
            {
                Color color = (Color) value;
                return new Border {Thickness = 4, Color = color};
            }
            catch
            {
                // Если value какай нибудь другой цет устанавливаем рамку для светлой темы системным цветом для темной прозрачной
                Border border = new Border {Thickness = 1};
                if (Application.Current.UserAppTheme == OSAppTheme.Light)
                {
                    border.Color = resource;
                }
                else
                {
                    border.Color = Color.Transparent;
                }

                return border;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           return  new Border{Color = Color.Transparent, Thickness = 1};
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}