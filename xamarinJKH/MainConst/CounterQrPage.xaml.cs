using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Counters;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.MainConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CounterQrPage : ContentPage
    {
        private RestClientMP server = new RestClientMP();

        public CounterQrPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake2.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            UkName.Text = Settings.MobileSettings.main_name;
            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);
            var scanQr = new TapGestureRecognizer();
            scanQr.Tapped += async (s, e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string scanAsync;
                    
                    scanAsync = await DependencyService.Get<IQrScanningService>().ScanAsync();
                    
                    if (scanAsync != null)
                    {
#if DEBUG
                        Toast.Instance.Show<ToastDialog>(new {Title = scanAsync, Duration = 5500, ColorB = Color.Gray,  ColorT = Color.White});
#endif
                        List<MeterInfo> meterInfos = await server.GetMeter(scanAsync);
                        if (meterInfos != null && meterInfos.Count > 0)
                        {
                            MeterInfo meterInfo = meterInfos.FirstOrDefault(x => !x.IsDisabled);

                            if(meterInfo==null)
                            {
                                await DisplayAlert(AppResources.Attention, $"Прибор учета {meterInfos[0].UniqueNum} отключен", "OK");
                                //Toast.Instance.Show<ToastDialog>(new { Title = $"Прибор учета {meterInfos[0].UniqueNum} отключен", Duration = 5500, ColorB = Color.Gray, ColorT = Color.White });
                                return;
                            }
                            else
                            {
                                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                {
                                    if (meterInfo.ValuesCanAdd)
                                    {
                                        if (meterInfo?.Values != null && meterInfo.Values.Count >= 1)
                                        {
                                            if (meterInfo.Values[0].IsCurrentPeriod)
                                            {
                                                var counterThisMonth = meterInfo.Values[0].Value;
                                                var counterThisMonth2 =
                                                    meterInfo.Values.Count >= 2 ? meterInfo.Values[1].Value : 0;
                                                await Navigation.PushAsync(new AddMetersPage(meterInfo, meterInfos, null,
                                                    counterThisMonth,
                                                    counterThisMonth2));
                                            }
                                            else
                                            {
                                                var counterThisMonth = meterInfo.Values[0].Value;
                                                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) ==
                                                    null)
                                                    await Navigation.PushAsync(new AddMetersPage(meterInfo, meterInfos,
                                                        null, 0,
                                                        counterThisMonth));
                                            }
                                        }
                                        else
                                        {
                                            var counterThisMonth = meterInfo.StartValue ?? 0;
                                            if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                                await Navigation.PushAsync(new AddMetersPage(meterInfo, meterInfos, null, 0,
                                                    counterThisMonth));
                                        }
                                    }
                                    else
                                    {
                                        Toast.Instance.Show<ToastDialog>(new
                                        { Title = meterInfo.PeriodMessage, Duration = 5500, ColorB = Color.Gray, ColorT = Color.White });
                                    }
                                }
                            }                           
                        }
                        else
                        {
                            Toast.Instance.Show<ToastDialog>(new {Title = AppResources.NothingFound, Duration = 5500, ColorB = Color.Gray,  ColorT = Color.White});
                        }
                    }
                    else
                    {
                        Toast.Instance.Show<ToastDialog>(new {Title = AppResources.NothingFound, Duration = 5500, ColorB = Color.Gray,  ColorT = Color.White});
                    }
                });
            };
            FrameBtnAdd.GestureRecognizers.Add(scanQr);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend;
            LabelTech.GestureRecognizers.Add(techSend);
            BindingContext = this;
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
    }
}