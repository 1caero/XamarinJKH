using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using FFImageLoading.Forms;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Shop;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.Additional;
using Map = Xamarin.Forms.Maps.Map;

namespace xamarinJKH.Additional
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdditionalPage : ContentPage
    {
        public ObservableCollection<AdditionalService> Additional { get; set; }
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();

        string _selectedGroup;

        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
            }
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

        public ObservableCollection<string> Groups { get; set; }
        string _mainColor;

        public string MainColor
        {
            get => _mainColor;
            set
            {
                _mainColor = value;
                OnPropertyChanged(nameof(MainColor));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                            await DisplayAlert(AppResources.ErrorTitle, null, "OK"));
                        IsRefreshing = false;
                    }
                    else
                    {
                        if (Settings.EventBlockData.AdditionalServices != null)
                        {
                            var l = Settings.EventBlockData.AdditionalServices.Where(x =>
                                x.HasLogo && x.ShowInAdBlock != null &&
                                !x.ShowInAdBlock.ToLower().Equals("не отображать"));
                            var groups = l.GroupBy(x => x.Group).Select(x => x.First())
                                .Select(y => y.Group).ToList();

                            var groups1 = Settings.EventBlockData.AdditionalServices.GroupBy(x => x.Group)
                                .Select(x => x.First())
                                .Select(y => y.Group).ToList();
                            Additional.Clear();

                            foreach (var each in Settings.EventBlockData.AdditionalServices)
                            {
                                //try
                                //{
                                if (each.HasLogo)
                                    if (each.ShowInAdBlock != null)
                                        if (!each.ShowInAdBlock.ToLower().Equals("не отображать"))
                                        {
                                            if (SelectedGroup != null)
                                            {
                                                if (each.Group == SelectedGroup)
                                                {
                                                    Device.BeginInvokeOnMainThread(() => Additional.Add(each));
                                                }
                                            }
                                            else
                                            {
                                                Device.BeginInvokeOnMainThread(() => Additional.Add(each));
                                            }
                                        }
                            }
                        }
                    }

                    IsRefreshing = false;
                });
            }
        }

        bool _busy;

        public bool IsBusy
        {
            get => _busy;
            set
            {
                _busy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        private async Task RefreshData()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            Settings.EventBlockData = await server.GetEventBlockData();
            if (Settings.EventBlockData.Error == null)
            {
                SetAdditional();
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAdditional, "OK");
            }
        }

        public AdditionalPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Доп услуги");
            NavigationPage.SetHasNavigationBar(this, false);
            Map.BindingContext = new MapPageViewModel();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

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

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch
                {
                }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);

            MainColor = "#" + Settings.MobileSettings.color;
            Additional = new ObservableCollection<AdditionalService>();
            Groups = new ObservableCollection<string>();
            this.BindingContext = this;
            MessagingCenter.Subscribe<Object>(this, "LoadGoods", async (s) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                SetAdditional();
            });

            MessagingCenter.Subscribe<MapPageViewModel, Position>(this, "FocusMap",
                (sender, args) =>
                {
                    (Map.Children[0] as Map).MoveToRegion(
                        MapSpan.FromCenterAndRadius(args, Distance.FromKilometers(2)));
                });

            MessagingCenter.Subscribe<Object, AdditionalService>(this, "OpenService", async (sender, args) =>
            {
                var select = args;
                if (select.ShopID == null)
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                        await Navigation.PushAsync(new AdditionalOnePage(select));
                }
                else
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                        await Navigation.PushAsync(new ShopPageNew(select));
                }
            });

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
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameKind.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SetText();
        }

        void SetAdditional()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, null, "OK"));
                return;
            }

            IsBusy = true;
            Groups.Clear();
            Additional.Clear();
            Device.BeginInvokeOnMainThread(() =>
                Task.Run(() =>
                {
                    if (Settings.EventBlockData.AdditionalServices != null)
                    {
                        var l = Settings.EventBlockData.AdditionalServices.Where(x =>
                            x.HasLogo && x.ShowInAdBlock != null && !x.ShowInAdBlock.ToLower().Equals("не отображать"));
                        var groups = l.GroupBy(x => x.Group).Select(x => x.First())
                            .Select(y => y.Group).ToList();

                        var groups1 = Settings.EventBlockData.AdditionalServices.GroupBy(x => x.Group)
                            .Select(x => x.First())
                            .Select(y => y.Group).ToList();

                        foreach (var group in groups)
                        {
                            Groups.Add(group);
                        }

                        foreach (var each in Settings.EventBlockData.AdditionalServices)
                        {
                            //try
                            //{
                            if (each.HasLogo)
                                if (each.ShowInAdBlock != null)
                                    if (!each.ShowInAdBlock.ToLower().Equals("не отображать"))
                                    {
                                        Additional.Add(each);
                                    }
                        }


                        if (groups.Count > 0)
                        {
                            SelectedGroup = null;
                        }
                    }

                    Device.BeginInvokeOnMainThread(SetGrupAdditional);

                    IsBusy = false;
                })
            );

        }


        void SetGrupAdditional()
        {
            StackLayout containerData = new StackLayout();
            containerData.HorizontalOptions = LayoutOptions.FillAndExpand;
            containerData.VerticalOptions = LayoutOptions.Start;
            foreach (var group in Groups)
            {
                
                Label titleLable = new Label();
                titleLable.TextColor = Color.Black;
                titleLable.FontSize = 18;
                titleLable.Text = group;
                titleLable.FontAttributes = FontAttributes.Bold;
                titleLable.VerticalOptions = LayoutOptions.StartAndExpand;
                titleLable.HorizontalOptions = LayoutOptions.StartAndExpand;

                ScrollView scrollViewAdditional = new ScrollView();
                scrollViewAdditional.Orientation = ScrollOrientation.Horizontal;
                scrollViewAdditional.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
                StackLayout containerAdd = new StackLayout();
                containerAdd.HorizontalOptions = LayoutOptions.FillAndExpand;
                containerAdd.Orientation = StackOrientation.Horizontal;
                foreach (var service in Settings.EventBlockData.AdditionalServices.Where(x => x.Group == group))
                {
                   
                    StackLayout stackLayoutCon = new StackLayout()
                    {
                        Padding = 10
                    };
                    PancakeView pic = new PancakeView()
                    {
                        HorizontalOptions=LayoutOptions.Center,
                        CornerRadius=20,
                        
                    };
                    
                    CachedImage cachedImage = new CachedImage()
                    {
                        HeightRequest=70,
                        WidthRequest=70,
                        Source = service.LogoLink
                    };
                    pic.Content = cachedImage;

                    Label labelText = new Label()
                    {
                        TextColor = Color.Black,
                        Text = FormatName(service.Name),//.Replace("\\n","\n"),
                        VerticalTextAlignment=TextAlignment.Center,
                        HorizontalOptions=LayoutOptions.Center,
                        FontSize=12,
                        HorizontalTextAlignment=TextAlignment.Center
                    };
                    stackLayoutCon.Children.Add(pic);
                    stackLayoutCon.Children.Add(labelText);
                    containerAdd.Children.Add(stackLayoutCon);
                    
                    var onItemTaped = new TapGestureRecognizer();
                    onItemTaped.Tapped += async (s, e) =>
                    {
                        if (service.ShopID == null)
                        {
                            if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                                await Navigation.PushAsync(new AdditionalOnePage(service));
                        }
                        else
                        {
                            if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                                await Navigation.PushAsync(new ShopPageNew(service));
                        }
                    };
                    stackLayoutCon.GestureRecognizers.Add(onItemTaped);
                    
                }

                scrollViewAdditional.Content = containerAdd;
                containerData.Children.Add(titleLable);
                containerData.Children.Add(scrollViewAdditional);
            }

            StackLayoutContainer.Content = containerData;
        }

        string FormatName(string input)
        {
            if (input.Contains("\\n"))
            {
                return input.Replace("\\n", "\n");
            }

            else
            {
                return input.Replace(" ", "\n");
            }
        }
        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            AdditionalService select = e.CurrentSelection[0] as AdditionalService;
            if (select.ShopID == null)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                    await Navigation.PushAsync(new AdditionalOnePage(select));
            }
            else
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                    await Navigation.PushAsync(new ShopPageNew(select));
            }
        }

        private void GroupChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string group = e.CurrentSelection[0] as string;
                Additional.Clear();
                foreach (var service in Settings.EventBlockData.AdditionalServices.Where(x => x.Group == group))
                {
                    if (service.HasLogo && service.ShowInAdBlock != null)
                        if (!service.ShowInAdBlock.ToLower().Equals("не отображать"))
                            Additional.Add(service);
                }
            }
            catch
            {
            }
        }


        private async void Pin_Clicked(object sender, EventArgs e)
        {
            var shop = Additional.FirstOrDefault(x =>
                x.ID == Convert.ToInt32((sender as Pin).ClassId));
            if (shop != null)
            {
                await Dialog.Instance.ShowAsync(new MapShopDialogView(shop));
            }
        }

    }
}