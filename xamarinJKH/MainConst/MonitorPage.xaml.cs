using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using FFImageLoading.Svg.Forms;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Monitor;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using Syncfusion.SfAutoComplete.XForms;
using xamarinJKH.Server.RequestModel.Monitor;
using Syncfusion.SfCalendar.XForms;

namespace xamarinJKH.MainConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonitorPage : ContentPage
    {
        public Color hex { get; set; }
        public int fontSize { get; set; }
        public int fontSize2 { get; set; }
        public int fontSize3 { get; set; }
        public int StarSize { get; set; }
        public bool isRunning = false;

        private List<string> period = new List<string>()
            {AppResources.TodayPeriod, AppResources.WeekPeriod, AppResources.MonthPeriod};

        private Dictionary<string, string> HousesGroup = new Dictionary<string, string>();
        private Dictionary<string, string> Houses = new Dictionary<string, string>();
        private Thickness IconViewNotComplite;
        private Thickness IconViewPrMargin;
        private double IconViewNotCompliteHeightRequest = 11;
        private double IconViewPrHeightRequest = 11;
        private string street = "";
        public ObservableCollection<NamedValue> Areas { get; set; }
        NamedValue selectedArea;

        public NamedValue SelectedArea
        {
            get => selectedArea;
            set
            {
                selectedArea = value;
                OnPropertyChanged("SelectedArea");
            }
        }

        public ObservableCollection<HouseProfile> Streets { get; set; }
        HouseProfile selectedStreet;

        public HouseProfile SelectedStreet
        {
            get => selectedStreet;
            set
            {
                selectedStreet = value;
                OnPropertyChanged("SelectedStreet");
            }
        }

        bool busy;

        public bool IsBusy
        {
            get => busy;
            set
            {
                busy = value;
                OnPropertyChanged("IsBusy");
            }
        }

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

        public MonitorPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Мониторинг");
            Areas = new ObservableCollection<NamedValue>();
            Streets = new ObservableCollection<HouseProfile>();
            NavigationPage.SetHasNavigationBar(this, false);
            hex = (Color) Application.Current.Resources["MainColor"];
            fontSize = 13;
            fontSize2 = 20;
            fontSize3 = 13;
            StarSize = 33;
            IconViewNotComplite = new Thickness(0, 5, 0, 0);
            IconViewPrMargin = new Thickness(0, 5, 0, 0);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    if (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Width > 2)
                        AreaGroups.MaximumDropDownHeight = 250;
                    else
                        AreaGroups.MaximumDropDownHeight = 150;


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
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        fontSize = 11;
                        StarSize = 25;
                        fontSize2 = 15;
                        fontSize3 = 12;
                        // ScrollViewContainer.Margin = new Thickness(10, -135, 10, 0);
                        // BackStackLayout.Margin = new Thickness(5, 25, 0, 0);
                        // IconViewNameUk.Margin = new Thickness(-3, -10, 0, 0);
                        // RelativeLayoutTop.Margin = new Thickness(0, 0, 0, -130);
                        IconViewNotComplite = 0;
                        IconViewNotCompliteHeightRequest = 8;
                        IconViewPrHeightRequest = 8;
                        IconViewPrMargin = 0;
                    }

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
            techSend.Tapped += TechSend; // async (s, e) => {  await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var addClick = new TapGestureRecognizer();
            addClick.Tapped += async (s, e) => { await StartStatistick(); };
            StackLayoutGroup.GestureRecognizers.Add(addClick);
            var addClickHome = new TapGestureRecognizer();
            addClickHome.Tapped += async (s, e) => { await StartStatistick(false); };
            StackLayoutHouse.GestureRecognizers.Add(addClickHome);

            var colapse = new TapGestureRecognizer();
            colapse.Tapped += async (s, e) =>
            {
                LayoutGrid.IsVisible = !LayoutGrid.IsVisible;
                if (LayoutGrid.IsVisible)
                {
                    IconViewArrow.Source = "ic_arrow_up_monitorpng";
                    MaterialFrameNotDoingContainer.Padding = new Thickness(0, 0, 0, 25);
                    colapseAll(AppResources.FailedRequests);
                }
                else
                {
                    IconViewArrow.Source = "ic_arrow_down_monitor";
                    MaterialFrameNotDoingContainer.Padding = 0;
                }
            };

            MaterialFrameNotDoing.GestureRecognizers.Add(colapse);

            _visibleModels.Add(AppResources.FailedRequests, new VisibleModel()
            {
                //IconView = IconViewArrow,
                _materialFrame = MaterialFrameNotDoingContainer,
                _grid = LayoutGrid
            });
            SetText();
            ChangeTheme = new Command(async () => { SetAdminName(); });
            MessagingCenter.Subscribe<Object>(this, "ChangeAdminMonitor", (sender) => ChangeTheme.Execute(null));
            BindingContext = this;
            Groups = new ObservableCollection<NamedValue>();
            MessagingCenter.Subscribe<Object>(this, "StartStatistic", sender =>
            {
                if (!loaded)
                {
                    Device.BeginInvokeOnMainThread(async () => await StartStatistick());
                }
                loaded = true;
            });
            AreasVisible = true;
            GroupVisible = true;
            StreetsVisible = true;
        }
        bool loaded;

        public ObservableCollection<NamedValue> Groups { get; set; }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            isRunning = true;
            //await StartStatistick();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isRunning = false;
        }

        public RestClientMP _server = new RestClientMP();
        private int Ryon = -1;
        private int HouseID = -1;
        async Task getMonitorStandart(int id, int houseID = -1)
        {
            return;
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            IsBusy = true;
            ItemsList<RequestStats> result = await _server.RequestStats(id, houseID);
            Ryon = id;
            HouseID = houseID;
            IsBusy = false;
            if (result.Error == null)
            {
                if (result.Data.Count > 0)
                    setMonitoring(result.Data[0]);
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
            }
        }

        void setMonitoring(RequestStats result)
        {
            if(!isRunning)
                return;
            List<PeriodStats> periodStatses = new List<PeriodStats>();
            periodStatses.Add(result.Today);
            periodStatses.Add(result.Week);
            periodStatses.Add(result.Month);
            setNotDoingApps(result.TotalUnperformedRequestsList);
            int i = 0;
            LayoutContent.Children.Clear();
            foreach (var each in periodStatses)
            {
                var container = AddMonitorPeriod(i, each, DateTime.Now);

                i++;
                LayoutContent.Children.Add(container);
            }
        }
        

        private StackLayout AddCalendar(int period, DateTime isReplace)
        {
            StackLayout container = new StackLayout
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 0
            };


            StackLayout dateCont = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };
            //BorderlessDatePickerMonitor datePicker = new BorderlessDatePickerMonitor
            //{
            //    IsVisible = false,
            //    Format = "dd.MM.yyyy",
            //    FontSize = 16,
            //    HorizontalOptions = LayoutOptions.Center,
            //    TextColor = hex
            //};

            //datePicker.MaximumDate = DateTime.Now;
            Label lableDate = new Label
            {
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                TextColor = hex
            };
            string text = isReplace.ToString("dd.MM.yyyy");

            var openPicker = new TapGestureRecognizer();

            switch (period)
            {
                case 0:
                    //openPicker.Tapped += (s, e) => { Device.BeginInvokeOnMainThread( () =>  datePicker.Focus()); };
                    //datePicker.MaximumDate = DateTime.Now;

                    openPicker.Tapped += (s, e) => {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            Configurations.LoadingConfig = new LoadingConfig
                            {
                                IndicatorColor = Color.Transparent,
                                OverlayColor = Color.Black,
                                Opacity = 0.8,
                                DefaultMessage = "",
                            };
                            await Loading.Instance.StartAsync(async progress =>
                            {
                                var ret = await Dialog.Instance.ShowAsync<CalendarDayDialog>(new
                                {
                                    HexColor = hex
                                });
                            });
                        }
                        );
                    };

                    //datePicker.DateSelected += async (sender, args) =>
                    //{
                    //    ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID, 
                    //        datePicker.Date.ToString("dd.MM.yyyy"),datePicker.Date.ToString("dd.MM.yyyy"));
                    //    if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                    //    {
                    //        var container = AddMonitorPeriod(0, result.Data[0].CustomPeriod, datePicker.Date);
                    //        if (LayoutContent != null && LayoutContent.Children.Count > 0)
                    //            LayoutContent.Children[0] = container;
                    //        colapseAllByName(this.period[0]);
                    //        colapseAll(this.period[0]);
                    //    }

                    //};

                    MessagingCenter.Subscribe<Object, DateTime>(this, "MonitorDay", async (sender, dt) => {
                        ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID,
                           dt.Date.ToString("dd.MM.yyyy"), dt.Date.ToString("dd.MM.yyyy"));
                        if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                        {
                            var container = AddMonitorPeriod(0, result.Data[0].CustomPeriod, dt.Date);
                            if (LayoutContent != null && LayoutContent?.Children?.Count > 0)
                                LayoutContent.Children[0] = container;
                            colapseAllByName(this.period[0]);
                            colapseAll(this.period[0]);
                        }
                    });

                    //dateCont.Children.Add(datePicker);

                    break;
                case 1:
                    openPicker.Tapped += (s, e) => {
                        Device.BeginInvokeOnMainThread(async () =>                          
                          {
                              Configurations.LoadingConfig = new LoadingConfig
                              {
                                  IndicatorColor = Color.Transparent,
                                  OverlayColor = Color.Black,
                                  Opacity = 0.8,
                                  DefaultMessage = "",
                              };
                              await Loading.Instance.StartAsync(async progress =>
                              {                                  
                                  var ret = await Dialog.Instance.ShowAsync<CalendarWeekDialog>(new
                                  {
                                      HexColor = hex
                                  });
                              });
                          }                          
                        );
                    };
                    var d1 = isReplace.DayOfWeek.GetHashCode()-1;
                    DateTime dateMonday = isReplace.AddDays(-d1); 
                    DateTime dateSunday = dateMonday.AddDays(6);
                    text = dateMonday.ToString("dd.MM") + "-" + dateSunday.ToString("dd.MM.yyyy");

                    
                    MessagingCenter.Subscribe<Object, SelectionRange>(this, "MonitorDateStart", async (sender, dt) => {
                    DateTime dateMonday = dt.StartDate.Date;
                        DateTime dateSunday = dt.EndDate.Date;
                        ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID,
                            dateMonday.ToString("dd.MM.yyyy"), dateSunday.ToString("dd.MM.yyyy"));
                        if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                        {
                            var container = AddMonitorPeriod(1, result.Data[0].CustomPeriod, dt.StartDate.Date);
                            if (LayoutContent != null && LayoutContent?.Children?.Count > 0)
                                LayoutContent.Children[1] = container;
                            colapseAllByName(this.period[1]);
                            colapseAll(this.period[1]);
                        }
                    });


                    //datePicker.DateSelected += async (sender, args) =>
                    //{
                    //    DateTime dateMonday = datePicker.Date.AddDays((datePicker.Date.DayOfWeek.GetHashCode() - 1) * -1).Date;
                    //    DateTime dateSunday = datePicker.Date.AddDays(7 - datePicker.Date.DayOfWeek.GetHashCode()).Date;
                    //    // lableDate.Text = dateMonday.ToString("dd.MM") + "-" + dateSunday.ToString("dd.MM.yyyy");
                    //    ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID, 
                    //        dateMonday.ToString("dd.MM.yyyy"),dateSunday.ToString("dd.MM.yyyy"));
                    //    if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                    //    {
                    //        var container = AddMonitorPeriod(1, result.Data[0].CustomPeriod, datePicker.Date);
                    //        if (LayoutContent != null && LayoutContent.Children.Count > 0)
                    //            LayoutContent.Children[1] = container;
                    //        colapseAllByName(this.period[1]);
                    //        colapseAll(this.period[1]);
                    //    }
                    //}; 
                    break;
                case 2:
                    string s = isReplace.ToString("MMMM yyyy");
                    text = FirstLetterToUpper(s);

                    openPicker.Tapped += (s, e) => {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            Configurations.LoadingConfig = new LoadingConfig
                            {
                                IndicatorColor = Color.Transparent,
                                OverlayColor = Color.Black,
                                Opacity = 0.8,
                                DefaultMessage = "",
                            };
                            await Loading.Instance.StartAsync(async progress =>
                            {
                                var ret = await Dialog.Instance.ShowAsync<CalendarMonthDialog>(new
                                {
                                    HexColor = hex
                                });
                            });
                        }
                        );
                    };

                    MessagingCenter.Subscribe<Object, DateTime>(this, "MonitorMonth", async (sender, dt) => {
                        DateTime now = dt.Date;
                        var startDate = new DateTime(now.Year, now.Month, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID,
                            startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy"));
                        if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                        {
                            var container = AddMonitorPeriod(2, result.Data[0].CustomPeriod, dt.Date);
                            if (LayoutContent != null && LayoutContent?.Children?.Count > 0)
                                LayoutContent.Children[2] = container;
                            colapseAllByName(this.period[2]);
                            colapseAll(this.period[2]);
                        }
                    });


                    //datePicker.DateSelected += async (sender, args) =>
                    //{
                    //    DateTime now = datePicker.Date;
                    //    var startDate = new DateTime(now.Year, now.Month, 1);
                    //    var endDate = startDate.AddMonths(1).AddDays(-1);
                    //    ItemsList<RequestStats> result = await _server.RequestStats(Ryon, HouseID, 
                    //        startDate.ToString("dd.MM.yyyy"),endDate.ToString("dd.MM.yyyy"));
                    //    if (result.Error == null && result.Data[0] != null && result.Data[0].CustomPeriod != null)
                    //    {
                    //        var container = AddMonitorPeriod(2, result.Data[0].CustomPeriod, datePicker.Date);
                    //        if (LayoutContent != null && LayoutContent.Children.Count > 0)
                    //            LayoutContent.Children[2] = container;
                    //        colapseAllByName(this.period[2]);
                    //        colapseAll(this.period[2]);
                    //    }
                    //}; 
                    break;
            }
            lableDate.Text = text;

           
            SvgCachedImage arrow = new SvgCachedImage
            {
                Source = "resource://xamarinJKH.Resources.ic_arrow_forward.svg",
                HeightRequest = 12,
                Rotation = 90,
                Margin = new Thickness(0, 5, 0, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                ReplaceStringMap = new Dictionary<string, string> {{"#000000", $"#{Settings.MobileSettings.color}"}}
            };

            Label splitLine = new Label
            {
                HeightRequest = 1,
                BackgroundColor = hex,
                HorizontalOptions = LayoutOptions.Fill
            };

            //var openPicker = new TapGestureRecognizer();
            //openPicker.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(async () => datePicker.Focus()); };
            container.GestureRecognizers.Add(openPicker);

            
            dateCont.Children.Add(lableDate);
            dateCont.Children.Add(arrow);

            container.Children.Add(dateCont);
            container.Children.Add(splitLine);

            return container;
        }
        public static string FirstLetterToUpper(string str)
        {
            if (str.Length > 0) { return Char.ToUpper(str[0]) + str.Substring(1); }
            return "";
        }
        private MaterialFrame AddMonitorPeriod(int i, PeriodStats each, DateTime dateTitle)
        {
            MaterialFrame container = new MaterialFrame();
            container.SetAppThemeColor(Frame.BorderColorProperty, (Color) Application.Current.Resources["MainColor"],
                Color.White);
            container.Margin = new Thickness(20, 0, 20, 10);
            container.CornerRadius = 35;
            container.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            container.SetOnAppTheme(CustomRenderers.MaterialFrame.ElevationProperty, 0, 20);
            container.BackgroundColor = Color.White;
            container.Padding = new Thickness(0, 0, 0, 25);

            StackLayout stackLayoutFrame = new StackLayout();
            stackLayoutFrame.Spacing = 0;

            container.Content = stackLayoutFrame;

            MaterialFrame materialFrameTop = new MaterialFrame();
            materialFrameTop.SetAppThemeColor(Frame.BorderColorProperty,
                (Color) Application.Current.Resources["MainColor"],
                Color.White);
            materialFrameTop.CornerRadius = 35;
            materialFrameTop.BackgroundColor = Color.White;
            materialFrameTop.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            materialFrameTop.SetOnAppTheme(CustomRenderers.MaterialFrame.ElevationProperty, 0, 20);
            materialFrameTop.Padding = new Thickness(0, 25, 0, 25);

            stackLayoutFrame.Children.Add(materialFrameTop);

            StackLayout stackLayoutTop = new StackLayout();
            stackLayoutTop.Orientation = StackOrientation.Horizontal;

            materialFrameTop.Content = stackLayoutTop;

            IconView iconViewTop = new IconView()
            {
                Source = "ic_calendar",
                Foreground = hex,
                HeightRequest = 30,
                Margin = new Thickness(-10, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            Label labelTitleTop = new Label()
            {
                Text = period[i],
                FontSize = 15,
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(-20, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            IconView iconViewArrow = new IconView()
            {
                Source = "ic_arrow_up_monitorpng",
                HeightRequest = 25,
                WidthRequest = 25,
                Foreground = hex,
                Margin = new Thickness(0, 0, 15, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };


            stackLayoutTop.Children.Add(iconViewTop);
            stackLayoutTop.Children.Add(labelTitleTop);
            stackLayoutTop.Children.Add(AddCalendar(i, dateTitle));
            stackLayoutTop.Children.Add(iconViewArrow);

            StackLayout stackLayoutBot = new StackLayout()
            {
                Margin = new Thickness(20, 0, 20, 0),
                Spacing = 0
            };

            var colapse = new TapGestureRecognizer();
            colapse.Tapped += (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {


                    stackLayoutBot.IsVisible = !stackLayoutBot.IsVisible;
                    if (stackLayoutBot.IsVisible)
                    {
                        iconViewArrow.Source = "ic_arrow_up_monitorpng";
                        container.Padding = new Thickness(0, 0, 0, 25);
                        colapseAll(labelTitleTop.Text);
                    }
                    else
                    {
                        iconViewArrow.Source = "ic_arrow_down_monitor";
                        container.Padding = 0;
                    }
                });
            };

            if (i > 0 || 1 == 1)
            {
                stackLayoutBot.IsVisible = false;
                iconViewArrow.Source = "ic_arrow_down_monitor";
                container.Padding = 0;
            }

            if (!_visibleModels.ContainsKey(period[i]))
            {
                _visibleModels.Add(period[i], new VisibleModel()
                {
                    IconView = iconViewArrow,
                    _materialFrame = container,
                    _grid = stackLayoutBot
                });
            }
            else
            {
                _visibleModels[period[i]] = new VisibleModel()
                {
                    //IconView = IconViewArrow,
                    _materialFrame = container,
                    _grid = stackLayoutBot
                };
            }

            materialFrameTop.GestureRecognizers.Add(colapse);

            stackLayoutFrame.Children.Add(stackLayoutBot);

            FormattedString formatted = new FormattedString();
            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.RequestsReceived} ",
                TextColor = Color.Black
            });
            formatted.Spans.Add(new Span
            {
                Text = each.RequestsCount.ToString(),
                TextColor = hex,
                FontAttributes = FontAttributes.Bold
            });

            Label labelCountApps = new Label()
            {
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 10),
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                FormattedText = formatted
            };

            Grid grid = new Grid
            {
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Absolute)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                }
            };

            StackLayout stackLayoutGridOne = new StackLayout();

            StackLayout stackLayoutNotDoing = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal
            };

            IconView iconViewDis = new IconView()
            {
                Source = "ic_dislike",
                HeightRequest = fontSize2,
                WidthRequest = fontSize2,
                Foreground = hex,
                HorizontalOptions = LayoutOptions.Center
            };

            Label labelNotDoing = new Label()
            {
                Text = $"{AppResources.FailedReq}:",
                FontSize = fontSize3,
                TextColor = Color.Black,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };

            StackLayout stackLayoutnotDoingCount = new StackLayout()
            {
                Spacing = 0
            };

            StackLayout stackLayoutNotDoingContent = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 0
            };

            Label labelCountNotDoing = new Label()
            {
                Text = each.UnperformedRequestsList?.Count.ToString(),
                TextColor = hex,
                FontSize = fontSize,
                FontAttributes = FontAttributes.Bold
            };

            var forwardAppsNot = new TapGestureRecognizer();
            forwardAppsNot.Tapped += async (s, e) =>
            {
                if (each.UnperformedRequestsList?.Count > 0)
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is MonitorAppsPage) == null)
                        await Navigation.PushAsync(new MonitorAppsPage(each.UnperformedRequestsList));
                }
            };

            stackLayoutNotDoingContent.GestureRecognizers.Add(forwardAppsNot);


            IconView iconViewArrowForward = new IconView()
            {
                Source = "ic_arrow_forward",
                HeightRequest = IconViewNotCompliteHeightRequest,
                WidthRequest = IconViewNotCompliteHeightRequest,
                Margin = IconViewNotComplite,
                VerticalOptions = LayoutOptions.Center,
                Foreground = hex,
                HorizontalOptions = LayoutOptions.Center
            };

            stackLayoutNotDoingContent.Children.Add(labelCountNotDoing);
            stackLayoutNotDoingContent.Children.Add(iconViewArrowForward);

            Label labelSeparatorNotDoing = new Label()
            {
                HeightRequest = 1,
                BackgroundColor = hex,
                HorizontalOptions = LayoutOptions.Fill
            };

            stackLayoutnotDoingCount.Children.Add(stackLayoutNotDoingContent);
            stackLayoutnotDoingCount.Children.Add(labelSeparatorNotDoing);


            stackLayoutNotDoing.Children.Add(iconViewDis);
            stackLayoutNotDoing.Children.Add(labelNotDoing);
            stackLayoutNotDoing.Children.Add(stackLayoutnotDoingCount);

            stackLayoutGridOne.Children.Add(stackLayoutNotDoing);

            Label separatorVertical = new Label()
            {
                WidthRequest = 1,
                BackgroundColor = Color.FromHex("#878787"),
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            StackLayout stackLayoutUnperformed = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start
            };

            IconView iconViewUnperformed = new IconView()
            {
                Source = "ic_time",
                HeightRequest = fontSize2,
                WidthRequest = fontSize2,
                Foreground = hex,
                HorizontalOptions = LayoutOptions.Center
            };
            Label labelUnperformed = new Label()
            {
                Text = AppResources.Overdue,
                FontSize = fontSize3,
                TextColor = Color.Black,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };

            StackLayout stackLayoutUnperformedCount = new StackLayout()
            {
                Spacing = 0,
                VerticalOptions = LayoutOptions.Center
            };

            StackLayout stackLayoutUnperformedContent = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 0
            };

            Label labelCountUnperformed = new Label()
            {
                Text = each.OverdueRequestsList?.Count.ToString(),
                TextColor = hex,
                FontSize = fontSize,
                FontAttributes = FontAttributes.Bold
            };
            IconView iconViewUnperformedArrowForward = new IconView()
            {
                Source = "ic_arrow_forward",
                HeightRequest = IconViewPrHeightRequest,
                WidthRequest = IconViewPrHeightRequest,
                Margin = IconViewPrMargin,
                VerticalOptions = LayoutOptions.Start,
                Foreground = hex,
                HorizontalOptions = LayoutOptions.Center
            };

            var forwardAppsUnper = new TapGestureRecognizer();
            forwardAppsUnper.Tapped += async (s, e) =>
            {
                if (each.OverdueRequestsList?.Count > 0)

                    if (Navigation.NavigationStack.FirstOrDefault(x => x is MonitorAppsPage) == null)
                        await Navigation.PushAsync(new MonitorAppsPage(each.OverdueRequestsList));
            };

            stackLayoutUnperformedCount.GestureRecognizers.Add(forwardAppsUnper);

            stackLayoutUnperformedContent.Children.Add(labelCountUnperformed);
            stackLayoutUnperformedContent.Children.Add(iconViewUnperformedArrowForward);

            Label labelSeparatorUnperformed = new Label()
            {
                HeightRequest = 1,
                BackgroundColor = hex,
                HorizontalOptions = LayoutOptions.Fill
            };

            stackLayoutUnperformedCount.Children.Add(stackLayoutUnperformedContent);
            stackLayoutUnperformedCount.Children.Add(labelSeparatorUnperformed);

            stackLayoutUnperformed.Children.Add(iconViewUnperformed);
            stackLayoutUnperformed.Children.Add(labelUnperformed);
            stackLayoutUnperformed.Children.Add(stackLayoutUnperformedCount);

            grid.Children.Add(stackLayoutGridOne, 0, 0);
            grid.Children.Add(separatorVertical, 1, 0);
            grid.Children.Add(stackLayoutUnperformed, 2, 0);

            setCustumerXaml(ref grid, each);

            Label separatorHorizontal = new Label()
            {
                HeightRequest = 1,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#878787")
            };

            StackLayout stackLayoutStar = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal
            };

            StackLayout stackLayoutPoint = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Vertical
            };

            Label labelTitlePoint = new Label()
            {
                Text = $"{AppResources.Marks}:",
                FontSize = fontSize,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.Black
            };
            stackLayoutPoint.Children.Add(labelTitlePoint);

            StackLayout stackLayoutStarItems = new StackLayout()
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Horizontal
            };

            int[] points = new[]
            {
                each.ClosedRequestsWithMark1Count,
                each.ClosedRequestsWithMark2Count,
                each.ClosedRequestsWithMark3Count,
                each.ClosedRequestsWithMark4Count,
                each.ClosedRequestsWithMark5Count
            };

            for (int j = 0; j < points.Length; j++)
            {
                StackLayout stackLayoutStarItem = new StackLayout()
                {
                    Spacing = -10
                };
                var forwardStar = new TapGestureRecognizer();
                List<Requests> requests = getRequestsStar(each, j);
                forwardStar.Tapped += async (s, e) =>
                {
                    if (requests?.Count > 0)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is MonitorAppsPage) == null)
                            await Navigation.PushAsync(new MonitorAppsPage(requests));
                    }
                };

                stackLayoutStarItem.GestureRecognizers.Add(forwardStar);
                Frame frameStarContainer = new Frame()
                {
                    Padding = 0,
                    CornerRadius = 0,
                    HasShadow = false,
                    BackgroundColor = Color.Transparent,
                    IsClippedToBounds = true
                };

                Grid gridContentStar = new Grid()
                {
                    HeightRequest = 50,
                    IsClippedToBounds = true
                };

                IconView iconViewStar = new IconView()
                {
                    Source = "ic_star",
                    HeightRequest = StarSize,
                    WidthRequest = StarSize,
                    Foreground = hex,
                    VerticalOptions = LayoutOptions.Center
                };

                Frame frameContentStar = new Frame()
                {
                    Margin = 0,
                    Padding = 0,
                    CornerRadius = 5,
                    BackgroundColor = Color.Transparent,
                    HasShadow = false,
                    IsClippedToBounds = true
                };

                Label labelTextStar = new Label()
                {
                    Text = points[j].ToString(),
                    FontSize = fontSize,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 0,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.White
                };

                frameContentStar.Content = labelTextStar;

                gridContentStar.Children.Add(iconViewStar);
                gridContentStar.Children.Add(frameContentStar);

                frameStarContainer.Content = gridContentStar;

                Label labelNumberPoints = new Label()
                {
                    Text = (j + 1).ToString(),
                    FontSize = fontSize,
                    TextColor = Color.FromHex("#878787"),
                    HorizontalOptions = LayoutOptions.Center
                };

                stackLayoutStarItem.Children.Add(frameStarContainer);
                stackLayoutStarItem.Children.Add(labelNumberPoints);


                stackLayoutStarItems.Children.Add(stackLayoutStarItem);
            }


            stackLayoutStar.Children.Add(stackLayoutPoint);
            stackLayoutStar.Children.Add(stackLayoutStarItems);

            stackLayoutBot.Children.Add(labelCountApps);
            stackLayoutBot.Children.Add(grid);
            stackLayoutBot.Children.Add(separatorHorizontal);
            stackLayoutBot.Children.Add(stackLayoutStar);
            return container;
        }

        List<Requests> getRequestsStar(PeriodStats stats, int i)
        {
            switch (i)
            {
                case 0: return stats.ClosedRequestsWithMark1List;
                case 1: return stats.ClosedRequestsWithMark2List;
                case 2: return stats.ClosedRequestsWithMark3List;
                case 3: return stats.ClosedRequestsWithMark4List;
                case 4: return stats.ClosedRequestsWithMark5List;
                default: return new List<Requests>();
            }
        }

        private void AutoCompleteHouses_OnFocusChanged(object sender, FocusChangedEventArgs e)
        {
            // (sender as SfAutoComplete).Unfocus();
            if(!isRunning)
                return;
            try
            {
                var result = new List<NamedValue>();
                var selected = AreaGroups.SelectedItem as IEnumerable<Object>;
                var selected_indecies = selected.Select(x => (x as NamedValue).ID).ToList();
                if (selected != null || selected_indecies?.Count > 0)
                {
                    result.AddRange(Groups.Where(x => !selected_indecies.Contains(x.ID)));
                    AreaGroups.DataSource = result;
                }
            }
            catch
            {

            }
            

        }
        public async Task StartStatistick(bool isGroup = true)
        {
            if(!isRunning)
                return;
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = hex,
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.MonitorStats,
            };

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Run(async () =>
            {
                //Device.BeginInvokeOnMainThread(async () =>
                //{
                var area_groups = await _server.GetAreaGroups();
                if (area_groups.Error == null)
                {
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    foreach (var area in area_groups.Data)
                    {
                        Groups.Add(area);
                    }
                    //});
                    if (AreaGroups.DataSource == null)
                    {
                        AreaGroups.DataSource = Groups;
                    }
                    AreaGroups.IsVisible = Groups?.Count() == 0;
                }
                //if (isGroup)
                //    await getHouseGroups();
                //else
                //{
                //    await getHouse();
                //}
                //});
            }).ContinueWith(async(obj)=> {
                if (isGroup)
                    await getHouseGroups();
            }).ContinueWith(async (obj) =>
            {
                await getHouse();
            }).ContinueWith(async (res) =>
            {
                await Task.Delay(500);
                Button_Clicked(null, null);
                try
                {
                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog(false));
                    await PopupNavigation.Instance.PopAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
               
            });

            });

        }

        void colapseAll(string name)
        {
            if(!isRunning)
                return;
            try
            {
                if (_visibleModels != null)
                    foreach (var each in _visibleModels
                        .Where(each => !string.IsNullOrEmpty(each.Key) && each.Value != null)
                        .Where(each => !each.Key.Equals(name) && each.Value._grid.IsVisible))
                    {
                        each.Value._grid.IsVisible = false;
                        each.Value._materialFrame.Padding = 0;
                        each.Value.IconView.Source = "ic_arrow_down_monitor";
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        void colapseAllByName(string name)
        {
            if(!isRunning)
                return;
            try
            {
                if (_visibleModels != null)
                    foreach (var each in _visibleModels
                        .Where(each => !string.IsNullOrEmpty(each.Key) && each.Value != null)
                        .Where(each => each.Key.Equals(name)))
                    {
                        each.Value._grid.IsVisible = true;
                        each.Value._materialFrame.Padding = new Thickness(0,0,0,10);
                        each.Value.IconView.Source = "ic_arrow_forward";
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        async Task getHouseGroups()
        {
            if(!isRunning)
                return;
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            IsBusy = true;
            ItemsList<NamedValue> groups = await _server.GetHouseGroups();
            //IsBusy = false;
            if (groups.Error == null)
            {
                string[] param = null;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (groups?.Data != null)
                    if (groups?.Data?.Count > 0)
                    {
                        foreach (var group in groups.Data)
                        {
                            Areas.Add(group);
                        }

                        SelectedArea = Areas[0];

                        setListGroups(groups, ref param);
                        LayoutContent.Children.Clear();
                        MaterialFrameNotDoingContainer.IsVisible = false;
                        //LabelGroup.Text = action;
                        street = SelectedArea.Name;
                    }

                    HouseGroups.IsVisible = Areas?.Count > 0;
                    
                    //await getMonitorStandart(Int32.Parse(HousesGroup[SelectedArea.Name]));
                });


                return;
                var action = await DisplayActionSheet(AppResources.AreaChoose, AppResources.Cancel, null, param);
                if (action != null && !action.Equals(AppResources.Cancel))
                {
                    LayoutContent.Children.Clear();
                    MaterialFrameNotDoingContainer.IsVisible = false;
                    LabelGroup.Text = action;
                    street = action;
                    await getMonitorStandart(Int32.Parse(HousesGroup[action]));
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, groups.Error, "OK");
            }
        }

        async Task getHouse()
        {
            if(!isRunning)
                return;
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            IsBusy = true;
            ItemsList<HouseProfile> groups = await _server.GetHouse();
            //IsBusy = false;

            if (groups.Error == null)
            {
                try
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Streets.Clear();
                        foreach (var group in groups.Data)
                        {
                            if (!string.IsNullOrEmpty(group.Address))
                                Streets.Add(group);
                        }

                        HousesList.IsVisible = Streets?.Count > 0;
                        //SelectedStreet = Streets[0];
                        //Streets[0].Selected = true;
                        // StreetsCollection.ScrollTo(Streets[0]);
                        string[] param = null;
                        setListHouse(groups, ref param);
                        var action = "";// Streets[0].Address;
                        if (action != null && !action.Equals(AppResources.Cancel))
                        {
                            //LayoutContent.Children.Clear();
                            //MaterialFrameNotDoingContainer.IsVisible = false;
                            //LabelHouse.Text = action;
                            //await getMonitorStandart(-1, Int32.Parse(Houses[action]));
                        }
                        LoadingStreets = false;
                    });
                }
                catch { }
                
                return;
                string[] param = null;
                setListHouse(groups, ref param);
                var action = await DisplayActionSheet(AppResources.HomeChoose, AppResources.Cancel, null, param);
                if (action != null && !action.Equals(AppResources.Cancel))
                {
                    LayoutContent.Children.Clear();
                    MaterialFrameNotDoingContainer.IsVisible = false;
                    LabelHouse.Text = action;
                    await getMonitorStandart(-1, Int32.Parse(Houses[action]));
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, groups.Error, "OK");
            }
        }

        void setListGroups(ItemsList<NamedValue> groups, ref string[] param)
        {
            if(!isRunning)
                return;
            HousesGroup = new Dictionary<string, string>();
            if (groups != null)
            {
                if (groups.Data != null)
                {
                    param = new string [groups.Data.Count];
                    int i = 0;
                    foreach (var each in groups.Data)
                    {
                        HousesGroup.Add(each.Name, each.ID.ToString());
                        param[i] = each.Name;
                        i++;
                    }
                }
            }
        }

        void setListHouse(ItemsList<HouseProfile> groups, ref string[] param)
        {
            if(!isRunning)
                return;
            Houses = new Dictionary<string, string>();
            if (groups?.Data != null)
            {
                param = new string [groups.Data.Count];
                int i = 0;
                foreach (var each in groups.Data)
                {
                    try
                    {
                        if (each.Address != null)
                        {
                            Houses.Add(each.Address, each.ID.ToString());
                            param[i] = each.Address;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    i++;
                }
            }
        }

        void setCustumerXaml(ref Grid grid, PeriodStats periodStats)
        {
            if(!isRunning)
                return;
            Dictionary<string, int> UnperformedMap = setPerformer(periodStats.UnperformedRequestsList);
            Dictionary<string, int> Overdue = setPerformer(periodStats.OverdueRequestsList);

            int max = Math.Max(UnperformedMap.Count, Overdue.Count);
            for (int i = 0; i < max; i++)
            {
                RowDefinition rowDefinition = new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                grid.RowDefinitions.Add(rowDefinition);
                Label labelSeparatorUnperformed = new Label()
                {
                    MinimumWidthRequest = 1,
                    BackgroundColor = Color.FromHex("#878787"),
                    VerticalOptions = LayoutOptions.Fill
                };
                grid.Children.Add(labelSeparatorUnperformed, 1, i + 1);
            }

            int j;
            VisiblePerformers(grid, UnperformedMap, 0, periodStats.UnperformedRequestsList);
            VisiblePerformers(grid, Overdue, 2, periodStats.OverdueRequestsList);
        }

        private void VisiblePerformers(Grid grid, Dictionary<string, int> UnperformedMap, int position,
            List<Requests> requestses, bool space = true)
        {
            if(!isRunning)
                return;
            int j = space ? 1 : 0;

            foreach (var each in UnperformedMap)
            {
                StackLayout stackLayoutNotDoing = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal
                };

                if (j == 1)
                {
                    stackLayoutNotDoing.Margin = new Thickness(0, 5, 0, 0);
                }


                Label labelNotDoing = new Label()
                {
                    Text = each.Key.Split()[0],
                    FontSize = fontSize3,
                    TextColor = Color.Black,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Fill
                };

                StackLayout stackLayoutnotDoingCount = new StackLayout()
                {
                    Spacing = 0,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };

                StackLayout stackLayoutNotDoingContent = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 0
                };

                Label labelCountNotDoing = new Label()
                {
                    Text = each.Value.ToString(),
                    TextColor = hex,
                    FontSize = fontSize,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };
                IconView iconViewArrowForward = new IconView()
                {
                    Source = "ic_arrow_forward",
                    HeightRequest = IconViewNotCompliteHeightRequest,
                    WidthRequest = IconViewNotCompliteHeightRequest,
                    Margin = IconViewNotComplite,
                    VerticalOptions = LayoutOptions.Center,
                    Foreground = hex,
                    HorizontalOptions = LayoutOptions.Center
                };

                var forwardAppsNot = new TapGestureRecognizer();
                List<Requests> requests = getRequests(each.Key, requestses);
                forwardAppsNot.Tapped += async (s, e) =>
                {
                    if (requests?.Count > 0)
                    {
                        if (Navigation.NavigationStack.FirstOrDefault(x => x is MonitorAppsPage) == null)
                            await Navigation.PushAsync(new MonitorAppsPage(requests));
                    }
                };

                stackLayoutNotDoingContent.GestureRecognizers.Add(forwardAppsNot);

                stackLayoutNotDoingContent.Children.Add(labelCountNotDoing);
                stackLayoutNotDoingContent.Children.Add(iconViewArrowForward);

                Label labelSeparatorNotDoing = new Label()
                {
                    HeightRequest = 1,
                    BackgroundColor = hex,
                    HorizontalOptions = LayoutOptions.Fill
                };

                stackLayoutnotDoingCount.Children.Add(stackLayoutNotDoingContent);
                stackLayoutnotDoingCount.Children.Add(labelSeparatorNotDoing);


                stackLayoutNotDoing.Children.Add(labelNotDoing);
                stackLayoutNotDoing.Children.Add(stackLayoutnotDoingCount);

                grid.Children.Add(stackLayoutNotDoing, position, j);
                j++;
            }
        }

        List<Requests> getRequests(string name, List<Requests> result)
        {
            List<Requests> requestses = new List<Requests>();
            foreach (var each in result)
            {
                var eachPerformer = each.Performer;
                if (eachPerformer == null || eachPerformer.Equals("") || eachPerformer.Equals("-"))
                {
                    if (name.Equals("Сотрудник"))
                    {
                        requestses.Add(each);
                    }
                }
                else if (eachPerformer.Equals(name))
                {
                    requestses.Add(each);
                }
            }

            return requestses;
        }


        void setNotDoingApps(List<Requests> requestses)
        {
            if(!isRunning)
                return;
            MaterialFrameNotDoingContainer.IsVisible = true;
            LayoutGrid.Children.Clear();
            Grid grid = new Grid
            {
                RowSpacing = 0,
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Absolute)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                }
            };

            LayoutGrid.Children.Add(grid);
            FormattedString formatted = new FormattedString();
            IconViewArrow.Source = "ic_arrow_down_monitor";
            MaterialFrameNotDoingContainer.Padding = 0;
            LayoutGrid.IsVisible = false;
            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.FailedRequests}: ",
                TextColor = Color.Black
            });
            formatted.Spans.Add(new Span
            {
                Text = requestses.Count.ToString(),
                TextColor = hex,
                FontAttributes = FontAttributes.Bold
            });
            LabelNotDoingCount.FormattedText = formatted;

            Dictionary<string, int> dictionary = setPerformer(requestses);
            Dictionary<string, int> dictionaryFirst = new Dictionary<string, int>();
            Dictionary<string, int> dictionarySecond = new Dictionary<string, int>();
            int half = dictionary.Count / 2;

            int i = 0;
            foreach (var each in dictionary)
            {
                if (i <= half)
                    dictionaryFirst.Add(each.Key, each.Value);
                else
                    dictionarySecond.Add(each.Key, each.Value);
                ;
                i++;
            }


            int max = Math.Max(dictionaryFirst.Count, dictionarySecond.Count);
            for (int j = 0; j < max; j++)
            {
                RowDefinition rowDefinition = new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                grid.RowDefinitions.Add(rowDefinition);
                Label labelSeparatorUnperformed = new Label()
                {
                    MinimumWidthRequest = 1,
                    BackgroundColor = Color.FromHex("#878787"),
                    VerticalOptions = LayoutOptions.Fill
                };
                grid.Children.Add(labelSeparatorUnperformed, 1, j);
            }


            VisiblePerformers(grid, dictionaryFirst, 0, requestses, false);
            VisiblePerformers(grid, dictionarySecond, 2, requestses, false);
        }


        Dictionary<string, int> setPerformer(List<Requests> requestses)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            try
            {
                foreach (var each in requestses)
                {
                    var eachPerformer = each.Performer;
                    if (eachPerformer == null || eachPerformer.Equals("") || eachPerformer.Equals("-"))
                    {
                        eachPerformer = "Сотрудник";
                    }

                    if (!dictionary.ContainsKey(eachPerformer))
                    {
                        dictionary.Add(eachPerformer, 1);
                    }
                    else
                    {
                        dictionary[eachPerformer]++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return dictionary;
        }

        public Command ChangeTheme { get; set; }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            SetAdminName();

            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            Frame.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#8D8D8D"));
            MaterialFrameNotDoingContainer.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            MaterialFrameNotDoing.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
        }

        private void SetAdminName()
        {
            if(!isRunning)
                return;
            FormattedString formatted = new FormattedString();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;

            formatted.Spans.Add(new Span
            {
                Text = Settings.Person.FIO + ", ",
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15
            });
            formatted.Spans.Add(new Span
            {
                Text = AppResources.GoodDay2,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontSize = 15
            });


            var selectedArea = new NamedValue();
            selectedArea = this.SelectedArea;

            var selectedStreet = new HouseProfile();
            selectedStreet = this.SelectedStreet;

            if (this.Areas != null)
                foreach (var area in this.Areas)
                {
                    area.Selected = true;
                    area.Selected = false;
                }

            if (SelectedArea != null)
                SelectedArea.Selected = true;

            if (this.Streets != null)
            {
                foreach (var street in Streets)
                {
                    street.Selected = true;
                    street.Selected = false;
                }

                if (SelectedStreet != null)
                    SelectedStreet.Selected = true;
            }
        }

        Dictionary<string, VisibleModel> _visibleModels = new Dictionary<string, VisibleModel>();

        class VisibleModel
        {
            public IconView IconView { get; set; }
            public MaterialFrame _materialFrame { get; set; }
            public StackLayout _grid { get; set; }
        }

        bool loadingStreets;
        public bool LoadingStreets
        {
            get => loadingStreets;
            set
            {
                loadingStreets = value;
                OnPropertyChanged("LoadingStreets");
            }
        }

        private async void CollectionView_SelectionChanged(object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var selection = e.CurrentSelection[0] as NamedValue;
                    if (selection != null)
                    {
                        foreach (var area in Areas)
                        {
                            area.Selected = false;
                        }

                        selection.Selected = true;
                    }
                    LoadingStreets = true;
                    (sender as CollectionView).ScrollTo(selection);
                    await getHouse();
                });
            }
            catch
            {
            }
        }

        private async void StreetCollectionSelectionChanged(object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    var selection = e.CurrentSelection[0] as HouseProfile;
                    var action = selection.Address;
                    if (action != null && !action.Equals(AppResources.Cancel))
                    {
                        if (selection != null)
                        {
                            foreach (var street in Streets)
                            {
                                street.Selected = false;
                            }

                            selection.Selected = true;
                        }

                        LayoutContent.Children.Clear();
                        MaterialFrameNotDoingContainer.IsVisible = false;
                        (sender as CollectionView).ScrollTo(selection);
                        //LabelHouse.Text = action;
                        await getMonitorStandart(-1, Int32.Parse(Houses[action]));
                    }
                });
            }
            catch
            {
            }
        }

        private void pickerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!isRunning)
                return;
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (SelectedArea != null)
                    {
                        foreach (var area in Areas)
                        {
                            area.Selected = false;
                        }

                        SelectedArea.Selected = true;
                    }
                    LoadingStreets = true;
                    // (sender as CollectionView).ScrollTo(SelectedArea);
                    await getHouse();
                });
            }
            catch
            {
            }
        }

        private void PickerHouse_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if(!isRunning)
                return;
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    var action = SelectedStreet.Address;
                    if (action != null && !action.Equals(AppResources.Cancel))
                    {
                        if (SelectedStreet != null)
                        {
                            foreach (var street in Streets)
                            {
                                street.Selected = false;
                            }

                            SelectedStreet.Selected = true;
                        }

                        LayoutContent.Children.Clear();
                        MaterialFrameNotDoingContainer.IsVisible = false;
                        // (sender as CollectionView).ScrollTo(selection);
                        //LabelHouse.Text = action;
                        await getMonitorStandart(-1, Int32.Parse(Houses[action]));
                    }
                });
            }
            catch
            {
            }
        }

        private List<int> AreaIDs { get; set; }
        private List<int> HouseIDs { get; set; }
        private List<int> GroupIDs { get; set; }
        private async void Button_Clicked(object sender, EventArgs e)
        {
            if(!isRunning)
                return;
            var queries = new List<RequestStatsQuerySettings>();
            if (HouseIDs == null)
                HouseIDs = new List<int>();
            if (AreaIDs == null)
                AreaIDs = new List<int>();
            if (GroupIDs == null)
                GroupIDs = new List<int>();
            if (HouseIDs?.Count > 0)
            for (int i = 0; i < HouseIDs.Count(); i++)
            {
                try
                {
                    queries.Add(
                        new RequestStatsQuerySettings
                        {
                            DistrictId = -1,//AreaIDs[i],
                            GroupOfDistrictId = -1,
                            HouseId = HouseIDs[i],
                        });
                }
                catch (Exception ex)
                { }
            }
            else if (AreaIDs?.Count > 0)
            {
                for (int i = 0; i < AreaIDs?.Count(); i++)
                {
                    try
                    {
                        queries.Add(
                            new RequestStatsQuerySettings
                            {
                                DistrictId = AreaIDs[i],
                                HouseId = -1,// HouseIDs[i],
                                GroupOfDistrictId = -1
                            });
                    }
                    catch (Exception ex)
                    { }
                }
            }
            else if (GroupIDs?.Count > 0)
            {
                for (int i = 0; i < GroupIDs?.Count(); i++)
                {
                    try
                    {
                        queries.Add(
                            new RequestStatsQuerySettings
                            {
                                DistrictId = -1,
                                HouseId = -1,// HouseIDs[i],
                                GroupOfDistrictId = GroupIDs[i]
                            });
                    }
                    catch (Exception ex)
                    { }
                }
            }
            else if (queries?.Count == 0)
            {
                queries.Add(new RequestStatsQuerySettings
                {
                    DistrictId = -1,
                    HouseId = -1,
                    GroupOfDistrictId = -1
                });
            }

            if (queries?.Count == 0)
            {
                queries.Add(new RequestStatsQuerySettings
                {
                    DistrictId = -1,
                    HouseId = -1,
                    GroupOfDistrictId = -1
                });
            }
                IsBusy = false;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Loading.Instance.StartAsync(async progress =>
                {
                    var result = await _server.GetMultipleStats(queries);
                    if (result.Error == null)
                    {
                        if (result.Data.Count > 0)
                        {
                            RequestStats sum = new RequestStats();
                            sum.TotalUnperformedRequestsList = new List<Requests>();
                            sum.Today = new PeriodStats();
                            sum.Today.OverdueRequestsList = new List<Requests>();
                            sum.Today.UnperformedRequestsList = new List<Requests>();
                            sum.Week = new PeriodStats();
                            sum.Week.OverdueRequestsList = new List<Requests>();
                            sum.Week.UnperformedRequestsList = new List<Requests>();
                            sum.Month = new PeriodStats();
                            sum.Month.OverdueRequestsList = new List<Requests>();
                            sum.Month.UnperformedRequestsList = new List<Requests>();
                            foreach (var monitor in result.Data)
                            {
                                sum.CustomPeriod = monitor.CustomPeriod;


                                sum.TotalUnperformedRequestsList.AddRange(monitor.TotalUnperformedRequestsList);
                                sum.Today.OverdueRequestsList.AddRange(monitor.Today.OverdueRequestsList);
                                sum.Today.RequestsCount += monitor.Today.RequestsCount;
                                sum.Today.UnperformedRequestsList.AddRange(monitor.Today.UnperformedRequestsList);

                                sum.Week.OverdueRequestsList.AddRange(monitor.Week.OverdueRequestsList);
                                sum.Week.RequestsCount += monitor.Week.RequestsCount;
                                sum.Week.UnperformedRequestsList.AddRange(monitor.Week.UnperformedRequestsList);

                                sum.Month.RequestsCount += monitor.Month.RequestsCount;
                                sum.Month.OverdueRequestsList.AddRange(monitor.Month.OverdueRequestsList);
                                sum.Month.UnperformedRequestsList.AddRange(monitor.Month.UnperformedRequestsList);
                            }

                            setMonitoring(sum);
                        }
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                });
            });
           
            
        }

        private void HouseGroups_SelectionChanged(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {
            if(!isRunning)
                return;
            try
            {
                if (AreaIDs == null)
                    AreaIDs = new List<int>();

                AreaIDs.Clear();

                foreach (var area in (IEnumerable<Object>)e.Value)
                {
                    AreaIDs.Add(((NamedValue)area).ID);
                }

            }
            catch
            {

            }
           

        }

        private void Houses_SelectionChanged(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {
            if(!isRunning)
                return;
            try
            {
                if (HouseIDs == null)
                    HouseIDs = new List<int>();

                HouseIDs.Clear();



                foreach (var house in (IEnumerable<Object>)e.Value)
                {
                    HouseIDs.Add(((HouseProfile)house).ID);
                }
            }
            catch
            {

            }
            
        }
        bool groupvisible;
        public bool GroupVisible
        {
            get => groupvisible;
            set
            {
                groupvisible = value;
                OnPropertyChanged("GroupVisible");
            }
        }

        bool areasvisible;
        public bool AreasVisible
        {
            get => areasvisible;
            set
            {
                areasvisible = value;
                OnPropertyChanged("AreasVisible");
            }
        }

        bool streetsvisible;
        public bool StreetsVisible
        {
            get => streetsvisible;
            set
            {
                streetsvisible = value;
                OnPropertyChanged("StreetsVisible");
            }
        }
        private void FoldAreaGroup(object sender, EventArgs args)
        {
            if(!isRunning)
                return;
            GroupVisible = !GroupVisible;
            AreaGroups.IsVisible = !GroupVisible;
        }

        private void FoldAreas(object sender, EventArgs args)
        {
            AreasVisible = !AreasVisible;
        }

        private void FoldStreets(object sender, EventArgs args)
        {
            StreetsVisible = !StreetsVisible;
        }

        private void AreaGroups_SelectionChanged(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {
            if(!isRunning)
                return;
            if (GroupIDs == null)
                GroupIDs = new List<int>();

            var select = (sender as SfAutoComplete).SelectedItem as IEnumerable<Object>;
            var selected_ids = select.Select(x => (x as NamedValue).ID).ToArray();
            GroupIDs.Clear();
            GroupIDs.AddRange(selected_ids);
        }

        private void HouseGroups_SelectionChanged_1(object sender, Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs e)
        {

        }

        private void HouseGroups_FocusChanged(object sender, FocusChangedEventArgs e)
        {
            // (sender as SfAutoComplete).Unfocus();
            if(!isRunning)
                return;
            try
            {
                if (Groups.Count > 0)
                {
                    var result = new List<NamedValue>();
                    if ((AreaGroups.SelectedItem != null))
                    {
                        if ((AreaGroups.SelectedItem as IEnumerable<Object>)?.Count() > 0)
                        {
                            if (AreaGroups.SelectedItem != null)
                            {
                                if ((AreaGroups.SelectedItem as IEnumerable<Object>)?.Count() > 0)
                                {
                                    var indecies = (AreaGroups.SelectedItem as IEnumerable<Object>).Select(x => (x as NamedValue).ID).ToList();
                                    result.AddRange(Areas.Where(x => indecies.Contains(x.Value)));
                                }
                                else
                                {
                                    result.AddRange(Areas);
                                }
                            }
                            else
                            {
                                result.AddRange(Areas);
                            }

                            var selected = HouseGroups.SelectedItem as IEnumerable<Object>;
                            if (selected != null)
                            {
                                var selected_indecies = selected.Select(x => (x as NamedValue).ID).ToList();
                                if (selected_indecies != null)
                                {
                                    result = result.Where(x => !selected_indecies.Contains(x.ID)).ToList();
                                }
                            }

                            HouseGroups.DataSource = null;
                            HouseGroups.DataSource = result;
                        }
                        else
                        {
                            result.AddRange(Areas);
                            var selected = HouseGroups.SelectedItem as IEnumerable<Object>;
                            if (selected != null)
                            {
                                var selected_indecies = selected.Select(x => (x as NamedValue).ID).ToList();
                                if (selected_indecies != null)
                                {
                                    result = result.Where(x => !selected_indecies.Contains(x.ID)).ToList();
                                }
                            }

                            HouseGroups.DataSource = null;
                            HouseGroups.DataSource = result;
                        }
                    }
                    
                    else
                    {
                        result.AddRange(Areas);
                        var selected = HouseGroups.SelectedItem as IEnumerable<Object>;
                        if (selected != null)
                        {
                            var selected_indecies = selected.Select(x => (x as NamedValue).ID).ToList();
                            if (selected_indecies != null)
                            {
                                result = result.Where(x => !selected_indecies.Contains(x.ID)).ToList();
                            }
                        }

                        HouseGroups.DataSource = null;
                        HouseGroups.DataSource = result;
                    }
                }
            }
            catch
            {

            }
            
        }

        private void HousesList_FocusChanged(object sender, FocusChangedEventArgs e)
        {
            // (sender as SfAutoComplete).Unfocus();
            if(!isRunning)
                return;
            try
            {
                var selected = HousesList.SelectedItem as IEnumerable<Object>;
                if (selected != null)
                {
                    var selected_indecies = selected.Select(x => (x as HouseProfile).ID).ToList();
                    var result = new List<HouseProfile>();

                    HousesList.DataSource = null;
                    HousesList.DataSource = Streets.Where(x => !selected_indecies.Contains(x.ID));
                }
                else
                {
                    HousesList.DataSource = null;
                    HousesList.DataSource = Streets;
                }
            }
            catch
            {

            }
            
        }
    }
}