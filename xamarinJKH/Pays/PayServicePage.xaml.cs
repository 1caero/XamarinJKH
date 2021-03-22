using System;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PayServicePage : ContentPage
    {
        private RestClientMP server = new RestClientMP();
        private int? idRequset;
        private PaymentSystem paymentSystem = null;
        public PayServicePage(string ident, decimal sum, int? idRequset = null, bool isInsurance= false, PaymentSystem paymentSystem = null)
        {
            this.idRequset = idRequset;
            this.paymentSystem = paymentSystem;
            InitializeComponent();
            Analytics.TrackEvent("Шлюз оплаты по лс" + ident);
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
            if (Device.RuntimePlatform == Device.iOS)
            {
                int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();

                iosBarSeparator.IsVisible = true;
                iosBarSeparator.IsEnabled = true;
                iosBarSeparator.HeightRequest = statusBarHeight;
            }


            if (idRequset == null)
            {
                Device.BeginInvokeOnMainThread(async () => { await GetPayLink(ident, sum, isInsurance); });
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () => { await GetPayLinkRequest(idRequset, sum); });
            }
        }

        async Task GetPayLink(string ident, decimal sum, bool isInsurance)
        {
            Analytics.TrackEvent("Загрузка страницы оплаты по лс " + ident);
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };
            await Loading.Instance.StartAsync(async progress =>
            {
                PayService payLink = await server.GetPayLink(ident, sum, isInsurance, paymentSystem?.Name);
                if (payLink.payLink != null)
                {
                    Analytics.TrackEvent("Ссылка на оплату " + payLink.payLink);
                    Device.BeginInvokeOnMainThread(async () => webView.Source = payLink.payLink);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Analytics.TrackEvent("Ссылка на оплату " + payLink.Error);
                        await DisplayAlert(AppResources.ErrorTitle, payLink.Error, "OK");
                        try
                        {
                            _ = await Navigation.PopAsync();
                        }
                        catch
                        {
                        }
                    });
                }
            });
           
        }

        async Task GetPayLinkRequest(int? id, decimal sum)
        {
            Analytics.TrackEvent("Загрузка страницы оплаты по платной заявке " + id);
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };
            await Loading.Instance.StartAsync(async progress =>
            {
                PayService payLink = await server.GetPayLink(id, sum);
                if (payLink.payLink != null)
                {
                    Analytics.TrackEvent("Ссылка на оплату " + payLink.payLink);
                    webView.Source = payLink.payLink;
                }
                else
                {
                    Analytics.TrackEvent("Ссылка на оплату " + payLink.Error);
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, payLink.Error, "OK"));
                    try
                    {
                        _ = await Navigation.PopAsync();
                    }
                    catch { }
                }
            });
           
        }

        private void WebView_OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            var eUrl = e.Url;
            Loading.Instance.Hide();
            var findByName = webView.FindByName("objectBox objectBox-string");
        }

        private async void WebView_OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            var eUrl = e.Url;
            if (eUrl.Contains("GetPayResult"))
            {
                string url = eUrl.Replace(RestClientMP.SERVER_ADDR + "/", "");
                Analytics.TrackEvent("оплата произведена " + url);
                if(!isProgress)
                    await StartProgressBar(url);
            }
            
        }

        private bool isProgress = false;
        public async Task StartProgressBar(string url)
        {
            isProgress = true;
            Analytics.TrackEvent("Обработка завершения оплаты");
            bool rate = Preferences.Get("rate", true);
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.6,
                DefaultMessage = "Оплата",
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                PayResult result = await server.GetPayResult(url);
                if (result.error != null && result.Equals(""))
                {
                    Analytics.TrackEvent("Результат оплаты " + result.error);
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, result.error, "OK"));
                    try
                    {
                        _ = await Navigation.PopAsync();
                    }
                    catch { }
                }
                else
                {
                    if(Device.RuntimePlatform==  Device.iOS)
                        Loading.Instance.Hide();
                    await Navigation.PopToRootAsync();
                    Analytics.TrackEvent("Результат оплаты " + result.message);
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.AlertSuccess, result.message, "OK"));
                    if (rate)
                    {
                        await PopupNavigation.Instance.PushAsync(new RatingAppMarketDialog());
                    }
                    if (idRequset != null)
                    {
                        await GetCodePay();
                    }
                }
            });
        }

        private async Task GetCodePay()
        {
            CommonResult resultCode =
                await server.SendPaidRequestCompleteCodeOnlineAndCah(idRequset, Settings.Person.Phone);
            if (resultCode.Error != null)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, resultCode.Error, "OK"));
            }
            else
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert("", AppResources.AlertCodeSent, "OK"));
                }
                else
                {
                    DependencyService.Get<IMessage>().ShortAlert(AppResources.AlertCodeSent);
                }
            }
        }
    }
}