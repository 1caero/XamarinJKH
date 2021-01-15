using System;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditCounterNameDialog :PopupPage
    {
        RestClientMP server = new RestClientMP();
        public Color hex { get; set; }
        public string UniqueNum { get; set; }
        
        public EditCounterNameDialog(Color hexColor, string uniqName)
        {
            hex = hexColor;
            UniqueNum = uniqName;
            InitializeComponent();
            Analytics.TrackEvent("Диалог смены названия прибора");

            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
            BindingContext = this;
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            string name = EditName.Text; 
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            if (!name.Equals(""))
            {
                Configurations.LoadingConfig = new LoadingConfig {
                    IndicatorColor = hex,
                    OverlayColor = Color.Black,
                    Opacity = 0.8,
                    DefaultMessage = "",
                };

                await Loading.Instance.StartAsync(async progress =>
                {
                        CommonResult result = await server.SetMeterCustomName(UniqueNum, name);
                        if (result.Error == null)
                        {
                            MessagingCenter.Send<Object>(this, "UpdateCounters");
                            await PopupNavigation.Instance.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert(AppResources.Error, result.Error, "OK");
                        }
                });
            }
            else
            {
                await DisplayAlert(AppResources.Error, AppResources.EnterName, "OK");
            }
        }
    }
}