using Xamarin.Forms;

namespace xamarinJKH.CustomRenderers
{
    public class CustomWebView:WebView
    {
        public static BindableProperty URIProperty = BindableProperty.Create("Uri", typeof(string), typeof(CustomWebView), default(string), BindingMode.TwoWay);
        public string Uri
        {
            get => (string)GetValue(URIProperty);
            set => SetValue(URIProperty, value);
        }
    }
}
