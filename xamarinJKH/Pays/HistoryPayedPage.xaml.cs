using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
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
    public partial class HistoryPayedPage : ContentPage
    {
        public ObservableCollection<AccountAccountingInfo> Accounts { get; set; }
        public List<PaymentInfo> Payments { get; set; }
        public AccountAccountingInfo SelectedAcc { get; set; }

        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;

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
                Device.BeginInvokeOnMainThread(() =>
                {
                    var ident = string.Empty;
                    if (SelectedAcc != null)
                        ident = SelectedAcc.Ident;
                    Accounts.Clear();

                    //foreach (var acc in info.Data)
                    //{
                    //    Accounts.Add(acc);
                    //}
                    Accounts = new ObservableCollection<AccountAccountingInfo>(info.Data);
                    AccountsCollection.ItemsSource = Accounts;
                    //Accounts = ao;


                    additionalList.ItemsSource = null;
                    if (SelectedAcc == null || !string.IsNullOrEmpty(ident))
                        SelectedAcc = Accounts.FirstOrDefault(x => x.Ident == ident);
                    if (SelectedAcc == null)
                        SelectedAcc = Accounts[0];
                    additionalList.ItemsSource = setPays(Accounts[Accounts.IndexOf(Accounts.First(x => x.Ident == SelectedAcc.Ident))]);
                    foreach (var account in Accounts)
                    {
                        account.Selected = false;
                    }
                    SelectedAcc.Selected = true;
                    Accounts[Accounts.IndexOf(Accounts.First(x => x.Ident == SelectedAcc.Ident))].Selected = true;
                });
                
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorPayInfo, "OK");
            }
        }

        List<AccountAccountingInfo> GetIdent(List<AccountAccountingInfo> infos)
        {
            List<AccountAccountingInfo> result = new List<AccountAccountingInfo>();
            var listStr = infos.Select(n => n.Ident.ToString()).ToHashSet();
            foreach (var each in listStr)
            {
                AccountAccountingInfo res = new AccountAccountingInfo();
                int i = 0;
                foreach (var acc in infos.Where(x => x.Ident == each))
                {
                    if (i == 0)
                        res = acc;
                    else
                    {
                        res.Payments.AddRange(acc.Payments);
                        res.PendingPayments.AddRange(acc.PendingPayments);
                        res.MobilePayments.AddRange(acc.MobilePayments);
                    }
                    i++;
                }
                result.Add(res);
            }

            return result;
        }

        public HistoryPayedPage(List<AccountAccountingInfo> accounts)
        {
            var accounts_ = GetIdent(accounts);
            this.Accounts = new ObservableCollection<AccountAccountingInfo>();
            foreach(var acc in accounts_)
            {
                this.Accounts.Add(acc);
            } 
            InitializeComponent();
            Analytics.TrackEvent("История платежей");
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);

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
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
                        phoneDialer.MakePhoneCall(
                            Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }
            };
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var pickLs = new TapGestureRecognizer();
            pickLs.Tapped += async (s, e) => { Device.BeginInvokeOnMainThread(() => { Picker.Focus(); }); };
            StackLayoutLs.GestureRecognizers.Add(pickLs);
            SetText();
            Payments = setPays(Accounts[0]);
            SelectedAcc = Accounts[0];
            BindingContext = this;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));

            MessagingCenter.Subscribe<Object>(this, "ChangeTheme", (sender) =>
            {
                if (Accounts != null)
                {
                    foreach (var account in Accounts)
                        account.Selected = false;

                    Accounts[Accounts.IndexOf(Accounts.First(x => x.Ident == SelectedAcc.Ident))].Selected = true;
                }
                var theme = App.Current.RequestedTheme;
                var color = theme == OSAppTheme.Light ? ((Color)Application.Current.Resources["MainColor"]).ToHex() : "#FFFFFF";
                Dictionary<string, string> replace = new Dictionary<string, string> { { "#000000", color } };
                ArrowBack.ReplaceStringMap = replace;
            });
        }


        List<PaymentInfo> setPays(AccountAccountingInfo accountingInfo)
        {
            List<PaymentInfo> paymentInfo = new List<PaymentInfo>(accountingInfo.Payments);
            List<PaymentInfo> paymentUO = new List<PaymentInfo>(accountingInfo.PendingPayments);
            List<MobilePayment> mobile = new List<MobilePayment>(accountingInfo.MobilePayments);
            paymentUO = paymentUO.Select(c =>
            {
                c.HasCheck = false;
                return c;
            }).ToList();
            paymentInfo.AddRange(paymentUO);
            foreach (var each in mobile)
            {
                paymentInfo.Add(new PaymentInfo()
                {
                    Date = each.Date.Split(' ')[0],
                    Ident = each.Ident,
                    Period = "Мобильный",
                    HasCheck = false,
                    Sum = each.Sum
                });
            }

            HistoryPayComparable comparable = new HistoryPayComparable();

            paymentInfo.Sort(comparable);
            paymentInfo.Reverse();
            return paymentInfo;
        }


        string period(string date)
        {
            string[] split = date.Split(' ')[0].Split('.');
            int month = Int32.Parse(split[0]);
            if (month > 0)
                return Settings.months[month - 1] + " " + split[2];
            return "";
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;


            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameSaldo.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.FromHex("#494949"));
            FrameHistory.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            return;
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var acc in Accounts)
            {
                acc.Selected = false;
            }

            var selection = e.CurrentSelection[0] as AccountAccountingInfo;
            Accounts[Accounts.IndexOf(Accounts.First(x => x.Ident == selection.Ident))].Selected = true;

            SelectedAcc = new AccountAccountingInfo();
            SelectedAcc = Accounts[Accounts.IndexOf(Accounts.First(x => x.Ident == selection.Ident))];
            SelectedAcc.Selected = true;

            additionalList.ItemsSource = setPays(selection);
            Analytics.TrackEvent("Смена лс на " + selection.Ident);
        }
    }
}