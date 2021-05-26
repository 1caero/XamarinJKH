using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Realms;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.DataModel;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.Utils.ReqiestUtils;

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
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (additionalList.HeightRequest != 3000)
                                additionalList.HeightRequest = 3000;
                        });
                    }

            }
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
        public ObservableCollection<RequestInfo> RequestInfos { get; set; } = new ObservableCollection<RequestInfo>();
   
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
            RequestUtils.UpdateRequestCons();
            getApps();
            IsVisibleFunction();
        
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
        private readonly Realm _realm;
        public AppsConstPage(bool isPass)
        {
            InitializeComponent();
            IsPass = isPass;
            _realm = Realm.GetInstance();
            CanDoMassOps = !Settings.MobileSettings.disableBulkRequestsClosing;
            if (Device.RuntimePlatform == Device.Android)
            {
                OrdersStack.HeightRequest = 1000;
            }

            
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
                    //Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    iosPanel.HeightRequest = statusBarHeight;
                    additionalList.HeightRequest = 3000;
                    //commGrid1.MinimumHeightRequest = ImageFon.Height;

                    //OrdersStack.Margin = new Thickness(OrdersStack.Margin.Left, OrdersStack.Margin.Top, OrdersStack.Margin.Right, OrdersStack.Margin.Bottom - 30);
                    //OrdersStack.Margin =  DeviceDisplay.MainDisplayInfo.Width < 700 ?  new Thickness(0, -80, 0, 0) : new Thickness(0, -160, 0, 0);


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
            var hideBot = new TapGestureRecognizer();
            hideBot.Tapped += async (s, e) => { await HideBot(); };
            StackLayoutArrow.GestureRecognizers.Add(hideBot);

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
            
            MessagingCenter.Unsubscribe<Object>(this, "UpdateAppCons");
            MessagingCenter.Subscribe<Object>(this, "UpdateAppCons", async (sender) =>
            {
                long seconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (seconds - Settings.timeLoadReq > 1000)
                {
                    await RefreshData();
                    Settings.timeLoadReq = seconds;
                }
            });
            // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
            // getApps();
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
                        {
                            List<RequestInfo> req = new List<RequestInfo>(additionalList.ItemsSource as IEnumerable<RequestInfo> ?? Array.Empty<RequestInfo>());
                            var index = req.IndexOf(request);
                            Preferences.Set("scrollY",index);
                            await Navigation.PushAsync(new AppConstPage(request));
                        }
                    }
                }
                catch
                {

                }
                
            });

            this.BindingContext = this;
            //isNeedShow = true;// StackLayoutHide.IsVisible;

           //Device.StartTimer(TimeSpan.FromMilliseconds(200), ShowBotTimer);

            Device.StartTimer(TimeSpan.FromMilliseconds(100), SetCanHideTrueAsync);
        }

        bool SetCanHideTrueAsync()
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Task.Delay(1000); 
                canHide = true; 
            } );
            
            return false;
        }

        static bool canHide = false;

        private int rotation = 1;
        private int rotation2 = 1;
        private async Task HideBot()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                additionalList.Scrolled -= OnCollectionViewScrolled;
                StackLayoutHide.IsVisible = !StackLayoutHide.IsVisible;
                await ImageHide.RotateTo(ImageHide.Rotation + 180 * rotation, 500, Easing.Linear);
                rotation *= -1;

                canHide = true;
                additionalList.Scrolled += OnCollectionViewScrolled;
            });
            //isNeedHide = StackLayoutHide.IsVisible;
            //hideBotClikced = true;
        }

        //bool hideBotClikced = false;

        private async Task HideBotTimer()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                additionalList.Scrolled -= OnCollectionViewScrolled;
            StackLayoutHide.IsVisible = !StackLayoutHide.IsVisible;
            //await ImageHide.RotateTo(ImageHide.Rotation + 180 * rotation2);
            rotation2 *= -1;
            additionalList.Scrolled += OnCollectionViewScrolled;
                //isNeedShow = StackLayoutHide.IsVisible;
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

        protected override async void OnAppearing()
        {
            if (IsPass)
            {

            }
                base.OnAppearing();

            if (Device.RuntimePlatform == Device.Android)
                await Task.Delay(500);

            var delta =  OrdersStack.Y - ukNameStack.Y;
            if (delta != 40)                
              Device.BeginInvokeOnMainThread(() => { OrdersStack.Margin = new Thickness(OrdersStack.Margin.Left, OrdersStack.Margin.Top - delta +40, OrdersStack.Margin.Right, OrdersStack.Margin.Bottom); });
            
            getApps();
                
            if (bottomMenu.VerticalOptions.Alignment != LayoutAlignment.End)
                Device.BeginInvokeOnMainThread(() => { bottomMenu.VerticalOptions = LayoutOptions.End; });
            IsChangeTheme = !IsChangeTheme;
            int y = Preferences.Get("scrollY", 0);
            additionalList.ScrollTo(y, animate:false);
            Preferences.Remove("scrollY");
            
        }
        

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            SetAdminName();
            if (IsPass)
            {
                LabelTitle.Text = AppResources.NavBarPassApp;
                LayoutFilter.IsVisible = false;
                LayoutFilterPass.IsVisible = true;
                //Pancake.Margin = new Thickness(25, 60, 15, 0);
                bottomMenu.IsVisible = false;
                
                var filterPas = new TapGestureRecognizer();
                filterPas.Tapped += async (s, e) =>
                {
                    var action = await DisplayActionSheet(AppResources.TypePass, AppResources.Cancel, null,
                        AppResources.All, AppResources.ConstantPass, AppResources.OneOffPass);
                    if (action != null && !action.Equals(AppResources.Cancel))
                    {
                        if (RequestDefault != null)
                        {
                            if (action.Equals(AppResources.All) && !action.Equals(LabelKind.Text))
                            {
                                RequestInfos.Clear();
                                foreach (var each in RequestDefault.Where(_ => _.HasPass ))
                                {
                                    Device.BeginInvokeOnMainThread((() =>
                                    {
                                        RequestInfos.Add(each);
                                    }));
                                }
                            }else if (action.Equals(AppResources.ConstantPass) && !action.Equals(LabelKind.Text))
                            {
                              
                                RequestInfos.Clear();
                                foreach (var each in RequestDefault.Where(_ => _.PassIsConstant && _.HasPass ))
                                {
                                    Device.BeginInvokeOnMainThread((() =>
                                    {
                                        RequestInfos.Add(each);
                                    }));
                                }
                            }else if (action.Equals(AppResources.OneOffPass) && !action.Equals(LabelKind.Text))
                            {
                                RequestInfos.Clear();
                                foreach (var each in RequestDefault.Where(_ => !_.PassIsConstant  && _.HasPass ))
                                {
                                    Device.BeginInvokeOnMainThread((() =>
                                    {
                                        RequestInfos.Add(each);
                                    }));
                                }
                            }
                            LabelKind.Text = action;

                        }

                    }
                };
                LayoutFilterPass.GestureRecognizers.Add(filterPas);
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
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        RequestDefault = new List<RequestInfoDao>(_realm.All<RequestInfoDao>()).ConvertAll(
                            new Converter<RequestInfoDao, RequestInfo>(RequestInfo.DaoToInfo));
                        SetReaded();
                    });
                });
            }
            SwitchApp.OnColor = hex;
            FrameBtnAdd.BackgroundColor = hex;
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            // bottomMenu.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
        }


        async void getApps()
        {
            var requestInfoDaos = _realm.All<RequestInfoDao>();

            if (requestInfoDaos.Any())
            {


                RequestDefault =new List<RequestInfoDao>( requestInfoDaos).ConvertAll(
                    new Converter<RequestInfoDao, RequestInfo>(RequestInfo.DaoToInfo));
                
                bool switchRead = Preferences.Get("SwitchAppRead",false);
                bool switchApp = Preferences.Get("SwitchApp",false);
                bool switchHide = Preferences.Get("SwitchAppHidePerfom",false);
                
                SwitchApp.IsToggled = switchApp;
                SwitchAppRead.IsToggled = switchRead;
                SwitchAppHidePerfom.IsToggled = switchHide;
                
                SetReaded();
                Device.StartTimer(new TimeSpan(0, 0, 1), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (IsPass)
                        {
                            MessagingCenter.Send<Object, int>(this, "SetRequestsPassAmount", requestInfoDaos.Where(x => x.HasPass).Count(x => !x.IsReaded));
                        }
                        else
                        {
                            MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", requestInfoDaos.Where(x => !x.HasPass).Count(x => !x.IsReaded));
                        }
                    });
                    return false; // runs again, or false to stop
                });
             
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.Instance.Show<ToastDialog>(new { Title = AppResources.ErrorAppsInfo, Duration = 1700, ColorB = Color.Gray,  ColorT = Color.White });
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
            if (Device.RuntimePlatform == Device.Android)
            {
                var action = await Application.Current.MainPage.DisplayActionSheet(AppResources.AddApp,
                    AppResources.Cancel, null,
                    AppResources.NewApplication,
                    AppResources.NewDocument);
                if (action.Equals(AppResources.NewApplication))
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppConstPage) == null)
                        await Navigation.PushAsync(new NewAppConstPage(this));
                }
                else
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is NewDocumentConstPage) == null)
                        await Navigation.PushAsync(new NewDocumentConstPage());
                }
            }
            else
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppConstPage) == null)
                    await Navigation.PushAsync(new NewAppConstPage(this));
            }
        }

        public bool IndicatorRun { get; set; }
        private List<int> performed = new List<int>
        {
            5,6,7,8,10,11,12
        }; 
        private void SetReaded()
        {
            Device.BeginInvokeOnMainThread(() => aInd.IsVisible = true);
            try
            {
                var d = aInd.IsEnabled;
                IndicatorRun = true;
                if (RequestDefault == null)
                {
                    return;
                }
                ObservableCollection<RequestInfo> setApps = new ObservableCollection<RequestInfo>();
                List<RequestInfo> dopList = new List<RequestInfo>(RequestDefault);
                if (SwitchAppHidePerfom.IsToggled)
                {
                    dopList = new List<RequestInfo>(RequestDefault.Where(x => !performed.Contains(x.StatusID)));
                }
                
                if (IsPass)
                {
                    setApps = new ObservableCollection<RequestInfo>(RequestDefault.Where(o => o.HasPass)
                        .OrderBy(o => !o.IsReaded).ThenBy(o => o.ID).Reverse().ToList());
                }
                else
                if (SwitchAppRead.IsToggled && SwitchApp.IsToggled)
                {
                    var readed = dopList.Where(o => !o.IsReaded).OrderByDescending(o => o.ID);
                    var requestTerm = dopList.Where(o => o.RequestTerm != null && o.IsReaded)
                        .OrderBy(o => o._RequestTerm);
                    var enother = dopList.Where(o => o.IsReaded && o.RequestTerm == null);

                    setApps = new ObservableCollection<RequestInfo>(readed.Concat(requestTerm).Concat(enother));

                }
                else if (SwitchAppRead.IsToggled && !SwitchApp.IsToggled)
                {
                    setApps = new ObservableCollection<RequestInfo>(dopList.Where(o => !o.HasPass)
                        .OrderBy(o => !o.IsReaded).ThenBy(o => o.ID).Reverse().ToList());
                }
                else 
                if (!SwitchAppRead.IsToggled && SwitchApp.IsToggled)
                {
                    setApps = new ObservableCollection<RequestInfo>(dopList.OrderBy(o => o._RequestTerm));
                }
                else
                {
                    //if (IsPass)
                    //{
                    //    setApps = new ObservableCollection<RequestInfo>(RequestDefault.Where(o => o.HasPass)
                    //        .OrderBy(o => !o.IsReaded).ThenBy(o => o.ID).Reverse().ToList());
                    //}
                    //else
                    {
                        setApps = new ObservableCollection<RequestInfo>(dopList.OrderBy(o => o.ID).Reverse());
                    }
                }
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    if (setApps != null)
                //    {
                //        RequestInfos.Clear();
                //        foreach (var each in setApps)
                //        {
                //            RequestInfos.Add(each);
                //        }
                //    }
                //});

                if (setApps != null)
                {
                    //RequestInfos.Clear();
                    additionalList.ItemsSource = setApps;
                    //RequestInfos = new ObservableCollection<RequestInfo>(setApps);
                    //foreach (var each in setApps)
                    //{
                    //    RequestInfos.Add(each);
                    //}
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
                Device.BeginInvokeOnMainThread(() => {
                    try
                    {
                        Empty = RequestInfos.Count == 0;
                    }
                    catch(Exception e)
                    {
                        throw;
                    }
                    
                    });
            }
            catch(Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => Empty = RequestInfos == null);
            }
            IndicatorRun = false;
            //this.BindingContext = this;
            Device.BeginInvokeOnMainThread(() => aInd.IsVisible = false);
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var s = (StackLayout)sender;
            var id = Convert.ToInt32(((Label)s.Children[0]).Text);
            RequestInfo select = RequestInfos.FirstOrDefault(_=>_.ID==id);
            if (select != null)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppConstPage) == null)
                {
                    await Navigation.PushAsync(new AppConstPage(select));
                    List<RequestInfo> req = new List<RequestInfo>(additionalList.ItemsSource as IEnumerable<RequestInfo> ?? Array.Empty<RequestInfo>());
                    var index = req.IndexOf(select);
                    Preferences.Set("scrollY",index);
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
            if (RequestDefault != null)
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
            
        }
        bool alive = false;

        private  bool ShowBotTimer()
        {
            if (canHide)
            {
                if (currentPos != lastPos)
                {
                    if (StackLayoutHide.IsVisible)
                    {
                        //isNeedShow = true;
                        Device.BeginInvokeOnMainThread(async () => await HideBotTimer());
                    }
                     alive = true;
                }
                else
                {
                     alive = false;

                    if (/*isNeedShow &&*/ !StackLayoutHide.IsVisible)
                    {
                        //hideBotClikced = false;
                        //isNeedShow = false;
                        canHide = false;
                        Device.StartTimer(TimeSpan.FromMilliseconds(200), SetCanHideTrueAsync);
                        Device.BeginInvokeOnMainThread(async () => await HideBotTimer());
                    }
                }
                currentPos = lastPos;

            }
            return alive;
        }

        
        //bool isNeedShow = false;

        static double lastPos = 0;
        static double currentPos = 0;

        
        //private double marginTopDefault = 0;
        void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if(canHide && StackLayoutHide.IsVisible)
            Device.StartTimer(TimeSpan.FromMilliseconds(300), ShowBotTimer);

            Device.BeginInvokeOnMainThread((async () =>
            {
                if (canHide && StackLayoutHide.IsVisible)
                {
                    //isNeedShow = true;
                    await HideBotTimer();                    
                }

                if (e.FirstVisibleItemIndex > 0)
                {
                    await mainScroll1.FadeTo(0, 500, Easing.Linear);
                    await OrdersStack.TranslateTo(0, -30, 500, Easing.Linear);
                }
                else if (e.FirstVisibleItemIndex == 0)
                {
                    await mainScroll1.FadeTo(1, 500, Easing.Linear);
                    await OrdersStack.TranslateTo(0, 5, 100, Easing.Linear);
                }

            }));

            if(canHide)
                lastPos = mainScroll1.ScrollY + e.VerticalOffset;

            //Device.BeginInvokeOnMainThread((() =>
            //{                   
            //    lastPos = mainScroll1.ScrollY + e.VerticalOffset;
            //    if (ImageFon.Height > lastPos && lastPos >= 0)
            //    {
            //        mainScroll1.ScrollToAsync(0, lastPos, false);
            //        OrdersStack.Margin = new Thickness(OrdersStack.Margin.Left, OrdersStack.Margin.Top , OrdersStack.Margin.Right, OrdersStack.Margin.Bottom - e.VerticalOffset);
            //    }

            //}));

        }
        

        private void SwitchApp_OnToggled(object sender, ToggledEventArgs e)
        {
            SetReaded();
            Preferences.Set("SwitchApp",e.Value);

        }

        private void SwitchAppRead_OnToggled(object sender, ToggledEventArgs e)
        {
            SetReaded();
            Preferences.Set("SwitchAppRead",e.Value);
        }

        private void SwitchAppHidePerfom_OnToggled(object sender, ToggledEventArgs e)
        {
            SetReaded();
            Preferences.Set("SwitchAppHidePerfom",e.Value);
        }
    }
}