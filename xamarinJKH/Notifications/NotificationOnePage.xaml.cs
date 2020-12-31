using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Additional;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Questions;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Shop;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using FileInfo = xamarinJKH.Server.RequestModel.FileInfo;

namespace xamarinJKH.Notifications
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationOnePage : ContentPage
    {
        private AnnouncementInfo _announcementInfo;
        private AdditionalService _additional;
        private PollInfo _polls;
        private RestClientMP _server = new RestClientMP();
        
        public List<FileInfo> Files { get; set; }

        public NotificationOnePage(AnnouncementInfo announcementInfo)
        {
            
            _announcementInfo = announcementInfo;
            InitializeComponent();
            Analytics.TrackEvent("Уведомление " + announcementInfo.ID);
            CollectionViewFiles.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    //Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    Pancake2.HeightRequest = statusBarHeight;

                    //BackgroundColor = Color.White;
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
            techSend.Tapped += async (s, e) => {  await Navigation.PushAsync(new AppPage());   };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            // LabelPhone.GestureRecognizers.Add(call);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { close(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            Files = announcementInfo.Files;
            BindingContext = this;
            if (!announcementInfo.IsReaded)
            Task.Run(async () =>
            {
                var result = await _server.SetNotificationReadFlag(announcementInfo.ID);
                MessagingCenter.Send<Object, int>(this, "SetEventsAmount", -1);
                MessagingCenter.Send<Object>(this, "ReduceAnnounsements");
            });
        }

        async void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            // if (!string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
            // {
            //     LabelPhone.Text = "+" + Settings.Person.companyPhone.Replace("+", "");
            // }
            // else
            // {
            //     IconViewLogin.IsVisible = false;
            //     LabelPhone.IsVisible = false;
            // }
            LabelTitle.Text = _announcementInfo.Header;
            LabelDate.Text = _announcementInfo.Created;
            LabelText.Text = _announcementInfo.Text;
            FrameBtnQuest.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            int announcementInfoAdditionalServiceId = _announcementInfo.AdditionalServiceId;
            if (announcementInfoAdditionalServiceId != -1)
            {
                _additional = Settings.GetAdditionalService(announcementInfoAdditionalServiceId);
                byte[] imageByte = await _server.GetPhotoAdditional(announcementInfoAdditionalServiceId.ToString());
                if (imageByte != null)
                {
                    Stream stream = new MemoryStream(imageByte);
                    ImageAdd.Source = ImageSource.FromStream(() => { return stream; });
                    ImageAdd.IsVisible = true;

                    var openAdditional = new TapGestureRecognizer();
                    openAdditional.Tapped += async (s, e) =>
                    {
                        if (_additional.ShopID == null)
                        {
                            await Navigation.PushAsync(new AdditionalOnePage(_additional));
                        }
                        else
                        {
                            if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                                await Navigation.PushAsync(new ShopPageNew(_additional));
                        }
                    };
                    ImageAdd.GestureRecognizers.Add(openAdditional);
                }
            }

            if (_announcementInfo.QuestionGroupID != -1)
            {
                _polls = Settings.GetPollInfo(_announcementInfo.QuestionGroupID);
                FrameBtnQuest.IsVisible = true;
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            // IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //
            // Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            // PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);if (Device.RuntimePlatform == Device.iOS){ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.Black);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.Black);
        }

        async void open(Page page)
        {
            try
            {
                await Navigation.PushAsync(page);
            }
            catch (Exception e)
            {
                await Navigation.PushModalAsync(page);
            }
        }
        async void close()
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception e)
            {
                await Navigation.PopModalAsync();
            }
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PollsPage(_polls, false));
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileInfo select = (e.CurrentSelection.FirstOrDefault() as FileInfo);
            try
            {
                if (@select != null) await Launcher.OpenAsync(@select.Link);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAdditionalLink, "OK");
            }
        }
    }
}