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
                        Settings.EventBlockData = await server.GetEventBlockData();
                        if (Settings.EventBlockData.AdditionalServicesByGroups != null)
                        {
                            SetGrupAdditional();
                        }
                    }

                    IsRefreshing = false;
                });
            }
        }

        bool _busy;
        private ObservableCollection<AdditionalGroup> _additionalGroups = new ObservableCollection<AdditionalGroup>();

        public bool IsBusy
        {
            get => _busy;
            set
            {
                _busy = value;
                OnPropertyChanged("IsBusy");
            }
        }
        public AdditionalPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Доп услуги");
            NavigationPage.SetHasNavigationBar(this, false);
            // Map.BindingContext = new MapPageViewModel();

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
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);

            MainColor = "#" + Settings.MobileSettings.color;
            Additional = new ObservableCollection<AdditionalService>();
            Groups = new ObservableCollection<string>();
            this.BindingContext = this;
            MessagingCenter.Subscribe<Object>(this, "LoadGoods", async (s) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                RefreshCommand.Execute(null);
            });

            // MessagingCenter.Subscribe<MapPageViewModel, Position>(this, "FocusMap",
            //     (sender, args) =>
            //     {
            //         (Map.Children[0] as Map).MoveToRegion(
            //             MapSpan.FromCenterAndRadius(args, Distance.FromKilometers(2)));
            //     });

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
                    if (Settings.EventBlockData.AdditionalServicesByGroups != null)
                    {
                        List<string> groups = new List<string>(Settings.EventBlockData.AdditionalServicesByGroups.Keys);
                        if (groups.Count > 0)
                        {
                            SelectedGroup = null;
                        }
                    }
                    Device.BeginInvokeOnMainThread(SetGrupAdditional);
                    
                })
            );

        }


        void SetGrupAdditional()
        {
            // StackLayout containerData = new StackLayout();
            // containerData.HorizontalOptions = LayoutOptions.FillAndExpand;
            // containerData.VerticalOptions = LayoutOptions.Start;
            Device.BeginInvokeOnMainThread(() =>
            {
                AdditionalGroups.Clear();
                if (Settings.EventBlockData.AdditionalServicesByGroups?.Keys != null)
                    foreach (var group in Settings.EventBlockData.AdditionalServicesByGroups?.Keys)
                    {
                        IEnumerable<AdditionalService> additionalServices = Settings.EventBlockData.AdditionalServicesByGroups[@group]
                              .Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать"));
                        AdditionalGroups.Add(new AdditionalGroup(group,new List<AdditionalService>(additionalServices)));
                       
                        
                        // Label titleLable = new Label();
                        // titleLable.TextColor = Color.Black;
                        // titleLable.FontSize = 18;
                        // titleLable.Text = @group;
                        // titleLable.FontAttributes = FontAttributes.Bold;
                        // titleLable.VerticalOptions = LayoutOptions.StartAndExpand;
                        // titleLable.HorizontalOptions = LayoutOptions.StartAndExpand;
                        //
                        // ScrollView scrollViewAdditional = new ScrollView();
                        // scrollViewAdditional.Orientation = ScrollOrientation.Horizontal;
                        // scrollViewAdditional.HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
                        // StackLayout containerAdd = new StackLayout();
                        // containerAdd.HorizontalOptions = LayoutOptions.FillAndExpand;
                        // containerAdd.Orientation = StackOrientation.Horizontal;
                        // // foreach (var service in Settings.EventBlockData.AdditionalServicesByGroups[@group]
                        // //     .Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать")))
                        // // {
                        // //     StackLayout stackLayoutCon = new StackLayout()
                        // //     {
                        // //         Padding = 10
                        // //     };
                        // //     PancakeView pic = new PancakeView()
                        // //     {
                        // //         HorizontalOptions = LayoutOptions.Center,
                        // //         CornerRadius = 20,
                        // //     };
                        // //
                        // //     CachedImage cachedImage = new CachedImage()
                        // //     {
                        // //         HeightRequest = 70,
                        // //         WidthRequest = 70,
                        // //         Source = service.LogoLink
                        // //     };
                        // //     pic.Content = cachedImage;
                        // //
                        // //     Label labelText = new Label()
                        // //     {
                        // //         TextColor = Color.Black,
                        // //         VerticalTextAlignment = TextAlignment.Center,
                        // //         HorizontalOptions = LayoutOptions.Center,
                        // //         FontSize = 12,
                        // //         HorizontalTextAlignment = TextAlignment.Center
                        // //     };
                        // //     stackLayoutCon.Children.Add(pic);
                        // //     stackLayoutCon.Children.Add(labelText);
                        // //     containerAdd.Children.Add(stackLayoutCon);
                        // //
                        // //     var onItemTaped = new TapGestureRecognizer();
                        // //     onItemTaped.Tapped += async (s, e) =>
                        // //     {
                        // //         if (service.ShopID == null)
                        // //         {
                        // //             if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                        // //                 await Navigation.PushAsync(new AdditionalOnePage(service));
                        // //         }
                        // //         else
                        // //         {
                        // //             if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                        // //                 await Navigation.PushAsync(new ShopPageNew(service));
                        // //         }
                        // //     };
                        // //     stackLayoutCon.GestureRecognizers.Add(onItemTaped);
                        // // }
                        //
                        // IEnumerable<AdditionalService> additionalServices = Settings.EventBlockData.AdditionalServicesByGroups[@group]
                        //     .Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать"));
                        // CollectionView collectionView = new CollectionView
                        // {
                        //     ItemsLayout = new GridItemsLayout(4,ItemsLayoutOrientation.Vertical),
                        //     ItemsSource = new ObservableCollection<AdditionalService>(additionalServices) ,
                        //     ItemTemplate = new DataTemplate(() =>
                        //     {
                        //          StackLayout stackLayoutCon = new StackLayout()
                        //     {
                        //         Padding = 10
                        //     };
                        //     PancakeView pic = new PancakeView()
                        //     {
                        //         HorizontalOptions = LayoutOptions.Center,
                        //         CornerRadius = 20,
                        //     };
                        //
                        //     CachedImage cachedImage = new CachedImage()
                        //     {
                        //         HeightRequest = 70,
                        //         WidthRequest = 70,
                        //     };
                        //     cachedImage.SetBinding(CachedImage.SourceProperty, "LogoLink");
                        //     pic.Content = cachedImage;
                        //
                        //     Label labelText = new Label()
                        //     {
                        //         TextColor = Color.Black,
                        //         VerticalTextAlignment = TextAlignment.Center,
                        //         HorizontalOptions = LayoutOptions.Center,
                        //         FontSize = 12,
                        //         HorizontalTextAlignment = TextAlignment.Center
                        //     };
                        //     labelText.SetBinding(Label.TextProperty, "FormatName");
                        //     stackLayoutCon.Children.Add(pic);
                        //     stackLayoutCon.Children.Add(labelText);
                        //
                        //     // var onItemTaped = new TapGestureRecognizer();
                        //     // onItemTaped.Tapped += async (s, e) =>
                        //     // {
                        //     //     if (service.ShopID == null)
                        //     //     {
                        //     //         if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                        //     //             await Navigation.PushAsync(new AdditionalOnePage(service));
                        //     //     }
                        //     //     else
                        //     //     {
                        //     //         if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                        //     //             await Navigation.PushAsync(new ShopPageNew(service));
                        //     //     }
                        //     // };
                        //     // stackLayoutCon.GestureRecognizers.Add(onItemTaped);
                        //         return stackLayoutCon;
                        //     })
                        // };
                        //
                        // // scrollViewAdditional.Content = containerAdd;
                        // containerData.Children.Add(titleLable);
                        // containerData.Children.Add(collectionView);
                    }

                // StackLayoutContainer.Content = containerData;
                IsBusy = false;
            });
        }

        public ObservableCollection<AdditionalGroup> AdditionalGroups
        {
            get => _additionalGroups;
            set => _additionalGroups = value;
        }

        public class AdditionalGroup : List<AdditionalService>
        {
            public string Name { get; private set; }

            public AdditionalGroup(string name, List<AdditionalService> additional) : base(additional)
            {
                Name = name;
            }
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            AdditionalService service = (AdditionalService) e.CurrentSelection?.First();
            if (service?.ShopID == null)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                    await Navigation.PushAsync(new AdditionalOnePage(service));
            }
            else
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                    await Navigation.PushAsync(new ShopPageNew(service));
            }

        }
    }
}