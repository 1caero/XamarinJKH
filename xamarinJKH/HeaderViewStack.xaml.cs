using System;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Tech;
using xamarinJKH.Main;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderViewStack : ContentView
    {
        public static BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(string));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public HeaderViewStack()
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

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            BindingContext = this;
        }

        async void Tech(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new AppPage());
        }

        void Back(object sender, EventArgs args)
        {
            MessagingCenter.Send<HeaderViewStack>(this, "GoBack");
        }
    }
}