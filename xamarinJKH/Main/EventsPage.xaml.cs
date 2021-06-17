using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
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
    /*!
 \b Форма событий УК
*/
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EventsPage : ContentPage
    {
        EventsPageViewModel viewModel { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
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
            MessagingCenter.Subscribe<Object>(this, "UpdateEvents", (sender) =>
            {
                try
                {
                    Analytics.TrackEvent("Загрузка событий");
                    viewModel?.LoadData?.Execute(null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Analytics.TrackEvent(e.Message);
                }

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
        /// <summary>
        /// Обработка переоткрытия формы
        /// </summary>
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
        /// <summary>
        /// Установка видимости вьюх
        /// </summary>
        /// <param name="visible">отображать или нет</param>
        /// <param name="funk">функция для открытия формы</param>
        /// <param name="frame">Блок которой нужно скрыть/показать</param>
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

        /// <summary>
        /// Открытие формы новостей
        /// </summary>
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

        /// <summary>
        /// Открытие формы опросов
        /// </summary>
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

        /// <summary>
        /// Открытие формы уведомлений
        /// </summary>
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
        /// <summary>
        /// Открытие формы общеего голосования собственников
        /// </summary>
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
        /// <summary>
        /// Установка названия УК
        /// </summary>
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
        }
        /// <summary>
        /// Установка фирменного цвета УК
        /// </summary>
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

        /// <summary>
        /// Открытие Формы просмотра камер видеонаблюдения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Cameras(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is CameraListPage) == null)
                await Navigation.PushAsync(new CameraListPage());
        }
    }
    /*!
 \b Вью модель для взаимодействия с формой событий УК
*/
    public class EventsPageViewModel : BaseViewModel
    {
        bool _showNews;
        /// <summary>
        /// Установка видимости новостей
        /// </summary>
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
        /// <summary>
        /// Установка видимости опросов
        /// </summary>
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
        /// <summary>
        /// Установка видимости уведомлений
        /// </summary>
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
        /// <summary>
        /// Установка видимости Общего голосования собственнеков
        /// </summary>
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
        /// <summary>
        /// Установка видимости web камер
        /// </summary>
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
        /// <summary>
        /// Команда загрузки данных с сервера
        /// </summary>
        public Command LoadData { get; set; }
        /// <summary>
        /// Команда подсчета новых событиф для отображения в бейдже
        /// </summary>
        public Command CountNew { get; set; }

        int announcementsCount;
        /// <summary>
        /// Кол-во новых уведомлений
        /// </summary>
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
        /// <summary>
        /// Кол-во новых опросов
        /// </summary>
        public int PollsCount
        {
            get => Settings.QuestVisible ? pollsCount : 0;
            set
            {
                pollsCount = value;
                OnPropertyChanged(nameof(PollsCount));
            }
        }

        int newsCount;
        /// <summary>
        /// Кол -во новых новостей
        /// </summary>
        public int NewsCount
        {
            get => newsCount;
            set
            {
                newsCount = value;
                OnPropertyChanged(nameof(NewsCount));
            }
        }
        /// <summary>
        /// Коструктор
        /// </summary>
        public EventsPageViewModel()
        {
            Analytics.TrackEvent("Конструктор модели страницы событий");
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
            Analytics.TrackEvent("Конструктор модели страницы событий-команда LoadData");
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
            Analytics.TrackEvent("Конструктор модели страницы событий-команда CountNew");
            MessagingCenter.Subscribe<Object>(this, "ReducePolls", sender =>
            {
                Analytics.TrackEvent("Конструктор модели страницы событий-ReducePolls");

                PollsCount--;
            });

            MessagingCenter.Subscribe<Object>(this, "ReduceAnnounsements", sender =>
            {
                Analytics.TrackEvent("Конструктор модели страницы событий-ReduceAnnounsements");
                AnnounsmentsCount--;
            });

            MessagingCenter.Subscribe<Object>(this, "ReduceNews", sender =>
            {
                Analytics.TrackEvent("Конструктор модели страницы событий-ReduceNews");

                NewsCount--;
            });

            Analytics.TrackEvent("Конструктор модели страницы событий-отработал");

        }
    }
}