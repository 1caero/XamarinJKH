using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Additional
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdditionalOnePage : ContentPage
    {
        private AdditionalService additionalService;
        private RestClientMP _server = new RestClientMP();
        public  string adress { get; set; }
        public ICommand ClickCommand => new Command<string>(async (url) =>
        {
            try
            {
                await Launcher.OpenAsync("https://" + url.Replace("https://", "").Replace("http://", ""));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAdditionalLink, "OK");
            }
        });
        public AdditionalOnePage(AdditionalService additionalService)
        {
            this.additionalService = additionalService;
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake2.HeightRequest = statusBarHeight;
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


            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
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
                        phoneDialer.MakePhoneCall(Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            
            SetText();
            BindingContext = this;
        }


        async void SetText()
        {
           

            if (additionalService.Name != null && !additionalService.Name.Equals(""))
            {
                LabelTitle.Text = additionalService.Name.Replace("\\n","");
                Analytics.TrackEvent("Доп услуга " + additionalService.Name);
            }
            else
            {
                LabelDesc.IsVisible = false;
            }

            if (additionalService.Address != null && !additionalService.Address.Equals(""))
            {
                LabelAdress.Text = additionalService.Address;
                adress = additionalService.Address;
            }
            else
            {
                LabelAdress.IsVisible = false;
            }

            if (additionalService.Description != null && !additionalService.Description.Equals(""))
            {
                LabelDesc.Text = additionalService.Description;
            }
            else
            {
                LabelDesc.IsVisible = false;
            }

            byte[] imageByte = await _server.GetPhotoAdditionalDop(additionalService.ID.ToString());
            if (imageByte != null)
            {
                Stream stream = new MemoryStream(imageByte);
                ImageAdd.Source = ImageSource.FromStream(() => { return stream; });
            }
            else
            {
                ImageAdd.IsVisible = false;
            }

            FrameBtnQuest.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            if (additionalService.CanBeOrdered)
            {
                FrameBtnQuest.IsVisible = true;
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
        }


        private async void ButtonClick(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            FrameBtnQuest.IsVisible = false;
            progress.IsVisible = true;
            if (Settings.Person.Accounts.Count > 0)
            {
                DateTime now = DateTime.Now;
                IDResult result = await _server.newApp(Settings.Person.Accounts[0].Ident,
                    additionalService.id_RequestType.ToString(),
                    $"{AppResources.OrderAccepted} {now:dd.MM.yyyy HH:mm:ss}. {AppResources.OrderMessageAdditional}\n" +
                    additionalService.Description);
                if (result.Error == null)
                {
                    RequestsUpdate requestsUpdate =
                        await _server.GetRequestsUpdates(Settings.UpdateKey, result.ID.ToString());
                    if (requestsUpdate.Error == null)
                    {
                        Settings.UpdateKey = requestsUpdate.NewUpdateKey;
                    }

                    await DisplayAlert(AppResources.AlertSuccess, AppResources.OrderSuccess, "OK");
                    RequestInfo requestInfo = new RequestInfo();
                    requestInfo.ID = result.ID; if (Navigation.NavigationStack.FirstOrDefault(x => x is Apps.AppPage) == null)
                        await Navigation.PushAsync(new Apps.AppPage(requestInfo, true));
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorConnectcIdent, "OK");
            }
            FrameBtnQuest.IsVisible = true;
            progress.IsVisible = false;
        }
    }
}