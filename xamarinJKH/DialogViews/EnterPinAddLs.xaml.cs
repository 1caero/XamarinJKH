using System;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Main;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterPinAddLs :PopupPage
    {
        RestClientMP server = new RestClientMP();
        public string Ident { get; set; }
        private PaysPage paysPage;
        private AddIdent AddIdent;
        public EnterPinAddLs(string ident, PaysPage paysPage, AddIdent addIdent)
        {
            Ident = ident;
            this.paysPage = paysPage;
            AddIdent = addIdent;
            InitializeComponent();
            Analytics.TrackEvent("Диалог для ввода пин-кода для добавления лс");
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) => { await PopupNavigation.Instance.PopAsync(); };
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
                AddAccountResult result = await server.AddIdent(Ident,false, pinCodeText);
                if (result.Error == null)
                {
                    await DisplayAlert("", $"{AppResources.Acc} " + Ident + $"{AppResources.AddLsString}", "ОК");
                    await paysPage.RefreshPaysData();
                    await PopupNavigation.Instance.PopAsync();
                    MessagingCenter.Send<Object>(this, "ClosePage");
                }
                else
                {
                    await DisplayAlert("", result.Error, "ОК");
                }
            }
        }
    }
}