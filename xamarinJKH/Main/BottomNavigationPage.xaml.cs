using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.MainConst;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomNavigationPage : TabbedPage
    {
        private RestClientMP server = new RestClientMP();

        public Command ChangeTheme { get; set; }
        int request_amount;
        public int RequestsAmount
        {
            get => request_amount;
            set
            {
                request_amount = value;
                OnPropertyChanged("RequestsAmount");
            }
        }

        int events_amount;
        public int EventsAmount
        {
            get => events_amount;
            set
            {
                if(Settings.Person.Accounts.Count > 0)
                    events_amount = value;
                else
                {
                    events_amount = 0;
                }
                OnPropertyChanged(nameof(EventsAmount));
            }
        }

        public BottomNavigationPage()
        {
            App.isStart = true;
            Analytics.TrackEvent("Форма нижнего меню");
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;
            Color hex = (Color)Application.Current.Resources["MainColor"];
            //SelectedTabColor = hex;
            GetBrand();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            
            Color unselect = hex.AddLuminosity(0.3); //hex.AddLuminosity(0.3);
            
            switch (currentTheme)
            {
                case OSAppTheme.Light:
                    if(Device.RuntimePlatform == Device.Android) UnselectedTabColor = unselect;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        EventsNavPage.BarTextColor = Color.Black;
                        PayPage.BarTextColor = Color.Black;
                        CounterPage.BarTextColor = Color.Black;
                        AppPage.BarTextColor = Color.Black;
                        ShopNavPage.BarTextColor = Color.Black;
                        ShopNavPage2.BarTextColor = Color.Black;
                    }

                    break;
                case OSAppTheme.Dark:
                    if(Device.RuntimePlatform == Device.Android) UnselectedTabColor = Color.Gray;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        EventsNavPage.BarTextColor = Color.White;
                        PayPage.BarTextColor = Color.White;
                        CounterPage.BarTextColor = Color.White;
                        AppPage.BarTextColor = Color.White;
                        ShopNavPage.BarTextColor = Color.White;
                        //ProfPage.BarTextColor = Color.White;
                        ShopNavPage2.BarTextColor = Color.White;
                    }

                    break;
                case OSAppTheme.Unspecified:
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        switch (Settings.MobileSettings.appTheme)
                        {
                            case "":
                            case "light":
                                EventsNavPage.BarTextColor = Color.Black;
                                PayPage.BarTextColor = Color.Black;
                                CounterPage.BarTextColor = Color.Black;
                                AppPage.BarTextColor = Color.Black;
                                ShopNavPage.BarTextColor = Color.Black;
                                //ProfPage.BarTextColor = Color.Black;
                                ShopNavPage2.BarTextColor = Color.Black;
                                break;
                            case "dark":
                                EventsNavPage.BarTextColor = Color.White;
                                PayPage.BarTextColor = Color.White;
                                CounterPage.BarTextColor = Color.White;
                                AppPage.BarTextColor = Color.White;
                                ShopNavPage.BarTextColor = Color.White;
                                //ProfPage.BarTextColor = Color.White;
                                ShopNavPage2.BarTextColor = Color.White;
                                break;
                        }
                    }

                    break;
            }


            Application.Current.Resources["Saldo"] = true;
            VisibleMenu();
            StartUpdateToken();
            if (Device.RuntimePlatform == Device.Android)
                RegisterNewDevice();

            MessagingCenter.Subscribe<Object, int>(this, "SwitchToApps", (sender, index) =>
            {
                try
                {
                    this.CurrentPage = AppPage;
                    MessagingCenter.Send<Object, int>(this, "OpenApp", index);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Analytics.TrackEvent(e.Message);
                }
                
            });

            ChangeTheme = new Command(async () =>
            {
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                //только темная тема в ios

                Color unselect = hex.AddLuminosity(0.3);
                switch (currentTheme)
                {
                    case OSAppTheme.Light:
                        UnselectedTabColor = unselect;
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            EventsNavPage.BarTextColor = Color.Black;
                            PayPage.BarTextColor = Color.Black;
                            CounterPage.BarTextColor = Color.Black;
                            AppPage.BarTextColor = Color.Black;
                            ShopNavPage.BarTextColor = Color.Black;
                            ShopNavPage2.BarTextColor = Color.Black;
                        }

                        break;
                    case OSAppTheme.Dark:
                        UnselectedTabColor = Color.Gray;
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            EventsNavPage.BarTextColor = Color.White;
                            PayPage.BarTextColor = Color.White;
                            CounterPage.BarTextColor = Color.White;
                            AppPage.BarTextColor = Color.White;
                            ShopNavPage.BarTextColor = Color.White;
                            ShopNavPage2.BarTextColor = Color.White;
                        }

                        break;
                    case OSAppTheme.Unspecified:
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            EventsNavPage.BarTextColor = Color.Black;
                            PayPage.BarTextColor = Color.Black;
                            CounterPage.BarTextColor = Color.Black;
                            AppPage.BarTextColor = Color.Black;
                            ShopNavPage.BarTextColor = Color.Black;
                            //ProfPage.BarTextColor = Color.Black;
                            ShopNavPage2.BarTextColor = Color.Black;
                        }

                        break;
                }
            });

            Loadtab();

            MessagingCenter.Subscribe<Object>(this, "ChangeTheme", (sender) => ChangeTheme.Execute(null));

            MessagingCenter.Subscribe<Object, int>(this, "SetRequestsAmount", (sender, args) =>
            {
                if (args == -1)
                    RequestsAmount -= 1;
                else
                    RequestsAmount = args;
            });

            MessagingCenter.Subscribe<Object, int>(this, "SetEventsAmount", (sender, args) =>
            {
                if (args == -1)
                    EventsAmount -= 1;
                else
                    EventsAmount = args;
            });

            MessagingCenter.Subscribe<Object, string>(this, "RemoveIdent", (sender, args) =>
            {
                if (args == null)
                    RequestsAmount = 0;
            });

            MessagingCenter.Subscribe<Object, (string, string)>(this, "OpenNotification", (sender, args) =>
            {
                this.CurrentPage = this.EventsNavPage;
            });
        }

        void StartUpdateToken()
        {
            Device.StartTimer(TimeSpan.FromMinutes(5), OnTimerTick);
        }

        async void SetTab(string title)
        {
            var services = this.Children.FirstOrDefault(x => x.Title == title);
            if (services != null)
            {
                this.CurrentPage = services;
            }
        }

        async void Loadtab()
        {
            Analytics.TrackEvent("Установка данных для вкладок");
            if (Settings.MobileSettings == null)
            {
                Analytics.TrackEvent("Ожидание подгрузки данных");
            }
            while (Settings.MobileSettings == null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
            if (Settings.MobileSettings != null)
                if (!string.IsNullOrEmpty(Settings.MobileSettings.startScreen))
                    switch (Settings.MobileSettings.startScreen.Trim())
                    {
                        case "Оплата":
                            {
                                SetTab(AppResources.Pays);
                                break;
                            }
                        case "События":
                            {
                                SetTab(AppResources.Events_NavBar);
                                break;
                            }
                        case "Показания":
                            {
                                SetTab(AppResources.Meters_NavBar);
                                break;
                            }
                        case "Наши услуги":
                            {
                                SetTab(AppResources.Shop_NavBar);
                                await Task.Delay(TimeSpan.FromMilliseconds(700));
                                MessagingCenter.Send<Object>(this, "LoadGoods");
                                break;
                            }
                        default:
                            SetTab(AppResources.Events_NavBar);
                            break;
                    }
        }

        private void GetBrand()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                ItemsList<string> vehicleMarks = await server.VehicleMarks();
                if (vehicleMarks.Error == null || vehicleMarks.Data == null)
                {
                    Settings.BrandCar = vehicleMarks.Data;
                }
                else
                {
                    Settings.BrandCar = new List<string>();
                }
            });
        }
        private bool OnTimerTick()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string login = Preferences.Get("login", "");
                string pass = Preferences.Get("pass", "");
                if (!pass.Equals("") && !login.Equals(""))
                {
                    if (!App.MessageNoInternet)
                        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                App.MessageNoInternet = true;
                                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK");
                                App.MessageNoInternet = false;
                            });
                            return;
                        }

                    LoginResult loginResult = await server.Login(login, pass);
                    if (loginResult.Error == null)
                    {
                        Settings.Person = loginResult;
                    }
                }
            });
            return true;
        }

        void VisibleMenu()
        {
            try
            {
                Children.Remove(ShopNavPage);
                if (!Settings.MobileSettings.showOurService)
                {
                    
                    Children.Remove(ShopNavPage2);
                }

                foreach (var each in Settings.MobileSettings.menu)
                {
                    if (each.name_app.Equals("Заявки"))
                    {
                        if (each.visible == 0)
                        {
                            Children.Remove(AppPage);
                            Settings.AppIsVisible = false;
                        }
                    }
                    else if (each.name_app.Equals("Показания счетчиков") || each.name_app.Equals("Показания приборов"))
                    {
                        if (each.visible == 0)
                        {
                            Children.Remove(CounterPage);
                        }
                    }
                    else if (each.name_app.Equals("Оплата ЖКУ"))
                    {
                        if (each.visible == 0)
                        {
                            Children.Remove(PayPage);
                        }
                    }
                    else if (each.name_app.Equals("Уведомления"))
                    {
                        if (each.visible == 0)
                        {
                            Settings.NotifVisible = false;
                        }
                    }
                    else if (each.name_app.Equals("Опросы"))
                    {
                        if (each.visible == 0)
                        {
                            Settings.QuestVisible = false;
                        }
                    }
                    else if (each.name_app.Equals("Квитанции"))
                    {
                        if (each.visible == 0)
                        {
                            Application.Current.Resources["Saldo"] = false;
                        }
                    }
                    else if (each.name_app.Equals("Дополнительные услуги"))
                    {
                        if (each.visible == 0)
                        {
                            if (RestClientMP.SERVER_ADDR.Contains("komfortnew"))
                            {
                                Children.Remove(ShopNavPage2);
                            }
                            else
                            {
                                Children.Remove(ShopNavPage);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();
            var i = Children.IndexOf(CurrentPage);
            if (i == 0)
                MessagingCenter.Send<Object>(this, "UpdateEvents");

            if (CurrentPage != null)
            {
                if (CurrentPage is AppsPage || CurrentPage is AppsConstPage ||
                    CurrentPage.Title == AppResources.App_NavBar)
                    MessagingCenter.Send<Object>(this, "AutoUpdate");

                if (CurrentPage.Title == AppResources.Shop_NavBar)
                    MessagingCenter.Send<Object>(this, "LoadGoods");
            }
        }


        async void RegisterNewDevice()
        {
            App.token = DependencyService.Get<IFirebaseTokenObtainer>().GetToken();
            var response = await (new RestClientMP()).RegisterDevice();
        }
    }
}