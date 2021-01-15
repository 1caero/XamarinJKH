using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;
using xamarinJKH.InterfacesIntegration;
using Plugin.Messaging;
using xamarinJKH.Tech;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderViewMain : ContentView
    {
        public static BindableProperty PictureSource = BindableProperty.Create("Picture", typeof(string), typeof(string));
        public static BindableProperty TitleSource = BindableProperty.Create("Title", typeof(string), typeof(string));
        public string Phone
        {
            get => Settings.Person.companyPhone.Replace("+", "");
        }

        public string Name
        {
            get => Settings.MobileSettings.main_name;
        }

        public string Picture
        {
            get => (string)GetValue(PictureSource);
            set
            {
                SetValue(PictureSource, value);
                OnPropertyChanged("Picture");
            }
        }

        
        public string Title
        {
            get => (string)GetValue(TitleSource);
            set
            {
                SetValue(TitleSource, value);
                OnPropertyChanged("Title");
            }
        }
        public HeaderViewMain()
        {
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }
            BindingContext = this;
        }

        private void Call(object sender, EventArgs args)
        {
            if (Settings.Person.Phone != null)
            {
                IPhoneCallTask phoneDialer;
                phoneDialer = CrossMessaging.Current.PhoneDialer;
                if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
                    phoneDialer.MakePhoneCall(System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
            }
        }

        private async void Tech(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new AppPage());
        }
    }
}