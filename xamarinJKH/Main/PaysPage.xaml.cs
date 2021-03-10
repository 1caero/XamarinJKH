using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using FFImageLoading.Svg.Forms;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaysPage : ContentPage
    {
        public List<AccountAccountingInfo> _accountingInfo { get; set; }
        public static List<AccountAccountingInfo> _accountingInfos { get; set; }
        public Color hex { get; set; }
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();

        public bool forSvg { get; set; }

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

                    await RefreshPaysData();

                    IsRefreshing = false;
                });
            }
        }

        public async Task RefreshPaysData()
        {
            getInfo();
            
        }

        bool OnTimerTick()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (IsRefreshing || IsBusy)
                {
                    IsRefreshing = false;
                    IsBusy = false;
                }
            });
            return true;
        }


        public PaysPage()
        {
            InitializeComponent();

            forSvg = true;

            Analytics.TrackEvent("Оплата");
            Settings.mainPage = this;
            NavigationPage.SetHasNavigationBar(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        LabelHistory.FontSize = 10;
                        LabelSaldos.FontSize = 10;
                    }

                    IconViewSaldos.Margin = new Thickness(0, 0, 5, 0);
                    break;
                default:
                    break;
            }

            hex = (Color) Application.Current.Resources["MainColor"];

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);


            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
                        phoneDialer.MakePhoneCall(Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }
            };
            SetTextAndColor();

            var goAddIdent = new TapGestureRecognizer();
            goAddIdent.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddIdent) == null)
                    await Navigation.PushAsync(new AddIdent(this));
            };
            FrameAddIdent.GestureRecognizers.Add(goAddIdent);
            var openSaldos = new TapGestureRecognizer();
            openSaldos.Tapped += async (s, e) => { if (Navigation.NavigationStack.FirstOrDefault(x => x is SaldosPage) == null) await Navigation.PushAsync(new SaldosPage(_accountingInfo)); };
            FrameBtnSaldos.GestureRecognizers.Add(openSaldos);
            MessagingCenter.Subscribe<Object>(this, "UpdateIdent", (sender) => SyncSetup());

            MessagingCenter.Subscribe<Object>(this, "StartRefresh", (sender) =>
            { Device.BeginInvokeOnMainThread(() => aIndicator.IsVisible = true); });
            MessagingCenter.Subscribe<Object>(this, "EndRefresh", (sender) =>
            { Device.BeginInvokeOnMainThread(() => aIndicator.IsVisible = false); });

            PaysPageViewModel(this.baseForPays, _accountingInfo);
            BindingContext = this;
            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) =>
            {

                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                var colors = new Dictionary<string, string>();
                var arrowcolor = new Dictionary<string, string>();
                if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
                {
                    colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                    arrowcolor.Add("#000000", "#494949");
                }
                else
                {
                    colors.Add("#000000", "#FFFFFF");
                    arrowcolor.Add("#000000", "#FFFFFF");
                }
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Settings.mainPage = this;
            
            await SyncSetup(); // This could be an await'd task if need be
           
        }

        async Task SyncSetup()
        {
           
            Device.BeginInvokeOnMainThread(async () =>
            {
                aIndicator.IsVisible = true;
                // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
                await InfoForUpd();
                aIndicator.IsVisible = false;
            });
        }

        void SetTextAndColor()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            

            FrameBtnHistory.BorderColor = hex;
            FrameBtnSaldos.BorderColor = hex;
            LabelSaldos.TextColor = hex;
            LabelHistory.TextColor = hex;

            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            FrameAddIdent.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#494949"));

            if (Settings.Person != null)
                if (Settings.Person.Accounts != null)
                    if (Settings.Person.Accounts.Count == 0)
                    {
                        AccExistsLbl.IsVisible = true;
                    }    
        }

        async void getInfo()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            IsBusy = true;
            IsRefreshing = true;
            ItemsList<AccountAccountingInfo> info = await _server.GetAccountingInfo();
            IsBusy = false;
            if (info.Error == null)
            {
                _accountingInfo = info.Data;
                _accountingInfos = _accountingInfo;
                /*viewModel.*/
                LoadAccounts.Execute(info.Data);
            }
            else
            {
                IsRefreshing = false;
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCountersNoData, "OK");
            }
        }


        async Task InfoForUpd()
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
                _accountingInfo = info.Data;
                _accountingInfos = _accountingInfo;
                /*viewModel.*/
                LoadAccounts.Execute(info.Data);
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCountersNoData, "OK");
            }
        }

        private async void openSaldo(object sender, EventArgs e)
        {
            if (_accountingInfo != null && _accountingInfo.Count > 0)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is SaldosPage) == null)
                    await Navigation.PushAsync(new SaldosPage(_accountingInfo));
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoIdent, "OK");
            }
        }

        private async void OpenHistory(object sender, EventArgs e)
        {
            if (_accountingInfo != null && _accountingInfo.Count > 0)
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is HistoryPayedPage) == null)
                    await Navigation.PushAsync(new HistoryPayedPage(_accountingInfo));
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAppsNoIdent, "OK");
            }
        }

        public async void DellLs(string Ident)
        {
            bool answer = await Settings.mainPage.DisplayAlert(AppResources.Delete,
                $"{AppResources.DeleteIdent} " + Ident + " ?",
                "Да", "Отмена");
            if (answer)
            {
                await DellIdent(Ident);
                int numberIndex = Settings.Person.Accounts.FindIndex(0 , 1 ,x => x.Ident == Ident);
                if (numberIndex >= 0)
                {
                    Settings.Person.Accounts.Remove(Settings.Person.Accounts[numberIndex]);
                }
            }
        }

        public async Task DellIdent(string ident)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.DeletingIdent,
            };
            RestClientMP server = new RestClientMP();
            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                await DellIdentTask(ident, server);
            });
        }

        private async Task DellIdentTask(string ident, RestClientMP server)
        {
            CommonResult result = await server.DellIdent(ident);
            if (result.Error == null)
            {
                // Settings.EventBlockData = await server.GetEventBlockData();
                ItemsList<RequestType> resultN = await server.GetRequestsTypes();
                Settings.TypeApp = resultN.Data;
                /*viewModel.*/
                RemoveAccount.Execute(ident); //removeLs(ident);


                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsRefreshing = true;

                    await RefreshPaysData();

                    IsRefreshing = false;
                });

                MessagingCenter.Send<Object, AccountInfo>(this, "RemoveIdent", Settings.Person.Accounts.Where(x => x.Ident == ident).FirstOrDefault());
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, result.Error, "ОК");
            }
        }

        public List<AccountAccountingInfo> Accounts { get; set; }

        public Command LoadAccounts { get; set; }

        public Command RemoveAccount
        {
            get => new Command<string>(ident =>
            {
                var account_to_delete = Accounts.FirstOrDefault(x => x.Ident == ident);
                if (account_to_delete != null)
                {
                    Accounts.Remove(account_to_delete);

                    //OnPropertyChanged("Accounts");
                }
            });
        }

        public void PaysPageViewModel(StackLayout baseForPays, List<AccountAccountingInfo> _accountingInfo)
        {
            hex = (Color) Application.Current.Resources["MainColor"];
            Accounts = new List<AccountAccountingInfo>();
            LoadAccounts = new Command<List<AccountAccountingInfo>>(async (accounts) =>
            {
                Accounts = new List<AccountAccountingInfo>();
                //Accounts.Clear();
                Device.BeginInvokeOnMainThread(() => { baseForPays.Children.Clear(); });

                if (accounts == null)
                    accounts = (await (new RestClientMP()).GetAccountingInfo()).Data;
                if (accounts != null)
                {
                    foreach (var account in accounts)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                            {
                                Accounts.Add(account);
                                baseForPays.Children.Add(AddAccountToList(account, _accountingInfos));                               
                            }
                        );
                    }
                    if (accounts.Count == 0)
                    {
                        Device.BeginInvokeOnMainThread(() => { AccExistsLbl.IsVisible = true; });
                        MessagingCenter.Send<Object, string>(this, "RemoveIdent", null);
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() => { AccExistsLbl.IsVisible = false; });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() => { AccExistsLbl.IsVisible = true; } );
                }

                IsRefreshing = false;
            });
        }

        async void ShowBonusHistory(object sender, EventArgs e)
        {
            await Dialog.Instance.ShowAsync(new BonusHistoryDialogView("123"));
        }

        MaterialFrame AddAccountToList(AccountAccountingInfo info, List<AccountAccountingInfo> _accountingInfo)
        {
            var accounts = Settings.Person.Accounts;
            Label ident = new Label();
            Label adress = new Label();
            Label bonus = new Label();
            StackLayout dell = new StackLayout();
            Label sumPayDate = new Label();
            Label sumPay = new Label();

            MaterialFrame frame = new MaterialFrame();
            frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"],
                Color.White);
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frame.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor = Color.White;
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(15, 15, 15, 15);
            frame.CornerRadius = 30;

            StackLayout container = new StackLayout();
            container.Orientation = StackOrientation.Vertical;

            StackLayout dateIdent = new StackLayout();
            dateIdent.Orientation = StackOrientation.Horizontal;

            StackLayout identAdress = new StackLayout();
            identAdress.HorizontalOptions = LayoutOptions.StartAndExpand;
            identAdress.Spacing = 0;

            ident.FontSize = 15;
            ident.TextColor = Color.Black;
            adress.FontSize = 12;
            adress.TextColor = Color.Gray;
            adress.FontFamily = "Roboto";

            bonus.FontSize = 15;
            FormattedString formattedBonus = new FormattedString();

            formattedBonus.Spans.Add(new Span
            {
                Text = AppResources.PayBonus + ": ",
                FontSize = 15,
                TextColor = Color.Black,
            });
            formattedBonus.Spans.Add(new Span
            {
                Text = info.BonusBalance.ToString(),
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
            });
            formattedBonus.Spans.Add(new Span
            {
                Text = AppResources.PayPoints,
                TextColor = Color.Gray,
                FontSize = 12
            });
            bonus.FormattedText = formattedBonus;
            bonus.HorizontalOptions = LayoutOptions.Start;

            StackLayout BonusStack = new StackLayout { 
                Orientation = StackOrientation.Horizontal, 
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            BonusStack.Children.Add(bonus);
            Label history = new Label { 
                TextColor = (Color)Application.Current.Resources["MainColor"], 
                Text = AppResources.ShowHistory, 
                HorizontalOptions = LayoutOptions.End, 
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12
            };
            history.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = OpenHistoryCommand,
                CommandParameter = accounts.Find(x => x.Ident == info.Ident)?.ID
            });
            BonusStack.Children.Add(history);

            identAdress.Children.Add(ident);
            identAdress.Children.Add(adress);
            if (Settings.MobileSettings.useBonusSystem)
                identAdress.Children.Add(BonusStack);

            SvgCachedImage x = new SvgCachedImage();
            x.Source = "resource://xamarinJKH.Resources.ic_close.svg";
            x.ReplaceStringMap = new Dictionary<string, string> { { "#000000", $"#{Settings.MobileSettings.color}"} };
            x.HeightRequest = 10;
            x.WidthRequest = 10;

            Label close = new Label();
            close.Text = AppResources.Delete; // "Удалить";
            close.TextColor = (Color)Application.Current.Resources["MainColor"];
            close.FontSize = 15;
            close.TextDecorations = TextDecorations.Underline;
            dell.Orientation = StackOrientation.Horizontal;
            dell.HorizontalOptions = LayoutOptions.EndAndExpand;
            dell.VerticalOptions = LayoutOptions.Center;
            dell.MinimumWidthRequest = 80;

            dell.Children.Add(x);
            dell.Children.Add(close);

            dateIdent.Children.Add(identAdress);
            dateIdent.Children.Add(dell);

            Label separator = new Label();

            separator.HeightRequest = 1;
            separator.BackgroundColor = Color.Gray;
            separator.Margin = new Thickness(0, 5, 0, 5);
            container.Children.Add(dateIdent);
            container.Children.Add(separator);

            StackLayout sums = new StackLayout();
            sums.Orientation = StackOrientation.Horizontal;
            sums.Margin = new Thickness(30, 0, 30, 5);

            sumPayDate.Text = string.Format("Сумма к оплате{0} на 31.05.2020", Environment.NewLine);
            sumPayDate.FontSize = 15;
            sumPayDate.TextColor = Color.Gray;
            sumPayDate.HorizontalTextAlignment = TextAlignment.End;
            sumPayDate.Margin = new Thickness(0, 0, 15, 0);

            sumPay.Text = "4593.01 руб";
            sumPay.TextColor = (Color)Application.Current.Resources["MainColor"];
            sumPay.FontSize = 25;
            sumPay.VerticalOptions = LayoutOptions.Center;

            sums.Children.Add(sumPayDate);
            sums.Children.Add(sumPay);

            container.Children.Add(sums);

            Frame frameBtn = new Frame();
            frameBtn.HorizontalOptions = LayoutOptions.FillAndExpand;
            frameBtn.VerticalOptions = LayoutOptions.Start;
            frameBtn.Padding = 0;
            frameBtn.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            frameBtn.CornerRadius = 10;
            frameBtn.SetOnAppTheme(Frame.HasShadowProperty, false, true);
            frameBtn.SetOnAppTheme(MaterialFrame.ElevationProperty, 0, 20);

            StackLayout containerBtn = new StackLayout();
            containerBtn.Orientation = StackOrientation.Horizontal;
            containerBtn.Spacing = 0;
            containerBtn.HorizontalOptions = LayoutOptions.CenterAndExpand;

            SvgCachedImage image = new SvgCachedImage();
            image.Source = "resource://xamarinJKH.Resources.ic_pays.svg";
            image.ReplaceStringMap = new Dictionary<string, string> { { "#000000", "#FFFFFF" } };
            // image.Margin = new Thickness(-45, 0, 0, 0);
            image.HeightRequest = 30;
            image.WidthRequest = 30;

            Label btn = new Label();
            // btn.Margin = new Thickness(-30, 0, 0, 0);
            btn.TextColor = Color.White;
            btn.BackgroundColor = Color.Transparent;
            btn.HorizontalOptions = LayoutOptions.Center;
            btn.Margin = new Thickness(13, 13, 0, 13);
            btn.FontAttributes = FontAttributes.Bold;
            btn.FontSize = 16;
            btn.Text = AppResources.Pay; //"Оплатить";

            containerBtn.Children.Add(image);
            containerBtn.Children.Add(btn);

            frameBtn.Content = containerBtn;

            container.Children.Add(frameBtn);

            Label payPeriod = new Label();

            FormattedString formatted = new FormattedString();

            formatted.Spans.Add(new Span
            {
                Text = AppResources.PayProcess,
                FontSize = 12
            });
            formatted.Spans.Add(new Span
            {
                Text = " 2-3 ",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            });
            formatted.Spans.Add(new Span
            {
                Text = AppResources.WorkDays,
                FontSize = 12
            });

            payPeriod.FormattedText = formatted;
            payPeriod.TextColor = (Color)Application.Current.Resources["MainColor"];
            payPeriod.FontSize = 12;
            payPeriod.HorizontalOptions = LayoutOptions.CenterAndExpand;

            container.Children.Add(payPeriod);

            frame.Content = container;

            var openPayRec = new TapGestureRecognizer();
            openPayRec.Tapped += async (s, e) => { if (Navigation.NavigationStack.FirstOrDefault(x => x is CostPage) == null) await Navigation.PushAsync(new CostPage(info, _accountingInfo)); };
            frameBtn.GestureRecognizers.Add(openPayRec);


            var delLs = new TapGestureRecognizer();
            delLs.Tapped += async (s, e) => { ((PaysPage) Settings.mainPage).DellLs(info.Ident); };
            dell.GestureRecognizers.Add(delLs);

            FormattedString formattedIdent = new FormattedString();
            formattedIdent.Spans.Add(new Span
            {
                Text = AppResources.Acc, //"Л/сч: ",
                TextColor = Color.Black,
                FontSize = 15
            });
            formattedIdent.Spans.Add(new Span
            {
                Text = AppResources.Number + info.Ident,
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15
            });
            ident.FormattedText = formattedIdent;
            FormattedString formattedPayDate = new FormattedString();
            var fs = 15;
            if (DeviceDisplay.MainDisplayInfo.Width < 800)
                fs = 12;
            formattedPayDate.Spans.Add(new Span
            {
                Text = $"{AppResources.SumToPay}\n",
                TextColor = Color.Gray,
                FontSize = fs
            });
            formattedPayDate.Spans.Add(new Span
            {
                Text = AppResources.By + info.DebtActualDate + ":",
                TextColor = Color.Black,
                FontSize = fs
            });
            adress.Text = info.AccountType + "\n" + info.Address;
            sumPayDate.FormattedText = formattedPayDate;
            FormattedString formattedPay = new FormattedString();
            formattedPay.Spans.Add(new Span
            {
                Text = info.Sum.ToString(),
                TextColor = (Color)Application.Current.Resources["MainColor"],
                FontAttributes = FontAttributes.Bold,
                FontSize = fs + 5
            });
            formattedPay.Spans.Add(new Span
            {
                Text = AppResources.Currency,
                TextColor = Color.Gray,
                FontSize = fs
            });
            sumPay.FormattedText = formattedPay;

            return frame;
        }

        Command OpenHistoryCommand
        {
            get
            {
                return new Command(async (ident) =>
                {
                    if (ident != null)
                        await Dialog.Instance.ShowAsync(new BonusHistoryDialogView(ident.ToString()));
                });
            }
        }
    }
}