using System;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Main;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterPin :PopupPage
    {
        RestClientMP server = new RestClientMP();
        public EnterPin()
        {            
            InitializeComponent();
            Analytics.TrackEvent("Диалог для ввода пин-кода");
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => {
                if (PopupNavigation.Instance.PopupStack.Count > 0) 
                    await PopupNavigation.Instance.PopAsync(); };
            IconViewClose.GestureRecognizers.Add(close);
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PinCode.Focus();

        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            string pinCodeText = PinCode.Text;
            if(!string.IsNullOrEmpty(pinCodeText))
            {
                //сохраняем пин-код
                Preferences.Set("PinCode", pinCodeText);
                Preferences.Set("PinAddNeed", false);
                await DisplayAlert("", $"{AppResources.PinField} {AppResources.Saved}", "ОК");
                MessagingCenter.Send<object>(this, "PinAddedSucces");

                if (PopupNavigation.Instance.PopupStack.Count>0)
                    await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                await DisplayAlert("", $"{AppResources.ErrorTitle} {AppResources.CreatePin}", "ОК");
            }
        }
    }
}