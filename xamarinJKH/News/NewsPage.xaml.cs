using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.News
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewsPage : ContentPage
    {
        public ObservableCollection<NewsInfo> NewsInfos { get; set; }
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();
        private bool _isChangeTheme;

        public bool IsChangeTheme
        {
            get => _isChangeTheme;
            set
            {
                _isChangeTheme = value;
                OnPropertyChanged(nameof(IsChangeTheme));
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsChangeTheme = !IsChangeTheme;
        }
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    await RefreshData();

                    IsRefreshing = false;
                });
            }
        }

        private async Task RefreshData()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
                
            }

            if (!isAll)
            {
                List<NewsInfo> newsInfos = await server.AllNews();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    NewsInfos = new ObservableCollection<NewsInfo>();
                    foreach (var each in newsInfos)
                    {
                        NewsInfos.Add(each);
                    }
                });
            }
            else
            {
                Settings.EventBlockData = await server.GetEventBlockData();
                if (Settings.EventBlockData.Error == null)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        foreach (var each in Settings.EventBlockData.News)
                        {
                            NewsInfos.Add(each);
                        }
                    });
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNewsInfo, "OK");
                }
            }
        }

        public NewsPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Список новостей");
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake2.Padding = new Thickness(0, statusBarHeight, 0, 0);
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

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            LabelPhone.GestureRecognizers.Add(call);
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            if (Settings.EventBlockData!=null && Settings.EventBlockData.News != null)
                NewsInfos = new ObservableCollection<NewsInfo>(Settings.EventBlockData.News);
            else
                NewsInfos = new ObservableCollection<NewsInfo>();

            this.BindingContext = this;
            NotificationList.BackgroundColor = Color.Transparent;
            NotificationList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));

        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            if (!string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
            {
                LabelPhone.Text = "+" + Settings.Person.companyPhone.Replace("+", "");
            }
            else
            {
                IconViewLogin.IsVisible = false;
                LabelPhone.IsVisible = false;
            }
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            
            // Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            // PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);{ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            NewsInfo select = e.Item as NewsInfo;
            if (Navigation.NavigationStack.FirstOrDefault(x => x is NewPage) == null)
                await Navigation.PushAsync(new NewPage(select));
            MessagingCenter.Send<Object, int>(this, "SetNotificationRead", select.ID);
        }

        private bool isAll = true;
        private void Button_Clicked(object sender, EventArgs e)
        {
            new Task(async () =>
            {
                Configurations.LoadingConfig = new LoadingConfig
                {
                    IndicatorColor =(Color) Application.Current.Resources["MainColor"],
                    OverlayColor = Color.Black,
                    Opacity = 0.8,
                    DefaultMessage = AppResources.Loading,
                };
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Loading.Instance.StartAsync(async progress =>
                    {
                        if (isAll)
                        {
                            List<NewsInfo> newsInfos = await server.AllNews();
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                NewsInfos = new ObservableCollection<NewsInfo>();
                                foreach (var each in newsInfos)
                                {
                                    NewsInfos.Add(each);
                                }
                                SeeAll.Text = AppResources.SeeNews;
                                isAll = false;
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                foreach (var each in Settings.EventBlockData.News)
                                {
                                    NewsInfos.Add(each);
                                }
                                SeeAll.Text = AppResources.AllNews;
                                isAll = true;
                            });
                        }
                    });
                });
                
            }).Start();
        }
    }
}