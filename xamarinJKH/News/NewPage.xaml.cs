using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiForms.Dialogs;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
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

namespace xamarinJKH.News
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewPage : ContentPage
    {
        private NewsInfo newsInfo;
        private NewsInfoFull newsInfoFull;
        private RestClientMP _server = new RestClientMP();
       
        public string hex { get; set; }

        public NewPage(NewsInfo newsInfo)
        {
            this.newsInfo = newsInfo;
            InitializeComponent();
            Analytics.TrackEvent("Новость");

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
            LabelPhone.GestureRecognizers.Add(call);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            BindingContext = this;
            if (!newsInfo.IsReaded)
            {
                Task.Run(async () =>
                {
                    var response = await _server.SetNewReadFlag(newsInfo.ID);
                    MessagingCenter.Send<Object, int>(this, "SetEventsAmount", -1);
                    MessagingCenter.Send<Object>(this, "ReduceNews");
                });
            }
        }

        bool navigated;
        async void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            if (!string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
            {
                LabelPhone.Text = "+" + Settings.Person.companyPhone.Replace("+", "");
            }
            else
            {
                IconViewLogin.IsVisible = false;
                LabelPhone.IsVisible = false;
            }
            LabelTitle.Text = newsInfo.Header;
            LabelDate.Text = newsInfo.Created;
            newsInfoFull = await _server.GetNewsFull(newsInfo.ID.ToString());
            // MainText.Source = new HtmlWebViewSource { Html = newsInfoFull.Text };
            HtmlLabel.Text = newsInfoFull.Text;
            HtmlLabel.FlowDirection = FlowDirection.MatchParent;
            // MainText.FlowDirection = FlowDirection.MatchParent;
            // MainText.Navigated += (s, e) =>
            // {
            //     if (!navigated)
            //     {
            //         (s as WebView).Source = new UrlWebViewSource() { Url = e.Url };
            //         navigated = true;
            //     }
            // };
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);            
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);{ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            Files.IsVisible = newsInfoFull.HasImage;
        }

        public async void OpenFile(object sender, EventArgs args)
        {
            Analytics.TrackEvent("Открытие файла новости");
            try
            {
                var link = RestClientMP.SERVER_ADDR + "/News/Image/" + newsInfoFull.ID.ToString();
                await Dialog.Instance.ShowAsync(new NewFile(link));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}