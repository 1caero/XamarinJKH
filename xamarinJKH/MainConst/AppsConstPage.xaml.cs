using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

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
        public ObservableCollection<RequestInfo> RequestInfos { get; set; } = new ObservableCollection<RequestInfo>();
        public ObservableCollection<RequestInfo> RequestInfosAlive { get; set; }
        public ObservableCollection<RequestInfo> RequestInfosClose { get; set; }
        private RequestList _requestList;
        private List<RequestInfo> RequestDefault;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        public Color hex { get; set; }
        
        public bool isDebug
        {
            get
            {
#if DEBUG 
                return true;
#endif
                return false;
            }
        }

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
        public Command Checked { get; set; }
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
            SetDisableCheck();
            CheckRequestInfos.Clear();
            getApps();
            IsVisibleFunction();
            // additionalList.ItemsSource = null;
            // additionalList.ItemsSource = RequestInfos;
            if (RequestInfos != null)
                if (IsPass)
                {
                    MessagingCenter.Send<Object, int>(this, "SetRequestsPassAmount", RequestInfos.Where(o => o.TypeID == Settings.MobileSettings.requestTypeForPassRequest || o.Name.ToLower().Contains("пропуск")).Count(x => !x.IsReaded));
                }
                else
                {
                    MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", RequestInfos.Count(x => !x.IsReaded));

                }
        }

        public bool CanComplete => Settings.Person.UserSettings.RightPerformRequest;
        public bool CanClose => Settings.Person.UserSettings.RightCloseRequest;

        private async void TechSend(object sender, EventArgs e)
        {
                

            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            string phone = Preferences.Get("techPhone", Settings.Person.Phone);
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
            {
                Settings.SetPhoneTech(phone);
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushModalAsync(new AppPage());
            }
            else
            {
                if (PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x is EnterPhoneDialog) == null)
                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }

        public bool CanDoMassOps = false;
        public bool IsPass = false;
        public AppsConstPage(bool isPass)
        {
            InitializeComponent();
            IsPass = isPass;
            
            CanDoMassOps = !Settings.MobileSettings.disableBulkRequestsClosing;
        
            
            BackStackLayout.SetAppThemeColor(BackgroundColorProperty,
                System.Drawing.Color.Transparent,
                System.Drawing.Color.FromArgb(200, System.Drawing.Color.Black));
            Resources["hexColor"] = (Color)Application.Current.Resources["MainColor"];
            Analytics.TrackEvent("Заявки сотрудника " + Settings.Person.Login);
            NavigationPage.SetHasNavigationBar(this, false);

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


            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend; 
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

            var buttonFilterTgr = new TapGestureRecognizer();
            buttonFilterTgr.Tapped += async (s, e) =>
            {
                if (PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x is AppFilterDialog) == null)
                {
                    await PopupNavigation.Instance.PushAsync(new AppFilterDialog(this));
                }
            };
            buttonFilter.GestureRecognizers.Add(buttonFilterTgr);

            SetText();
            // additionalList.BackgroundColor = Color.Transparent;
            // additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            MessagingCenter.Subscribe<Object>(this, "UpdateAppCons", (sender) => RefreshData());
            // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
            getApps();
            ChangeTheme = new Command(async () =>
            {
                SetAdminName();
            });
            MessagingCenter.Unsubscribe<Object>(this, "ChangeAdminApp");
            MessagingCenter.Subscribe<Object>(this, "ChangeAdminApp", (sender) => ChangeTheme.Execute(null));
           
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

     

        private void CheckDown(string args)
        {
            RequestInfo requestInfo = getRequestInfo(args);
            if (requestInfo != null)
            {
                requestInfo.IsCheked = false;
                CheckRequestInfos.Remove(requestInfo);
            }

            IsVisibleFunction();
        }

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAllAsync();
                return true;
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }

        private void CheckApp(string args)
        {
            RequestInfo requestInfo = getRequestInfo(args);
            if (requestInfo != null)
            {
                requestInfo.IsCheked = true;
                CheckRequestInfos.Add(requestInfo);
            }

            IsVisibleFunction();
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
            try
            {
                foreach (var each in RequestInfos)
                {
                    Device.BeginInvokeOnMainThread(async () =>each.IsCheked = false);
                }
            }
            catch (Exception ex)
            {
                // ignored
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
            if (IsPass)
            {
                LabelTitle.Text = AppResources.NavBarPassApp;
                LayoutFilter.IsVisible = false;
                Pancake.Margin = new Thickness(25, 60, 15, 0);
                bottomMenu.IsVisible = false;
            }
            else
            {
                MessagingCenter.Unsubscribe<Object, List<RequestInfo>>(this, "SetFilter");
                MessagingCenter.Subscribe<Object, List<RequestInfo>>(this, "SetFilter", (sender, kvp) => {
                    Device.BeginInvokeOnMainThread(() => {
                        RequestDefault = kvp;
                        SetReaded();
                    });
                });
            
                MessagingCenter.Unsubscribe<Object, List<RequestInfo>>(this, "RemooveFilter");
                MessagingCenter.Subscribe<Object>(this, "RemooveFilter", (sender) => {
                    Device.BeginInvokeOnMainThread(() => {
                        RequestDefault = _requestList.Requests;
                        SetReaded();
                    });
                });
            }
            SwitchApp.OnColor = hex;
            FrameBtnAdd.BackgroundColor = hex;
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            bottomMenu.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
        }


        async void getApps()
        {

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            _requestList = await _server.GetRequestsListConst();
            if (_requestList.Error == null)
            {
//#if DEBUG
//                Device.BeginInvokeOnMainThread(() =>
//                {
//                    Toast.Instance.Show<ToastDialog>(new { Title = AppResources.ErrorAppsInfo, Duration = 1700 });
//                });
//#endif

                RequestDefault = _requestList.Requests;
                SetReaded();
                Settings.UpdateKey = _requestList.UpdateKey;
                if (IsPass)
                {
                    MessagingCenter.Send<Object, int>(this, "SetRequestsPassAmount", _requestList.Requests.Where(o => o.TypeID == Settings.MobileSettings.requestTypeForPassRequest || o.Name.ToLower().Contains("пропуск")).Count(x => !x.IsReaded));
                }
                else
                {
                    MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", _requestList.Requests.Count(x => !x.IsReaded));
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.Instance.Show<ToastDialog>(new { Title = AppResources.ErrorAppsInfo, Duration = 1700 });
                });
            }
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
                if (RequestDefault == null)
                {
                    return;
                }

                // if(_requestList.Requests == null)
                // {
                //     return;
                // }

                if (SwitchApp.IsToggled)
                {
                    
                    // RequestInfos =
                    //     new ObservableCollection<RequestInfo>(_requestList.Requests);
                     Device.BeginInvokeOnMainThread(async () =>
                    {
                        //if (RequestDefault != null)
                        //{
                            RequestInfos.Clear();
                            foreach (var each in new ObservableCollection<RequestInfo>(RequestDefault)
                                .OrderBy(o => o._RequestTerm).ThenBy(o => o.IsReaded)
                                
                                )
                            {
                                RequestInfos.Add(each);
                            }
                        //}
                    });

                }
                else
                {
                    // RequestInfos =
                    //     new ObservableCollection<RequestInfo>(from i in _requestList.Requests
                    //         where !i.IsReaded
                    //         select i);
                    if (IsPass)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            RequestInfos.Clear();
                            foreach (var each in RequestDefault.Where(o => o.TypeID == Settings.MobileSettings.requestTypeForPassRequest || o.Name.ToLower().Contains("пропуск")).OrderBy(o => !o.IsReaded).ThenBy(o => o.ID).Reverse())
                            {
                                each.IsEnableMass = false;
                                RequestInfos.Add(each);
                            }
                        });
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            RequestInfos.Clear();
                            foreach (var each in RequestDefault.OrderBy(o => !o.IsReaded).ThenBy(o => o.ID).Reverse())
                            {
                                RequestInfos.Add(each);
                            }
                        });
                    }
                   
                  
                    
                }
                // BindingContext = this;
                // additionalList.ItemsSource = null;
                // additionalList.ItemsSource = RequestInfos.OrderBy(o => o.ID).Reverse();
            }
            catch (Exception ex)
            {
            }

            try
            {
                Device.BeginInvokeOnMainThread(async () =>Empty = RequestInfos.Count == 0);
            }
            catch
            {
                Device.BeginInvokeOnMainThread(async () => Empty = RequestInfos == null);
            }
            this.BindingContext = this;

        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var s = (StackLayout)sender;
            var id = Convert.ToInt32(((Label)s.Children[0]).Text);
            RequestInfo select = RequestInfos.First(_=>_.ID==id);
            if (select != null)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppConstPage) == null)
                {
                    await Navigation.PushAsync(new AppConstPage(select));
                    
                    try
                    {
                        StackLayout stackLayout2 = (StackLayout) ((PancakeView) s.LogicalChildren[1]).Content;
                        Grid grid = (Grid) stackLayout2.LogicalChildren[0];
                        grid.Children[1].IsVisible = false;
                        
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        if (IsPass)
                        {
                            MessagingCenter.Send<Object, int>(this, "SetRequestsPassAmount", -1);
                        }
                        MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", -1);
                        
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    
                    
                }
            }
        }

        private void CheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                CheckBox checkBox = (CheckBox) sender;
                if (checkBox != null)
                {
                    RequestInfo r = (RequestInfo) checkBox.BindingContext;
                    if (checkBox.IsChecked)
                    {
                        CheckApp(r?.RequestNumber);
                    }
                    else
                    {
                        CheckDown(r?.RequestNumber);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            RequestInfos.Clear();
                foreach (var each in RequestDefault.Where(_ => _.RequestNumber.Contains(e.NewTextValue) || _.Status.ToLowerInvariant().Contains(e.NewTextValue.ToLowerInvariant()) || _.Name.ToLowerInvariant().Contains(e.NewTextValue.ToLowerInvariant())))
                {
                    
                    Device.BeginInvokeOnMainThread((() =>
                    {
                    RequestInfos.Add(each);
                    }));
                }
        }
        
        void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (e.VerticalDelta > 0)
            {
                Device.BeginInvokeOnMainThread((() =>
                {
                    double scrollY = MainScrollView.ScrollY;
                    MainScrollView.ScrollToAsync(0, scrollY + e.VerticalOffset, false);
                }));
            }
        }
    }
}