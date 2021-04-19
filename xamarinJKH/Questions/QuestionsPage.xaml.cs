using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
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

namespace xamarinJKH.Questions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionsPage : ContentPage
    {
        public List<PollInfo> Quest { get; set; }
        public List<PollInfo> QuestComlite { get; set; }
        public List<PollInfo> QuestNotComlite { get; set; }
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();

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
            try
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    return;
                }
            }
            catch
            {
                return;
            }
            if (Settings.EventBlockData.Error == null)
            {
                Settings.EventBlockData = await server.GetEventBlockData();
                isComplite();
                if (!SwitchQuest.IsToggled)
                {
                    Quest = QuestNotComlite;
                }
                else
                {
                    Quest = QuestComlite;
                }

                additionalList.ItemsSource = null;
                additionalList.ItemsSource = Quest;
                this.BindingContext = this;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorOSSMainInfo, "OK");
            }
        }

        public QuestionsPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Список опросов");

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {   await Navigation.PushAsync(new AppPage()); };
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
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        labelShowClosed.FontSize = 14;
                        
                    }
                    

                    break;
                default:
                    break;
            }

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
            
           
            additionalList.BackgroundColor = Color.Transparent;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            this.BindingContext = this;
        }
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

            new Task(SyncSetup).Start(); // This could be an await'd task if need be
            IsChangeTheme = !IsChangeTheme;
        }

        async void SyncSetup()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                    return;
                }
                if (IsRefreshing)
                    return;

                IsRefreshing = true;
                // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
                await RefreshData();
                IsRefreshing = false;
            });
        }

        void isComplite()
        {
            QuestComlite = new List<PollInfo>();
            QuestNotComlite = new List<PollInfo>();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            if (Settings.EventBlockData.Polls != null)
                foreach (var each in Settings.EventBlockData.Polls)
                {
                    bool flag = true;
                    foreach (var quest in each.Questions)
                    {
                        if (!quest.IsCompleteByUser)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        each.IsComplite = true;
                        QuestComlite.Add(each);
                    }
                    else
                    {
                        each.IsComplite = false;
                        QuestNotComlite.Add(each);
                    }
                }
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            
            SwitchQuest.OnColor = (Color)Application.Current.Resources["MainColor"];
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            // GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            additionalList.RefreshControlColor = hexColor;

        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            PollInfo select = e.Item as PollInfo;
            if (Navigation.NavigationStack.FirstOrDefault(x => x is PollsPage) == null)
                await Navigation.PushAsync(new PollsPage(select, SwitchQuest.IsToggled));
        }

        private void chage(object sender, PropertyChangedEventArgs e)
        {
            if (!SwitchQuest.IsToggled)
            {
                Quest = QuestNotComlite;
            }
            else
            {
                Quest = QuestComlite;
            }

            additionalList.ItemsSource = null;
            additionalList.ItemsSource = Quest;
        }
    }
}