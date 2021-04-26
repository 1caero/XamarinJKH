using System;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.VideoStreaming
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraPage : ContentPage
    {
        string _link;
        public string Link
        {
            get => _link;
            set
            {
                _link = value;
                OnPropertyChanged("Link");
            }
        }
        string _adress;
        public string Adress
        {
            get => _adress;
            set
            {
                _adress = value;
                OnPropertyChanged("Adress");
            }
        }
        bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        double _pHeight;
        public double PlayerHeight
        {
            get => _pHeight;
            set
            {
                _pHeight = value;
                OnPropertyChanged(nameof(PlayerHeight));
            }
        }
        bool rotated;
        public CameraPage(string link, string adress)
        {
            InitializeComponent();
            Analytics.TrackEvent("Камера " + link);
            BindingContext = this;

            IsLoading = true;
            Link = link;
            Adress = adress;
            HeaderViewStack.Title = adress;
            PlayerHeight = 100;
            MessagingCenter.Subscribe<object>(this, "StopLoadingPlayer", sender =>
            {
                IsLoading = false;
                FullScreen.IsVisible = true;
                Video.Opacity = 1;
            });

            MessagingCenter.Subscribe<object, float>(this, "SetRatio", (sender, args) =>
            {
                var width = Video.Width;
                PlayerHeight = Convert.ToDouble(width * args);
            });
            var fullScreenTap = new TapGestureRecognizer();
            fullScreenTap.Tapped += async (s, e) => { Button_Clicked(this, null); };
            FullScreen.GestureRecognizers.Add(fullScreenTap);
            // FullScreen.IsVisible = true;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<object, bool>(this, "FullScreen", rotated);
            rotated = !rotated;
            var parent = this.Parent.Parent as TabbedPage;
            if (rotated)
            {
                Video.ScaleTo(1.4);
                //HeightRequest = App.ScreenWidth;
                Video.Margin = new Thickness(3, -15, 0, 0);
                Video.BackgroundColor = Color.Black;
                (VideoContainer.Parent as StackLayout).BackgroundColor = Color.Black;
                FullScreen.IsVisible = false;

            }
            else
            {
                Video.BackgroundColor = Color.Transparent;
                (VideoContainer.Parent as StackLayout).BackgroundColor = Color.Transparent;
                Video.Margin = new Thickness(0);
                Video.ScaleTo(1);
                FullScreen.IsVisible = true;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (rotated)
            {
                MessagingCenter.Send<object, bool>(this, "FullScreen", rotated);
                Video.BackgroundColor = Color.Transparent;
                (VideoContainer.Parent as StackLayout).BackgroundColor = Color.Transparent;
                Video.Margin = new Thickness(0,-20,0,0);
                Video.ScaleTo(1);
                rotated = !rotated;
                FullScreen.IsVisible = true;
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}