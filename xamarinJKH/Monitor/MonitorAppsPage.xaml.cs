using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Apps;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.MainConst;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Monitor
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonitorAppsPage : ContentPage
    {
        public List<Requests> RequestInfos { get; set; }
        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        public Color hex { get; set; }

        private async void TechSend(object sender, EventArgs e)
        {

            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(Settings.Person.Phone))
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is Tech.AppPage) == null)
                    await Navigation.PushModalAsync(new Tech.AppPage());
            }
            else
            {
                if (PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x is EnterPhoneDialog) == null)
                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }

        public MonitorAppsPage(List<Requests> requestInfos)
        {
            InitializeComponent();
            Analytics.TrackEvent("Заявки мониторинга");
            NavigationPage.SetHasNavigationBar(this, false);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    break;
                default:
                    break;
            }
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //BackgroundColor = Color.White;
                    // ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    break;
                case Device.Android:
                    // double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -125);
                    //     BackStackLayout.Margin = new Thickness(5, 25, 0, 0);
                    // }

                    break;
                default:
                    break;
            }

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend;// async (s, e) => {     await Navigation.PushAsync(new Tech.AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            hex = (Color)Application.Current.Resources["MainColor"];
            SetText();
            RequestInfos = requestInfos;
            additionalList.BackgroundColor = Color.Transparent;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            BindingContext = this;
        }
        

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            FormattedString formattedName = new FormattedString();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            formattedName.Spans.Add(new Span
            {
                Text = Settings.Person.FIO,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            });
            formattedName.Spans.Add(new Span
            {
                Text = AppResources.GoodDay,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.None,
                FontSize = 16
            });
         
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            
        }

        
        
        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Requests select = e.Item as Requests;
            RequestInfo requestInfo = new RequestInfo()
            {
                ID = select.Number,
                RequestNumber  = select.RequestNumber,
                Added = select.Added.ToString(),
                Name = select.Name,
                Status = select.Status,
                StatusID = select.id_Status,
                IsClosed = !select.IsActive,
                IsPerformed = true
            };
            if (Navigation.NavigationStack.FirstOrDefault(x => x is AppConstPage) == null)
                await Navigation.PushAsync(new AppConstPage(requestInfo));

        }
        
    }
}