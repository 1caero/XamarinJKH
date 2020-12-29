using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using Xamarin.Essentials;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;
using Rg.Plugins.Popup.Services;
using xamarinJKH.DialogViews;
using System.Threading;

namespace xamarinJKH.MainConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomNavigationConstPage : TabbedPage
    {
        private RestClientMP server = new RestClientMP();
        public Command ChangeTheme { get; set; }
        public BottomNavigationConstPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Color hex = (Color) Application.Current.Resources["MainColor"];
            SelectedTabColor = hex;
            UnselectedTabColor = Color.Gray;
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            Color unselect = hex.AddLuminosity(0.3);
            //switch (currentTheme)
            //{
            //    case OSAppTheme.Light: UnselectedTabColor = unselect;
            //        break;
            //    case OSAppTheme.Dark: UnselectedTabColor = Color.Gray;
            //        break;
            //}

            switch (currentTheme)
            {
                case OSAppTheme.Light:
                    if(Device.RuntimePlatform == Device.Android) UnselectedTabColor = unselect;
                    if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                    {
                        appNavBar.BarTextColor = Color.Black;
                        monNavBar.BarTextColor = Color.Black;
                        //profNavBar.BarTextColor = Color.Black;
                    }
                    break;
                case OSAppTheme.Dark:
                    if(Device.RuntimePlatform == Device.Android) UnselectedTabColor = Color.Gray;
                    if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                    {
                        appNavBar.BarTextColor = Color.White;
                        monNavBar.BarTextColor = Color.White;
                        //profNavBar.BarTextColor = Color.White;
                    }
                    break;
                case OSAppTheme.Unspecified:
                    if(Device.RuntimePlatform == Device.Android) UnselectedTabColor = Color.Gray;
                    if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                    {
                        appNavBar.BarTextColor = Color.Black;
                        monNavBar.BarTextColor = Color.Black;
                        //profNavBar.BarTextColor = Color.Black;
                    }
                    break;
            }

            StartUpdateToken();
            ChangeTheme = new Command(async () =>
            {
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                
                Color unselect = hex.AddLuminosity(0.3);
                switch (currentTheme)
                {
                    case OSAppTheme.Light: 
                        UnselectedTabColor = unselect;
                        if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                        {
                            appNavBar.BarTextColor = Color.Black;
                            monNavBar.BarTextColor = Color.Black;
                            //profNavBar.BarTextColor = Color.Black;
                        }
                            break;
                    case OSAppTheme.Dark: 
                        UnselectedTabColor = Color.Gray;
                        if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                        {
                            appNavBar.BarTextColor = Color.White;
                            monNavBar.BarTextColor = Color.White;
                            //profNavBar.BarTextColor = Color.White;
                        }
                        break;
                    case OSAppTheme.Unspecified:
                        if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                        {
                            appNavBar.BarTextColor = Color.Black;
                            monNavBar.BarTextColor = Color.Black;
                            //profNavBar.BarTextColor = Color.Black;
                        }
                        break;
                }
            });
            MessagingCenter.Subscribe<Object>(this, "ChangeThemeConst", (sender) => ChangeTheme.Execute(null));
            //if (Device.RuntimePlatform == "Android")
                RegisterNewDevice();

            MessagingCenter.Subscribe<Object, int>(this, "SwitchToAppsConst", (sender, args) =>
            {
                this.CurrentPage = this.Children[0];
                MessagingCenter.Send<Object, int>(this, "OpenAppConst", args);
            });

            MessagingCenter.Subscribe<Object, int>(this, "SetRequestsAmount", (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() => RequestsAmount = args);
            });

            BindingContext = this;

        }
        void StartUpdateToken()
        {
            Device.StartTimer(TimeSpan.FromMinutes(5), OnTimerTick);
        }
        int requestsAmount;
        public int RequestsAmount
        {
            get => requestsAmount;
            set
            {
                requestsAmount = value;
                OnPropertyChanged("RequestsAmount");
            }
        }

        private  bool OnTimerTick()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string login = Preferences.Get("loginConst", "");
                string pass = Preferences.Get("passConst", "");
                if (!pass.Equals("") && !login.Equals(""))
                {

                    if (!App.MessageNoInternet)
                        if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                App.MessageNoInternet = true;
                                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK");
                                App.MessageNoInternet = false;
                            });
                            return;
                        }
                    LoginResult loginResult = await server.LoginDispatcher(login, pass);
                    if (loginResult.Error == null)
                    {
                        Settings.Person = loginResult;
                    }
                }

            });
            return true;
        }
        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();
            var i = Children.IndexOf(CurrentPage);
            if (i == 0)
                MessagingCenter.Send<Object>(this, "UpdateAppCons");
        }

        async void RegisterNewDevice()
        {
            if (Device.RuntimePlatform == "Android")
                App.token = DependencyService.Get<xamarinJKH.InterfacesIntegration.IFirebaseTokenObtainer>().GetToken();
            else
            {
                await Task.Delay(500);
            }
            var response = await (new RestClientMP()).RegisterDevice(true);
        }

        public CancellationToken Token { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        Task GeoLocationTask { get; set; }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            var Server = new RestClientMP();
            var location_perm = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
            if (location_perm != PermissionStatus.Granted)
            {
                await PopupNavigation.PushAsync(new LocationNotification());
            }
            else
            {
                if (GeoLocationTask == null)
                {
                    GeoLocationTask = new Task(async () =>
                    {
                        while (!Token.IsCancellationRequested)
                        {
                            try
                            {
                                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                                {
                                    DesiredAccuracy = GeolocationAccuracy.Medium,
                                    Timeout = TimeSpan.FromSeconds(10)
                                });
                                if (location != null)
                                {
                                    var result = await Server.SendGeolocation(location.Latitude, location.Longitude);
                                }
                            }
                            catch { }
                            await Task.Delay(TimeSpan.FromMinutes(5));
                        }
                    }, Token);
                    GeoLocationTask.Start();
                }
                
            }
        }
    }
}
