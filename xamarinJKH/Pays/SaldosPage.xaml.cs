using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.CustomRenderers;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.Utils.Compatator;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SaldosPage : ContentPage
    {
        public List<BillInfo> BillInfos { get; set; }
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();
        private bool isSortDate = false;
        private bool isSortLs = true;
        public List<AccountAccountingInfo> Accounts { get; set; }
        public AccountAccountingInfo SelectedAcc { get; set; }
        private Color hex { get; set; } = (Color) Application.Current.Resources["MainColor"];

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
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            ItemsList<AccountAccountingInfo> info = await _server.GetAccountingInfo();
            if (info.Error == null)
            {
                SetBills(info.Data);
                isSortDate = !isSortDate;
                SortDate();
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorInfoBills, "OK");
            }
        }

        //List<AccountAccountingInfo> GetIdent(List<AccountAccountingInfo> infos)
        //{
        //    List<AccountAccountingInfo> result = new List<AccountAccountingInfo>();
        //    var listStr = infos.Select(n => n.Ident.ToString()).ToHashSet();
        //    foreach (var each in listStr)
        //    {
        //        result.Add(infos.Find(x => x.Ident==each));
        //    }
        //    return result;
        //}
        
        public SaldosPage(List<AccountAccountingInfo> infos)
        {            
            InitializeComponent();
            Analytics.TrackEvent("Квитанции");

            if(infos==null)
            {                
                Analytics.TrackEvent($"infos null:{infos == null}") ;
                
            }
            SetBills(infos);
            if (infos != null)
                Accounts = new List<AccountAccountingInfo>(infos);
            else
                Accounts = new List<AccountAccountingInfo>();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        kvitLabel2.FontSize = 14;
                        LabelDate.FontSize = 12;
                        LabelLs.FontSize = 12;
                    }

                    //хак чтобы список растягивался на все необходимое пространоство. а так
                    //есть баг в xamarin, потому что fillAndExpand не работает(https://github.com/xamarin/Xamarin.Forms/issues/6908)
                    additionalList.HeightRequest = 3000;

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

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);

            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var sortDate = new TapGestureRecognizer();
            sortDate.Tapped += async (s, e) => { SortDate(); };
            StackLayoutSortDate.GestureRecognizers.Add(sortDate);
            var sortLs = new TapGestureRecognizer();
            sortLs.Tapped += async (s, e) => { SortLs(); };
            StackLayoutSortIdent.GestureRecognizers.Add(sortLs);
            var pickLs = new TapGestureRecognizer();
            pickLs.Tapped += async (s, e) => { Device.BeginInvokeOnMainThread(() => { Picker.Focus(); }); };
            StackLayoutLs.GestureRecognizers.Add(pickLs);
            SetText();
            this.BindingContext = this;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            SortDate();
          
            
        }

        void SortDate()
        {
            List<BillInfo> listTop = new List<BillInfo>();
            List<BillInfo> listBottom = new List<BillInfo>();
            foreach (var each in BillInfos)
            {
                if (!string.IsNullOrWhiteSpace(each.Period) && each.Period.Split().Length > 1)
                {
                    listTop.Add(each);
                }
                else
                {
                    listBottom.Add(each);
                }
            }

            BillinfoComarable comarable = new BillinfoComarable();

            IconViewSortDate.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", hex.ToHex()}
            };
            LabelDate.TextColor = hex;
            Color fromHex = Color.FromHex("#8B8B8B");
            IconViewSortIdent.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", "#8B8B8B"}
            };
            LabelLs.TextColor = fromHex;
            if (isSortDate)
            {
                IconViewSortDate.Rotation = 0;
                listTop.Sort(comarable);
                var list = listBottom.OrderBy(u => u.Period);
                listBottom = new List<BillInfo>(list);
                isSortDate = false;
            }
            else
            {
                isSortDate = true;
                IconViewSortDate.Rotation = 180;
                listTop.Sort(comarable);
                listTop.Reverse();
                var list = listBottom.OrderByDescending(u => u.Ident);
                listBottom = new List<BillInfo>(list);
            }

            BillInfos.Clear();
            BillInfos.AddRange(listTop);
            BillInfos.AddRange(listBottom);
            additionalList.ItemsSource = null;
            if (SelectedAcc != null)
            {
                additionalList.ItemsSource = from i in BillInfos where i.Ident.Equals(SelectedAcc.Ident) select i;
            }
            else
            {
                additionalList.ItemsSource = BillInfos;
            }
        }

        private void SortLs()
        {
            Color fromHex = Color.FromHex("#8B8B8B");
            IconViewSortDate.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", "#8B8B8B"}
            };
            LabelDate.TextColor = fromHex;

            IconViewSortIdent.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", $"#{Settings.MobileSettings.color}"}
            };
            LabelLs.TextColor = hex;
            if (!isSortLs)
            {
                IconViewSortIdent.Rotation = 0;
                var list = BillInfos.OrderBy(u => u.Ident);
                BillInfos = new List<BillInfo>(list);
                isSortLs = true;
            }
            else
            {
                isSortLs = false;
                IconViewSortIdent.Rotation = 180;
                var list = BillInfos.OrderByDescending(u => u.Ident);
                BillInfos = new List<BillInfo>(list);
            }

            additionalList.ItemsSource = null;
            additionalList.ItemsSource = BillInfos;
        }

        void SetBills(List<AccountAccountingInfo> infos)
        {
            BillInfos = new List<BillInfo>();
            if (infos == null)
            {
                return;
            }

            foreach (var each in infos)
            {
                if (each.Bills == null)
                {
                    continue;
                }

                foreach (var VARIABLE in each.Bills)
                {
                    BillInfos.Add(VARIABLE);
                }
            }

            var list = BillInfos.OrderByDescending(u => u.Period);

            BillInfos = new List<BillInfo>(list);
        }

        void SortNotFormat()
        {
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;


            IconViewSortDate.ReplaceStringMap = new Dictionary<string, string>
            {
                {"#000000", $"#{Settings.MobileSettings.color}"}
            };
            LabelDate.TextColor = hex;

            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameSaldo.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            BillInfo select = e.Item as BillInfo;
            if (!select.HasFile)
            {
                return;
            }

            try
            {
                Analytics.TrackEvent("Открытие квитанции " + select.Period + " " + select.Ident);
                string filename = @select.Period + select.Ident.Replace("/", "").Replace("\\", "") + ".jpg";
                await Navigation.PushAsync(new ImageSaldoPage( select));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async void GetFile(string id, string fileName, BillInfo period)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };
            byte[] stream = null;

            bool result = false;
            if (Device.RuntimePlatform == "iOS")
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Loading.Instance.StartAsync(async progress =>
                    {
                        try
                        {
                            byte[] stream;
                            stream = await server.DownloadFileAsync(id, 1);
                            if (stream != null)
                            {
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                  
                                });
                            }
                            else
                            {
                                await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                                result = false;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                            result = false;
                        }
                    });
                });
            }
            else
            {
                await Loading.Instance.StartAsync(async progress =>
                {
                    try
                    {
                        period.stream = await server.DownloadFileAsync(id, 1);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                        result = false;
                    }
                });
              
            }
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            additionalList.ItemsSource = null;
            additionalList.ItemsSource = from i in BillInfos where i.Ident.Equals(SelectedAcc.Ident) select i;
        }
    }
}