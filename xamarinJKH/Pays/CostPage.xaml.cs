using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CostPage : ContentPage
    {
        private AccountAccountingInfo account { get; set; }
        RestClientMP server = new RestClientMP();
        private List<AccountAccountingInfo> Accounts { get; set; }
        private bool isComission = false;
        
        public string svg { get; set; }
        private Entry EntrySum = new Entry();
        private AccountingInfoModel _model;
        public CostPage(AccountAccountingInfo account, List<AccountAccountingInfo> accounts)
        {
            this.account = account;
            Accounts = accounts;
            InitializeComponent();
            GetPaymentList();
            Analytics.TrackEvent("Оплата по ЛС " + account.Ident);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);
            
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {    await Navigation.PushAsync(new AppPage());};
            LabelTech.GestureRecognizers.Add(techSend);
            var pay = new TapGestureRecognizer();
            pay.Tapped += async (s, e) => Pay(FrameBtnLogin, null);
            FrameBtnLogin.GestureRecognizers.Add(pay);
            var pickLs = new TapGestureRecognizer();
            pickLs.Tapped += async (s, e) => {  
                Device.BeginInvokeOnMainThread(() =>
                {
                    Picker.Focus();
                });
            };
            StackLayoutLs.GestureRecognizers.Add(pickLs);

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
                    // <Entry x:Name="EntrySum"
                    // FontSize="15"
                    // VerticalOptions="End"
                    // FontAttributes="Bold"
                    // Completed="Entry_Completed"
                    // TextChanged="EntrySum_OnTextChanged"
                    // HorizontalTextAlignment="Center"
                    // TextColor="Black"
                    // Keyboard="Numeric"
                    // HorizontalOptions="FillAndExpand
                    EntrySum = new Entry
                    {
                        FontSize = 15,
                        VerticalOptions = LayoutOptions.End,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.Black,
                        Keyboard = Keyboard.Numeric,
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    };
                    EntrySum.TextChanged += EntrySum_OnTextChanged;
                    IconViewSaldos.Margin = new Thickness(0, 0, 5, 0);
                    break;
                case Device.Android:
                    EntrySum = new EntryWithCustomKeyboard
                    {
                        FontSize = 15,
                        VerticalOptions = LayoutOptions.End,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.Black,
                        IntegerPoint = 6,
                        DecimalPoint = 2,
                        HorizontalOptions = LayoutOptions.FillAndExpand
                    };
                    EntrySum.TextChanged += EntrySum_OnTextChanged;
                    break;
                default:
                    break;
            }
            LayoutEntry.Children.Add(EntrySum);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    break;
                case Device.Android:
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        BackStackLayout.Margin = new Thickness(-5, 15, 0, 0);
                    }

                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var openSaldos = new TapGestureRecognizer();
            openSaldos.Tapped += async (s, e) => { if (Navigation.NavigationStack.FirstOrDefault(x => x is SaldosPage) == null) await Navigation.PushAsync(new SaldosPage(Accounts)); };
            FrameBtnSaldos.GestureRecognizers.Add(openSaldos);
            var openHistory = new TapGestureRecognizer();
            openHistory.Tapped += async (s, e) => { if (Navigation.NavigationStack.FirstOrDefault(x => x is HistoryPayedPage) == null) await Navigation.PushAsync(new HistoryPayedPage(Accounts)); };
            FrameBtnHistory.GestureRecognizers.Add(openHistory);
            BindingContext =_model= new AccountingInfoModel(PayServiceList)
            {
                AllAcc = Accounts,
                hex = (Color)Application.Current.Resources["MainColor"]
            };
            SetText();
            
            if (!RestClientMP.SERVER_ADDR.Contains("komfortnew"))
            {
                LayoutPaymentService.IsVisible = false;
            }
        }

      

        async void GetPaymentList()
        {
           
            
        }
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
           
            Picker.Title = account.Ident;
            
            FrameBtnLogin.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            LabelSaldos.TextColor = (Color)Application.Current.Resources["MainColor"];
            LabelHistory.TextColor = (Color)Application.Current.Resources["MainColor"];
            FrameBtnHistory.BorderColor = (Color)Application.Current.Resources["MainColor"];
            FrameBtnSaldos.BorderColor = (Color)Application.Current.Resources["MainColor"];
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            Frame.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);

            SetPays();
            SwitchInsurance.IsToggled = true;
        }

        async void SetPays()
        {
            EntrySum.Text = account.Sum.ToString();
            FormattedString formatted = new FormattedString();
            ComissionModel result = await server.GetSumWithComission(account.Sum.ToString());
            string totalSum = EntrySum.Text;
            if (result.Error == null && result.Comission != 0)
            {
                isComission = true;
                LabelCommision.Text = $"{AppResources.Commision} " + result.Comission + $" {AppResources.Currency}";
                LabelCommision.IsVisible = !result.HideComissionInfo;
                
                totalSum = result.TotalSum.ToString();
                if (result.Comission == 0)
                {
                    LabelCommision.Text = AppResources.NotComissions;
                }
            }

            LayoutInsurance.IsVisible = account.InsuranceSum != 0;
            InsuranceDoc.IsVisible = account.InsuranceSum != 0;
            SwitchInsurance.IsToggled = account.InsuranceSum != 0;

            LabelInsurance.Text = AppResources.InsuranceText.Replace("111", account.InsuranceSum.ToString());
            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.Total}: ",
                FontSize = 17,
                TextColor = Color.Black
            });
            formatted.Spans.Add(new Span
            {
                Text = totalSum,
                FontSize = 20,
                TextColor = (Color)Application.Current.Resources["MainColor"],
                FontAttributes = FontAttributes.Bold
            });
            formatted.Spans.Add(new Span
            {
                Text = $" {AppResources.Currency}",
                FontSize = 15,
                TextColor = Color.FromHex("#777777")
            });
            LabelTotal.FormattedText = formatted;
            String[] month = account.DebtActualDate.Split('.');
            formatted = new FormattedString();

            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.PaymentOf} ",
                FontSize = 12,
                TextColor = Color.Black
            });
            formatted.Spans.Add(new Span
            {
                Text = Settings.months[Int32.Parse(month[1]) - 1] + " " + month[2],
                FontSize = 12,
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold
            });
            LabelMonth.FormattedText = formatted;
            Picker.Title = account.Ident;
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            account = Accounts[Picker.SelectedIndex];
            Analytics.TrackEvent("Смена лс в оплате на "  + account.Ident);

            SetPays();
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
        }

        bool isDigit(char s)
        {
            var dgts = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            return dgts.Contains(s);
        }

        private async void EntrySum_OnTextChanged(object sender, TextChangedEventArgs e)
        {
           if(!string.IsNullOrWhiteSpace(e.NewTextValue) && !isDigit(e.NewTextValue.Last()))
            {
                if (e.OldTextValue.Length< e.NewTextValue.Length && e.OldTextValue.Contains(e.NewTextValue.Last()))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        EntrySum.TextChanged -= EntrySum_OnTextChanged;
                        EntrySum.Text = e.OldTextValue; 
                        EntrySum.TextChanged += EntrySum_OnTextChanged;
                    }
);
                    return;
                }
            }

            await SetSumPay();
        }

        private async Task SetSumPay()
        {
            FormattedString formatted = new FormattedString();
            //
            string sumText = string.IsNullOrEmpty(EntrySum.Text) ? "0" : EntrySum.Text;


            decimal totalSum = 0;
            if (sumText.Equals("-"))
            {
                totalSum = 0;
            }
            else
            {
                try
                {                    
                    var sumWithDot = sumText.Replace(',', '.');
                    var correctSum = decimal.TryParse(sumWithDot, NumberStyles.Any, CultureInfo.InvariantCulture, out totalSum);

                    if (!correctSum)
                    {
                        Analytics.TrackEvent($"Оплата по ЛС. ошибка при конвертации в decimal числа {sumText}");
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorNumberSumm, null, "OK"));
                        return;
                    }
                }
                catch(Exception ex)
                {
                    Analytics.TrackEvent($"Оплата по ЛС. ошибка при конвертации в decimal числа {sumText}");
                    Crashes.TrackError(ex);
                    throw;
                }
            }

            ComissionModel result = await server.GetSumWithComission(totalSum.ToString());
            if (result.Error == null && !result.Comission.Equals("0"))
            {
                isComission = true;
                LabelCommision.Text = $"{AppResources.Commision} " + result.Comission + $" {AppResources.Currency}";
                LabelCommision.IsVisible =  !result.HideComissionInfo;
                totalSum = result.TotalSum + (SwitchInsurance.IsToggled ? account.InsuranceSum : 0);
                if (result.Comission == 0)
                {
                    LabelCommision.Text = AppResources.NotComissions;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            
            formatted.Spans.Add(new Span
            {
                Text = $"{AppResources.Total}: ",
                FontSize = 17,
                TextColor = Color.Black
            });

            formatted.Spans.Add(new Span
            {
                Text = totalSum.ToString(),
                FontSize = 20,
                TextColor = (Color)Application.Current.Resources["MainColor"],
                FontAttributes = FontAttributes.Bold
            });
            formatted.Spans.Add(new Span
            {
                Text = $" {AppResources.Currency}",
                FontSize = 15,
                TextColor = Color.Gray
            });
            LabelTotal.FormattedText = formatted;
        }

        private async void openSaldo(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is SaldosPage) == null)
                await Navigation.PushAsync(new SaldosPage(Accounts));
        }

        private async void OpenHistory(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is HistoryPayedPage) == null)
                await Navigation.PushAsync(new HistoryPayedPage(Accounts));
        }

        private async void Pay(object sender, EventArgs e)
        {
            string sumText = EntrySum.Text;
            PaymentSystem paymentSystem = _model.PaymentSystems.FirstOrDefault(x => x.Check);
            if (!sumText.Equals("") && !sumText.Equals("0") && !sumText.Equals("-"))
            {
                decimal sumPay = -1; // Decimal.Parse(sumText);

                try
                {
                    var sumWithDot = sumText.Replace(',', '.');
                    var correctSum = decimal.TryParse(sumWithDot, NumberStyles.Any, CultureInfo.InvariantCulture, out sumPay);

                    if (!correctSum)
                    {
                        Analytics.TrackEvent($"Оплата по ЛС. ошибка при конвертации в decimal числа {sumText}");
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorNumberSumm, null, "OK"));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent($"Оплата по ЛС. ошибка при конвертации в decimal числа {sumText}");
                    Crashes.TrackError(ex);
                    throw;
                }


                if (sumPay < 1 && sumPay > 0)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorEnterSumOne, "OK");
                    return;
                }
                if (sumPay > 0)
                {
                    Analytics.TrackEvent("Оплата " + paymentSystem);
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is PayServicePage) == null)
                        await Navigation.PushAsync(new PayServicePage(account.AccountID, sumPay, null, SwitchInsurance.IsToggled && SwitchInsurance.IsVisible, paymentSystem));
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle,AppResources.ErrorOverpay, "OK");
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorEnterSum, "OK");
            }
        }

        private async void SwitchLogin_OnToggled(object sender, ToggledEventArgs e)
        {
            await SetSumPay();
        }

        private async void OpenInsuranceInfo(object sender, EventArgs args)
        {
            try
            {
                await Launcher.OpenAsync("https://sm-center.ru/vsk_polis.pdf");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}