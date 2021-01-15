using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace xamarinJKH.CustomRenderers
{
    public class SvgIcon:SvgCachedImage
    {
        public static BindableProperty ForegroundProperty = BindableProperty.Create("Foreground", typeof(Color), typeof(Color));
        public Color Foreground
        {
            get => (Color)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        void SetColor(string color)
        {
            var svg = this.Source;
        }
    }
}
