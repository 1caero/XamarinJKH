using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using Plugin.FirebaseCrashlytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Additional;
using xamarinJKH.CustomRenderers;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.News;
using xamarinJKH.Questions;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.VideoStreaming;
using xamarinJKH.ViewModels;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventsPage : ContentPage
    {
        EventsPageViewModel viewModel { get; set; }

        public EventsPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("События");
            Analytics.TrackEvent(JsonConvert.SerializeObject(Settings.Person.Accounts));
            BindingContext = viewModel = new EventsPageViewModel();
            NavigationPage.SetHasNavigationBar(this, false);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Settings.MobileSettings.сheckCrashSystem)
                {
                    Analytics.TrackEvent("Тестовый краш");
                    int a = 0;
                    int b = 10 / a;
                }
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);

                IconViewProfile.IsVisible = true;
                var profile = new TapGestureRecognizer();
                profile.Tapped += async (s, e) =>
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                        await Navigation.PushAsync(new ProfilePage());
                };
                IconViewProfile.GestureRecognizers.Add(profile);

            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    try
                    {
                        if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
                            phoneDialer.MakePhoneCall(
                                Regex.Replace(Settings.Person.companyPhone, "[^+0-9]",
                                    ""));
                    }
                    catch (Exception ex)
                    {
                    }
                }
            };
            SetText();
            SetColor();
            StartNews();
            StartNotification();
            StartOffers();
            StartQuestions();
            StartOSS();
            CrossFirebaseCrashlytics.Current.SetUserIdentifier(Settings.Person.Login);
            CrossFirebaseCrashlytics.Current.SetUserName(Settings.Person.FIO);
            CrossFirebaseCrashlytics.Current.SetUserEmail(Settings.Person.Email);
            MessagingCenter.Subscribe<Object>(this, "UpdateEvents", (sender) =>
            {
                viewModel.LoadData.Execute(null);

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                    return;
                }
            });
            MessagingCenter.Subscribe<Object>(this, "StartTech",
                async (sender) => { await Navigation.PushAsync(new AppPage()); });
            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) =>
            {
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                var colors = new Dictionary<string, string>();
                var arrowcolor = new Dictionary<string, string>();
                if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
                {
                    colors.Add("#000000", ((Color) Application.Current.Resources["MainColor"]).ToHex());
                    arrowcolor.Add("#000000", "#494949");
                }
                else
                {
                    colors.Add("#000000", "#FFFFFF");
                    arrowcolor.Add("#000000", "#FFFFFF");
                }

            });

            MessagingCenter.Subscribe<Object, (string, string)>(this, "OpenNotification", async (sender, args) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (Navigation.NavigationStack.FirstOrDefault(x => x is NotificationsPage) == null)
                    await Navigation.PushAsync(new NotificationsPage());

                MessagingCenter.Send<Object, AnnouncementInfo>(this, "OpenAnnouncement", Settings.EventBlockData.Announcements.FirstOrDefault(x => x.Header == args.Item2 && x.Text == args.Item1));
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send<Object>(this, "ChangeThemeCounter");
            
        }

        async void SetVisibleControls()
        {
            RestClientMP server = new RestClientMP();
            Settings.EventBlockData = await server.GetEventBlockData();
            setVisible(Settings.EventBlockData.News.Count == 0, StartNews, FrameNews);
            setVisible(Settings.EventBlockData.Polls.Count == 0, StartQuestions, FrameQuestions);
            setVisible(Settings.EventBlockData.Announcements.Count == 0, StartNotification, FrameNotification);
            setVisible(Settings.EventBlockData.AdditionalServices.Count == 0, StartOffers, FrameOffers);
            setVisible(false, StartShop, FrameShop);

            //для ОСС
            setVisible(false, StartOSS, FrameOSS);
        }

        void setVisible(bool visible, Action funk, VisualElement frame)
        {
            if (visible)
            {
                frame.IsVisible = false;
            }
            else
            {
                //funk();
            }
        }

        private void StartNews()
        {
            var startNews = new TapGestureRecognizer();
            startNews.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is NewsPage) == null)
                    await Navigation.PushAsync(new NewsPage());
            };
            FrameNews.GestureRecognizers.Add(startNews);
        }

        private void StartQuestions()
        {
            var startQuest = new TapGestureRecognizer();
            startQuest.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is QuestionsPage) == null)
                    await Navigation.PushAsync(new QuestionsPage());
            };
            FrameQuestions.GestureRecognizers.Add(startQuest);
        }

        private async void StartOffers()
        {
            var startAdditional = new TapGestureRecognizer();
            startAdditional.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalPage) == null)
                    await Navigation.PushAsync(new AdditionalPage());
            };
            FrameOffers.GestureRecognizers.Add(startAdditional);
        }

        private void StartNotification()
        {
            var startNotif = new TapGestureRecognizer();
            startNotif.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is NotificationsPage) == null)
                    await Navigation.PushAsync(new NotificationsPage());
            };
            FrameNotification.GestureRecognizers.Add(startNotif);
        }

        private void StartOSS()
        {
            var startOSSTGR = new TapGestureRecognizer();
            startOSSTGR.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is OSSMain) == null)
                    await Navigation.PushAsync(new OSSMain());
            };
            FrameOSS.GestureRecognizers.Add(startOSSTGR);
        }

        private void StartShop()
        {
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;

        }

        void SetColor()
        {
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            

            FrameNews.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
            FrameQuestions.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
            FrameOffers.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
            FrameOSS.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
            FrameNotification.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
            FrameCameras.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
        }

        private async void Cameras(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is CameraListPage) == null)
                await Navigation.PushAsync(new CameraListPage());
        }
    }

    public class EventsPageViewModel : BaseViewModel
    {
        bool _showNews;

        public bool ShowNews
        {
            get => _showNews;
            set
            {
                _showNews = value;
                OnPropertyChanged("ShowNews");
            }
        }

        bool _showPolls;

        public bool ShowPolls
        {
            get => _showPolls;
            set
            {
                _showPolls = value;
                OnPropertyChanged("ShowPolls");
            }
        }

        bool _showAnnouncements;

        public bool ShowAnnouncements
        {
            get => _showAnnouncements;
            set
            {
                _showAnnouncements = value;
                OnPropertyChanged("ShowAnnouncements");
            }
        }

        bool _showAddService;

        public bool ShowAdditionalServices
        {
            get => _showAddService;
            set
            {
                _showAddService = value;
                OnPropertyChanged("ShowAdditionalServices");
            }
        }  
        bool _ShowOSS;

        public bool ShowOss
        {
            get => _ShowOSS;
            set
            {
                _ShowOSS = value;
                OnPropertyChanged("ShowOss");
            }
        }

        bool _showCameras;

        public bool ShowCameras
        {
            get
            {
                try
                {
                    MobileMenu mobileMenu = Settings.MobileSettings.menu.Find(x => x.name_app == "Web-камеры");
                    return mobileMenu != null && mobileMenu.visible != 0 && Settings.Person.Accounts.Count > 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return true;
                }
            }
            set {  _showCameras = value;
                OnPropertyChanged("ShowCameras"); }
        }

        public Command LoadData { get; set; }
        public Command CountNew { get; set; }

        int announcementsCount;

        public int AnnounsmentsCount
        {
            get => announcementsCount;
            set
            {
                announcementsCount = value;
                OnPropertyChanged(nameof(AnnounsmentsCount));
            }
        }

        int pollsCount;

        public int PollsCount
        {
            get => pollsCount;
            set
            {
                pollsCount = value;
                OnPropertyChanged(nameof(PollsCount));
            }
        }

        int newsCount;
        public int NewsCount
        {
            get => newsCount;
            set
            {
                newsCount = value;
                OnPropertyChanged(nameof(NewsCount));
            }
        }

        public EventsPageViewModel()
        {
            LoadData = new Command(async () =>
            {
                bool isPerson = Settings.Person?.Accounts?.Count > 0;
                var server = new RestClientMP();
                var data = Settings.EventBlockData;
                data = await server.GetEventBlockData();
                Settings.EventBlockData = data;
                if (data != null)
                {
                    if (data.News != null)
                        ShowNews = data.News.Count != 0;
                    if (data.Polls != null)
                        ShowPolls = data.Polls.Count != 0 && Settings.QuestVisible && isPerson;
                    if (data.AdditionalServices != null)
                        ShowAdditionalServices = data.AdditionalServices.Count != 0 && Settings.AddVisible;
                    if (data.Announcements != null)
                        ShowAnnouncements = data.Announcements.Count != 0 && Settings.NotifVisible && isPerson;
                    
                    MobileMenu mobileMenu = Settings.MobileSettings.menu.Find(x => x.name_app == "Web-камеры");
                    if (mobileMenu != null)
                    {
                        ShowCameras = mobileMenu.visible != 0 && Settings.Person?.Accounts?.Count > 0;
                    }

                    if (!RestClientMP.SERVER_ADDR.ToLower().Contains("water"))
                    {
                        if(Settings.Person != null && (!Settings.MobileSettings.enableOSS && !Settings.Person.accessOSS))
                        {
                            ShowOss = false;
                        }
                        else
                        {
                            ShowOss = true;
                        }
                    }
                    else
                    {
                        
                            ShowOss = true;
                    }
                }
                ShowAdditionalServices = false;
                CountNew.Execute(null);
            });
            CountNew = new Command(() =>
            {
                try
                {
                    if (Settings.EventBlockData?.Announcements != null)
                        AnnounsmentsCount = Settings.EventBlockData.Announcements.Count(x => !x.IsReaded);
                    else
                        Analytics.TrackEvent($"Announcements is null");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    AnnounsmentsCount = 0;
                }

                try
                {
                    if (Settings.EventBlockData?.Polls != null)
                        PollsCount = Settings.EventBlockData.Polls.Count(x => !x.IsReaded);
                    else
                        Analytics.TrackEvent($"Polls is null");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    PollsCount = 0;
                }

                try
                {
                    if (Settings.EventBlockData?.News != null)
                        if(Settings.Person?.Accounts?.Count > 0)
                            NewsCount = Settings.EventBlockData.News.Count(x => !x.IsReaded);
                        else
                        {
                            NewsCount = 0;
                        }
                    else
                        Analytics.TrackEvent($"News is null");
                }
                catch
                {
                    NewsCount = 0;
                }
                MessagingCenter.Send<Object, int>(this, "SetEventsAmount", PollsCount + AnnounsmentsCount + NewsCount);
            });
            MessagingCenter.Subscribe<Object>(this, "ReducePolls", sender =>
            {
                PollsCount--;
            });

            MessagingCenter.Subscribe<Object>(this, "ReduceAnnounsements", sender =>
            {
                AnnounsmentsCount--;
            });

            MessagingCenter.Subscribe<Object>(this, "ReduceNews", sender =>
            {
                NewsCount--;
            });
        }
    }
}