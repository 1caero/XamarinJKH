using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MockupPage : ContentPage
    {
        public MockupPage()
        {
            InitializeComponent();
            var mockups = new List<ImageSource>();
            for(int i=1; i <=Settings.MobileSettings.MockupCount; i++)
            {
                ImageSource imageSource = new UriImageSource
                {
                    Uri = new Uri($"{RestClientMP.SERVER_ADDR}/Public/DownloadMockup/{i}"),
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(5,0,0,0)
                };
                if (imageSource != null)
                {
                    mockups.Add(imageSource);
                }
            }
            
            TheCarousel.ItemsSource = mockups;
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) =>
            {
               await Navigation.PopModalAsync();
            };
            IconViewClose.GestureRecognizers.Add(close);
        }
        
        
    }
}