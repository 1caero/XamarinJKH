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
                            //_ = Task.Run(() =>
                            //  {

                            //      Device.BeginInvokeOnMainThread(SetGrupAdditional);

                            //  });


                            SetGrupAdditional();


                            //Device.BeginInvokeOnMainThread(SetGrupAdditional);
                        }
                    }

                    IsRefreshing = false;
                });
            }
        }

        bool _busy;
        private ObservableCollection<AdditionalGroup> _additionalGroups;

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

            AdditionalGroups = new ObservableCollection<AdditionalGroup>();

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

            this.BindingContext = this;
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IsChangeTheme = !IsChangeTheme;
            // FrameKind.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
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

        int timerun = 0;
        void SetGrupAdditional_old()
        {
            // StackLayout containerData = new StackLayout();
            // containerData.HorizontalOptions = LayoutOptions.FillAndExpand;
            // containerData.VerticalOptions = LayoutOptions.Start;
           

           
            Device.BeginInvokeOnMainThread(() =>
            {
         try
            {
                timerun++;
                    var ok = new ObservableCollection<AdditionalGroup>();
                AdditionalGroups.Clear();

            if (Settings.EventBlockData.AdditionalServicesByGroups?.Keys != null)
                    foreach (var group in Settings.EventBlockData.AdditionalServicesByGroups?.Keys)
                    {
                        IEnumerable<AdditionalService> additionalServices = Settings.EventBlockData.AdditionalServicesByGroups[@group]
                              .Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать"));
                            
                            ok.Add(new AdditionalGroup(group, new List<AdditionalService>(additionalServices)));
                        //AdditionalGroups=ok;
                        //MainThread.BeginInvokeOnMainThread(()=>
                        //{
                        //var list = new List<AdditionalGroup>();
                        //foreach (var group in Settings.EventBlockData.AdditionalServicesByGroups?.Keys)
                        //{
                        //    IEnumerable<AdditionalService> additionalServices = Settings.EventBlockData.AdditionalServicesByGroups[@group]
                        //          .Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать"));

                        //    list.Add(new AdditionalGroup(group, new List<AdditionalService>(additionalServices)));
                        //   // AdditionalGroups.Add(new AdditionalGroup(group, new List<AdditionalService>(additionalServices)));
                        //}

                        //Device.BeginInvokeOnMainThread(() =>
                        //{
                        //    foreach (var v in list)
                        //        AdditionalGroups.Add(v);
                        //});



                        //});


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
                    AdditionalGroups = ok;
                    // StackLayoutContainer.Content = containerData;
                    //GropusColl.ItemsSource = AdditionalGroups;
                    IsBusy = false;
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }

        void SetGrupAdditional()
        {
            Device.BeginInvokeOnMainThread(() =>
            {

                allService.Children.Clear();

                if (Settings.EventBlockData.AdditionalServicesByGroups?.Keys != null)
                    foreach (var group in Settings.EventBlockData.AdditionalServicesByGroups?.Keys)
                    {
                        StackLayout containerData = new StackLayout();
                        containerData.HorizontalOptions = LayoutOptions.FillAndExpand;
                        containerData.VerticalOptions = LayoutOptions.Start;
                        containerData.Orientation = StackOrientation.Vertical;

                        var additionalServices = 
                        Settings.EventBlockData.AdditionalServicesByGroups[@group].
                        Where(x => !x.NotNullShowInAdBlock.ToLower().Equals("не отображать")).ToList();

                        Label titleLable = new Label();
                        titleLable.TextColor = Color.Black;
                        titleLable.FontSize = 18;
                        titleLable.Text = @group;
                        titleLable.FontAttributes = FontAttributes.Bold;
                        titleLable.VerticalOptions = LayoutOptions.StartAndExpand;
                        titleLable.HorizontalOptions = LayoutOptions.StartAndExpand;

                        StackLayout containerAdd = new StackLayout();
                        containerAdd.HorizontalOptions = LayoutOptions.FillAndExpand;
                        containerAdd.Orientation = StackOrientation.Horizontal;

                        int l = Convert.ToInt32(additionalServices.Count() / 4);
                        var lp = (double)additionalServices.Count() / 4;
                        if(lp-l>0)
                        {
                            l++;
                        }

                        Grid g = new Grid();
                        g.ColumnDefinitions.Add(new ColumnDefinition());
                        g.ColumnDefinitions.Add(new ColumnDefinition());
                        g.ColumnDefinitions.Add(new ColumnDefinition());
                        g.ColumnDefinitions.Add(new ColumnDefinition());

                        int j = 0;
                        for (int i=0; i < l; i++)
                        {
                            g.RowDefinitions.Add(new RowDefinition());
                            int col = 0;
                            for(int k=j; k < additionalServices.Count(); k++)
                            {
                                StackLayout stackLayoutCon = new StackLayout()
                                {
                                    Padding = 0,
                                    Margin=5
                                };
                                PancakeView pic = new PancakeView()
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    CornerRadius = 20,
                                };

                                CachedImage cachedImage = new CachedImage()
                                {
                                    HeightRequest = 65,
                                    WidthRequest = 65,
                                    Source = additionalServices[k].LogoLink
                                };
                                pic.Content = cachedImage;

                                Label labelText = new Label()
                                {
                                    LineBreakMode=LineBreakMode.WordWrap,
                                    TextColor = Color.Black,
                                    VerticalTextAlignment = TextAlignment.Center,
                                    HorizontalOptions = LayoutOptions.Center,
                                    FontSize = 12,
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    Text=additionalServices[k].FormatName
                                };

                                //if( additionalServices[k].FormatName.Contains("\n"))
                                //{
                                //    Console.WriteLine($"строка с N:{additionalServices[k].FormatName}");

                                //}

                                stackLayoutCon.Children.Add(new Label() { Text = additionalServices[k].ID.ToString(),IsVisible=false });
                                stackLayoutCon.Children.Add(pic);
                                stackLayoutCon.Children.Add(labelText);
                                containerAdd.Children.Add(stackLayoutCon);

                                var onItemTaped = new TapGestureRecognizer();
                                onItemTaped.Tapped += async (s, e) =>
                                {
                                    var id = Convert.ToInt32(((Label)((StackLayout)s).Children[0]).Text);
                                    var additional = additionalServices.First(_ => _.ID == id);

                                    if (additional.ShopID == null)
                                    {
                                        if (Navigation.NavigationStack.FirstOrDefault(x => x is AdditionalOnePage) == null)
                                            await Navigation.PushAsync(new AdditionalOnePage(additional));
                                    }
                                    else
                                    {
                                        if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                                            await Navigation.PushAsync(new ShopPageNew(additional));
                                    }
                                };
                                stackLayoutCon.GestureRecognizers.Add(onItemTaped);

                                Grid.SetRow(stackLayoutCon, i);
                                Grid.SetColumn(stackLayoutCon, col);

                                g.Children.Add(stackLayoutCon);
                                if (col == 3)
                                {
                                    j = k+1;
                                    break;
                                }
                                col++;
                                
                            }
                            //j++;
                        }

                        containerData.Children.Add(titleLable);
                        containerData.Children.Add(g);
                        allService.Children.Add(containerData);
                    }

                //StackLayoutContainer.Content = containerData;
            });
        }

        public ObservableCollection<AdditionalGroup> AdditionalGroups
        {
            get => _additionalGroups;
            set {
                if (value != null) _additionalGroups = value;
                OnPropertyChanged("AdditionalGroups");
                  }
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