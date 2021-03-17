using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterPhoneDialog : Rg.Plugins.Popup.Pages.PopupPage
    {
        private readonly bool _isDeviceId;
        private RestClientMP _server = new RestClientMP();

        public EnterPhoneDialog(bool isVisible = true, bool isDeviceId = false)
        {
            _isDeviceId = isDeviceId;
            InitializeComponent();
            Frame.IsVisible = isVisible;
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) =>
            {
                if(PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
            };
            IconViewClose.GestureRecognizers.Add(close);
        }

        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
            Device.BeginInvokeOnMainThread(() => PinCode.Focus());
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            string replace;
            if (!string.IsNullOrEmpty(PinCode.Text))
            {
                replace = PinCode.Text.Replace("+", "")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "");

                if (replace.Length < 11)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorTechNumberFormat, "OK");
                }
                else
                {
                   
                    if (Settings.Person == null)
                        Settings.Person = new LoginResult()
                        {
                            Phone = replace
                        };
                    else
                        Settings.Person.Phone = replace;
                    Preferences.Set("techPhone", replace);
                    await _server.RegisterDeviceNotAvtorization(Settings.Person.Phone);
                    await Navigation.PushModalAsync(new AppPage(_isDeviceId));
                    if(PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync();
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorTechNumberFormat, "OK");
            }
        }

        private void PinCode_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}