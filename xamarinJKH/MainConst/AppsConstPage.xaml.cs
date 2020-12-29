using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.Server;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;
using Xamarin.Essentials;
using System.Runtime.Serialization;

namespace xamarinJKH.MainConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppsConstPage : ContentPage
    {
        bool empty;
        public bool Empty
        {
            get => empty;
            set
            {
                empty = value;
                OnPropertyChanged("Empty");

                if (Device.RuntimePlatform == Device.iOS)
                    if (empty)
                    {
                        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = -1);
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() => additionalList.HeightRequest = 3000);
                    }

            }
        }
        public ObservableCollection<RequestInfo> RequestInfos { get; set; }
        public ObservableCollection<RequestInfo> RequestInfosAlive { get; set; }
        public ObservableCollection<RequestInfo> RequestInfosClose { get; set; }
        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        public Color hex { get; set; }
        
        private HashSet<RequestInfo> CheckRequestInfos = new HashSet<RequestInfo>();
        
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
        
        
        public async Task RefreshData()
        {
            if (RequestInfos == null)
            {
                RequestInfos = new ObservableCollection<RequestInfo>();
            }
            SetDisableCheck();
            CheckRequestInfos.Clear();
            getApps();
            IsVisibleFunction();
            additionalList.ItemsSource = null;
            additionalList.ItemsSource = RequestInfos;
            MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", RequestInfos.Where(x => !x.IsReaded).Count());
            Empty = RequestInfos.Count == 0;
        }

        public bool CanComplete => Settings.Person.UserSettings.RightPerformRequest;
        public bool CanClose => Settings.Person.UserSettings.RightCloseRequest;

        private async void TechSend(object sender, EventArgs e)
        {

            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(Settings.Person.Phone))
            {
                await Navigation.PushModalAsync(new Tech.AppPage());
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }

        public AppsConstPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Заявки сотрудника " + Settings.Person.Login);
            NavigationPage.SetHasNavigationBar(this, false);
            this.BindingContext = this;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    additionalList.HeightRequest = 3000;

                    break;
                default:
                    break;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //BackgroundColor = Color.White;
                    // ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    break;
                case Device.Android:
                    // double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -125);
                    //     BackStackLayout.Margin = new Thickness(5, 25, 0, 0);
                    // }

                    break;
                default:
                    break;
            }

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend; // async(s, e) => {     await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            
            var addClick = new TapGestureRecognizer();
            addClick.Tapped += async (s, e) => { startNewApp(FrameBtnAdd, null); };
            FrameBtnAdd.GestureRecognizers.Add(addClick);
            hex = (Color) Application.Current.Resources["MainColor"];
            var performApps = new TapGestureRecognizer();
            performApps.Tapped += async (s, e) => { await PerformsApps(); };
            StackLayoutExecute.GestureRecognizers.Add(performApps);
            var closeApps = new TapGestureRecognizer();
            closeApps.Tapped += async (s, e) => { await CloseApps(); };
            StackLayoutClose.GestureRecognizers.Add(closeApps);
            SetText();
            additionalList.BackgroundColor = Color.Transparent;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            MessagingCenter.Subscribe<Object>(this, "UpdateAppCons", (sender) => RefreshData());
            // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
            getApps();
            ChangeTheme = new Command(async () =>
            {
                SetAdminName();
            });
            MessagingCenter.Subscribe<Object>(this, "ChangeAdminApp", (sender) => ChangeTheme.Execute(null));
            MessagingCenter.Subscribe<Object, string>(this, "ChechApp", async (sender, args) =>
            {
                RequestInfo requestInfo = getRequestInfo(args);
                if (requestInfo != null)
                {
                    requestInfo.IsCheked = true;
                    CheckRequestInfos.Add(requestInfo);
                }

                IsVisibleFunction();
            });  
            MessagingCenter.Subscribe<Object, string>(this, "ChechDownApp", async (sender, args) =>
            {
                RequestInfo requestInfo = getRequestInfo(args);
                if (requestInfo != null)
                {
                    requestInfo.IsCheked = false;
                    CheckRequestInfos.Remove(requestInfo);
                }

                IsVisibleFunction();
            });

            MessagingCenter.Subscribe<Object, int>(this, "OpenAppConst", async (sender, args) =>
            {
                while (RequestInfos == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
                try
                {
                    var request = RequestInfos.First(x => x.ID == args);
                    if (request != null)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is AppConstPage) == null)
                            await Navigation.PushAsync(new AppConstPage(request));
                    }
                }
                catch
                {

                }
                
            });

        }
        private async Task CloseApps()
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                CommonResult result = await _server.CloseList(CheckRequestInfos.Select(each => each.ID).ToList());
                if (result.Error == null)
                {
                    await RefreshData();
                    await DisplayAlert(AppResources.AlertSuccess, AppResources.AppsClosed, "OK");
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                }
                
            });
        }

        private async Task PerformsApps()
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                CommonResult result = await _server.PerformList(CheckRequestInfos.Select(each => each.ID).ToList());
                if (result.Error == null)
                {
                    await RefreshData();
                    await DisplayAlert(AppResources.AlertSuccess, AppResources.AppsPerformed, "OK");
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                }
            });
        }

        private RequestInfo getRequestInfo(string number)
        {
            foreach (var each in RequestInfos)
            {
                if (number.Equals(each.RequestNumber))
                {
                    return each;
                }
            }

            return null;
        }
        private void SetDisableCheck()
        {
            foreach (var each in RequestInfos)
            {
                each.IsCheked = false;
            }
        }
        private void IsVisibleFunction()
        {
            if (CheckRequestInfos.Count > 0)
            {
                StackLayoutFunction.IsVisible = true;
                StackLayoutBot.IsVisible = false;
            }
            else
            {
                StackLayoutFunction.IsVisible = false;
                StackLayoutBot.IsVisible = true;
            }
        }
        
        private void SetAdminName()
        {
            FormattedString formattedName = new FormattedString();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            formattedName.Spans.Add(new Span
            {
                Text = Settings.Person.FIO,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            });
            formattedName.Spans.Add(new Span
            {
                Text = AppResources.GoodDay,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.None,
                FontSize = 16
            });
        }

        public Command ChangeTheme { get; set; }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetReaded();

            if (bottomMenu.VerticalOptions.Alignment != LayoutAlignment.End)
                Device.BeginInvokeOnMainThread(() => { bottomMenu.VerticalOptions = LayoutOptions.End; });
            // new Task(SyncSetup).Start(); // This could be an await'd task if need be
        }

        async void SyncSetup()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
                RefreshData();

            });
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            SetAdminName();
            SwitchApp.OnColor = hex;
            FrameBtnAdd.BackgroundColor = hex;
            //IconAddApp.Foreground = Color.White;
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            bottomMenu.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
        }


        async void getApps()
        {

            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            _requestList = await _server.GetRequestsListConst();
            if (_requestList.Error == null)
            {
                // setReaded(_requestList.Requests);
                SetReaded();
                Settings.UpdateKey = _requestList.UpdateKey;
                MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", _requestList.Requests.Where(x => !x.IsReaded).Count());
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsInfo, "OK");
            }
        }

        void setReaded(List<RequestInfo> infos)
        {
            RequestInfosAlive = new ObservableCollection<RequestInfo>();
            RequestInfosClose = new ObservableCollection<RequestInfo>();
            foreach (var each in infos)
            {
                if (each.IsReaded)
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

            if (RequestInfos != null)
            {
                BindingContext = this;
                additionalList.ItemsSource = null;
                additionalList.ItemsSource = RequestInfos;
            }
            Empty = RequestInfos.Count == 0;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            RequestInfo select = e.Item as RequestInfo;
            if (Navigation.NavigationStack.FirstOrDefault(x => x is AppConstPage) == null)
                await Navigation.PushAsync(new AppConstPage(select));

            await Task.Delay(TimeSpan.FromSeconds(1));

            MessagingCenter.Send<Object, int>(this, "SetAppReadConst", select.ID);
        }

        private async void startNewApp(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppConstPage) == null)
                await Navigation.PushAsync(new NewAppConstPage(this));
        }

        private async void change(object sender, PropertyChangedEventArgs e)
        {
            SetReaded();
        }

        private void SetReaded()
        {
            try
            {
                if (_requestList == null)
                {
                    return;
                }

                if (SwitchApp.IsToggled)
                {
                    RequestInfos =
                        new ObservableCollection<RequestInfo>(from i in _requestList.Requests
                            where i.IsReaded
                            select i);
                }
                else
                {
                    RequestInfos =
                        new ObservableCollection<RequestInfo>(
                            from i in _requestList.Requests where !i.IsReaded select i);
                }

                BindingContext = this;
                additionalList.ItemsSource = null;
                additionalList.ItemsSource = RequestInfos.OrderBy(o => o.ID).Reverse();
            }
            catch (Exception ex)
            {
            }

            try
            {
                Empty = RequestInfos.Count == 0;
            }
            catch
            {
                Empty = RequestInfos == null;
            }
        }
    }
}