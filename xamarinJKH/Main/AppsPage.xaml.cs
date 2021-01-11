using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Messaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Apps;
using xamarinJKH.Server;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using System.Threading;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.PancakeView;
using xamarinJKH.DialogViews;
using xamarinJKH.ViewModels.Main;
using Xamarin.Essentials;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Microsoft.AppCenter.Analytics;
using xamarinJKH.Pays;
using AppPage = xamarinJKH.Apps.AppPage;
using System.Runtime.Serialization;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppsPage : ContentPage
    {
        public List<RequestInfo> RequestInfos { get; set; }
        public List<RequestInfo> RequestInfosAlive { get; set; }
        public List<RequestInfo> RequestInfosClose { get; set; }
        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        public Color hex { get; set; }
        public AppsPageViewModel viewModel { get; set; }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand2
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

        Task UpdateTask;

        public async Task ShowMessage(string message,
            string title,
            string buttonText,
            Action afterHideCallback)
        {
            await DisplayAlert(
                title,
                message,
                buttonText);

            afterHideCallback?.Invoke();
        }

        bool showNoInetWindow = true;

        void StartAutoUpdate()
        {
            CheckAkk();
            CancellationTokenSource = new CancellationTokenSource();
            this.CancellationToken = CancellationTokenSource.Token;
            UpdateTask = null;
            UpdateTask = new Task(async () =>
            {
                // while (!this.CancellationToken.IsCancellationRequested)
                // {
                    if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (showNoInetWindow)
                            {
                                showNoInetWindow = false;

                                await ShowMessage(AppResources.ErrorNoInternet, AppResources.ErrorTitle, "OK", () =>
                                {
                                    showNoInetWindow = true;
                                    //await ShowMessage("OK was pressed", "Message", "OK", null);
                                });
                            }
                            //await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK");

                            await Task.Delay(TimeSpan.FromSeconds(5));
                        });
                    }

                    await viewModel.UpdateTask();
                    //if (Device.RuntimePlatform == Device.iOS)
                    //    if (viewModel.Empty)
                    //    {
                    //        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = -1);
                    //    }
                    //    else
                    //    {
                    //        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = 3000);
                    //    }
                    // await Task.Delay(TimeSpan.FromSeconds(5));
                // }

                return;
            });
            try
            {
                UpdateTask.Start();
            }
            catch
            {
            }
        }

        private void CheckAkk()
        {
            if (Settings.Person.Accounts != null)
            if (Settings.Person.Accounts.Count > 0)
            {
                StackLayoutNewApp.IsVisible = true;
                StackLayoutIdent.IsVisible = false;
            }
            else
            {
                StackLayoutNewApp.IsVisible = false;
                StackLayoutIdent.IsVisible = true;
            }
        }

        static bool inUpdateNow = false;

        public async Task RefreshData()
        {
            try
            {
                if (inUpdateNow)
                    return;
                inUpdateNow = true;

                Device.BeginInvokeOnMainThread(() =>
                {
                    aIndicator.IsVisible = true;
                });

                await getAppsAsync();
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    additionalList.ItemsSource = null;
                //    additionalList.ItemsSource = RequestInfos;
                //});

                inUpdateNow = false;
            }
            catch (Exception e)
            {
                inUpdateNow = false;
            }
            finally
            {
                inUpdateNow = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    aIndicator.IsVisible = false;
                });
            }
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public AppsPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Заявки жителя");
            //Settings.MobileSettings.color = null;
            hex = Color.FromHex(!string.IsNullOrEmpty(Settings.MobileSettings.color)
                ? Settings.MobileSettings.color
                : "#FF0000");
            Analytics.TrackEvent("Заявки жителя - определили основной цвет");
            BindingContext = viewModel = new AppsPageViewModel();
            Analytics.TrackEvent("Заявки жителя - создали/прибиндили модель");
            aIndicator.Color = hex;

            NavigationPage.SetHasNavigationBar(this, false);
            MessagingCenter.Subscribe<Object>(this, "AutoUpdate", (sender) => { StartAutoUpdate(); });
            Analytics.TrackEvent("Заявки жителя - подписались на автообновление");

            MessagingCenter.Subscribe<Object>(this, "StartRefresh", (sender) => 
            { Device.BeginInvokeOnMainThread(() => aIndicator.IsVisible = true); });
            Analytics.TrackEvent("Заявки жителя-подписались на обновление");

            MessagingCenter.Subscribe<Object>(this, "EndRefresh", (sender) => 
            { Device.BeginInvokeOnMainThread(() => aIndicator.IsVisible = false); });
            Analytics.TrackEvent("Заявки жителя-подписались на окончание обновления");

            var goAddIdent = new TapGestureRecognizer();
            goAddIdent.Tapped += async (s, e) =>
            {
                /*await Dialog.Instance.ShowAsync<AddAccountDialogView>();*/
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddIdent) == null)
                    await Navigation.PushAsync(new AddIdent((PaysPage) Settings.mainPage));
            };
            StackLayoutIdent.GestureRecognizers.Add(goAddIdent);
            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа добавления счета");


            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    //хак чтобы список растягивался на все необходимое пространоство. а так
                    //есть баг в xamarin, потому что fillAndExpand не работает(https://github.com/xamarin/Xamarin.Forms/issues/6908)
                    additionalList.HeightRequest = 3000;

                    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 700)
                        LabelSwitch.FontSize = 12;

                    FrameBtnAdd.IsVisible = false;
                    FrameBtnAddIos.IsVisible = true;

                    break;
                case Device.Android:
                    FrameBtnAdd.IsVisible = true;
                    FrameBtnAddIos.IsVisible = false;

                    break;
                default:
                    break;
            }

            Analytics.TrackEvent("Заявки жителя-платформозависимый код выполнен");

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа Профиля");

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new Tech.AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            Analytics.TrackEvent("Заявки жителя-добавили обработку тапа Техподдержки");

            //var call = new TapGestureRecognizer();
            //call.Tapped += async (s, e) =>
            //{
            //    if (Settings.Person.Phone != null)
            //    {
            //        IPhoneCallTask phoneDialer;
            //        phoneDialer = CrossMessaging.Current.PhoneDialer;
            //        if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
            //            phoneDialer.MakePhoneCall(
            //                System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
            //    }
            //};

            var addClick = new TapGestureRecognizer();
            addClick.Tapped += async (s, e) => { startNewApp(FrameBtnAdd, null); };
            FrameBtnAdd.GestureRecognizers.Add(addClick);
            Analytics.TrackEvent("Заявки жителя-добавление обработки тапа кнопки Новая заявка андроид");
            var addClickIOS = new TapGestureRecognizer();
            addClickIOS.Tapped += async (s, e) => { startNewApp(FrameBtnAddIos, null); };
            FrameBtnAddIos.GestureRecognizers.Add(addClickIOS);
            Analytics.TrackEvent("Заявки жителя-добавление обработки тапа кнопки Новая заявка иос");

            SetText();
            Analytics.TrackEvent("Заявки жителя-SetText выполнено");
            //getApps();
            additionalList.BackgroundColor = Color.Transparent;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            this.CancellationTokenSource = new CancellationTokenSource();
            MessagingCenter.Subscribe<Object>(this, "UpdateAppCons", (sender) => RefreshData());
            Analytics.TrackEvent("Заявки жителя-UpdateAppCons подписались");
            MessagingCenter.Subscribe<Object, int>(this, "CloseAPP", async (sender, args) =>
            {
                await RefreshData();
                var request = RequestInfos?.Find(x => x.ID == args);
                if (request != null)
                {
                    try
                    {
                        CancellationTokenSource.Cancel();
                        CancellationTokenSource.Dispose();
                    }
                    catch
                    {
                    }

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                            await Navigation.PushAsync(new AppPage(request));
                    });
                }
            });
            Analytics.TrackEvent("Заявки жителя-CloseAPP подписались");
            viewModel.LoadRequests.Execute(null);
            Analytics.TrackEvent("Заявки жителя-LoadRequests");
            //            PropertyChanged = "change"

            SwitchApp.Toggled += SwitchApp_Toggled;
            Analytics.TrackEvent("Заявки жителя-SwitchApp.Toggled подписались");

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

                //IconViewTech.ReplaceStringMap = colors;
            });
            Analytics.TrackEvent("Заявки жителя-ChangeThemeCounter подписались");

            MessagingCenter.Subscribe<Object, int>(this, "OpenApp", async (sender, index) =>
            {
                await viewModel.UpdateTask();
                //if (Device.RuntimePlatform == Device.iOS)
                //    if (viewModel.Empty)
                //    {
                //        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = -1);
                //    }
                //    else
                //    {
                //        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = 3000);
                //    }
                while (viewModel.AllRequests == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
                var request = viewModel.AllRequests.Where(x => x.ID == index).ToList();
                if (request != null && request.Count > 0)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                            await Navigation.PushAsync(new AppPage(request[0],false, request[0].IsPaid));
                    });
                }
            });

            Analytics.TrackEvent("Заявки жителя-OpenApp подписались");

            MessagingCenter.Subscribe<Object, string>(this, "RemoveIdent", (sender, args) =>
           {
               if (args == null)
               {
                   viewModel.Requests.Clear();
               }
           });
            Analytics.TrackEvent("Заявки жителя-RemoveIdent подписались");
        }

        private void SwitchApp_Toggled(object sender, ToggledEventArgs e)
        {
            if (SwitchApp.IsToggled)
            {
                RequestInfos = RequestInfosClose;
            }
            else
            {
                RequestInfos = RequestInfosAlive;
            }

            additionalList.ItemsSource = null;
            additionalList.ItemsSource = RequestInfos;
        }

        public AppsPage(string app_id) : base()
        {
            var request = RequestInfos.Find(x => x.ID == int.Parse(app_id));
            if (request != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                        await Navigation.PushAsync(new AppPage(request));
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
            }
            catch
            {
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var colors = new Dictionary<string, string>();
            var buttonColor = new Dictionary<string, string>();
            if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
            {
                colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                buttonColor.Add("#000000", "#FFFFFF");
            }
            else
            {
                colors.Add("#000000", "#FFFFFF");
                buttonColor.Add("#000000", "#FFFFFF");
            }

            //IconViewTech.ReplaceStringMap = colors;
            IconViewSaldos.ReplaceStringMap = buttonColor;
            CheckAkk();
            
        }

        async void SyncSetup()
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
            // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await RefreshData();
            }

            //});
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            

            SwitchApp.OnColor = hex;
            //IconAddApp.Foreground = Color.White;
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
          
            GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
        }

        async Task getAppsAsync()
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            _requestList = await _server.GetRequestsList();
            if (_requestList.Error == null)
            {
                if (Settings.UpdateKey != _requestList.UpdateKey)
                {
                    RequestInfos = null;
                    setCloses(_requestList.Requests);
                    Settings.UpdateKey = _requestList.UpdateKey;
                    this.BindingContext = this;
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsInfo, "OK");
            }
        }

        async void getApps()
        {
            _requestList = await _server.GetRequestsList();
            if (_requestList.Error == null)
            {
                setCloses(_requestList.Requests);
                Settings.UpdateKey = _requestList.UpdateKey;
                this.BindingContext = this;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsInfo, "OK");
            }
        }

        void setCloses(List<RequestInfo> infos)
        {
            RequestInfosAlive = new List<RequestInfo>();
            RequestInfosClose = new List<RequestInfo>();
            foreach (var each in infos)
            {
                if (each.IsClosed)
                {
                    RequestInfosClose.Add(each);
                }
                else
                {
                    RequestInfosAlive.Add(each);
                }
            }

            if (SwitchApp.IsToggled)
            {
                RequestInfos = RequestInfosClose;
            }
            else
            {
                RequestInfos = RequestInfosAlive;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                additionalList.ItemsSource = null;
                additionalList.ItemsSource = RequestInfos;
            });
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            RequestInfo select = e.Item as RequestInfo;
            if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                await Navigation.PushAsync(new AppPage(select));
        }

        private async void startNewApp(object sender, EventArgs e)
        {
            try
            {
                if (Settings.Person.Accounts.Count > 0)
                {
                    if (Settings.TypeApp.Count > 0)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppPage) == null)
                            await Navigation.PushAsync(new NewAppPage());
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoTypes, "OK");
                    }
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoIdent, "OK");
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent(ex.Message);
            }
        }

        private void change(object sender, PropertyChangedEventArgs e)
        {
            if (SwitchApp.IsToggled)
            {
                RequestInfos = RequestInfosClose;
            }
            else
            {
                RequestInfos = RequestInfosAlive;
            }

            additionalList.ItemsSource = null;
            additionalList.ItemsSource = RequestInfos;
        }
    }
}