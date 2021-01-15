using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OSSPersonalVotingResult : ContentPage
    {
        Color colorFromMobileSettings = (Color)Application.Current.Resources["MainColor"];
        public bool forsvg { get; set; }

        public OSSPersonalVotingResult(OSS oSS, bool userFinishPool=false)
        {
            InitializeComponent();
            Analytics.TrackEvent("Персональные результаты голосования ОСС");
            if (oSS.Accounts.Count > 0)
            {
                OSSAccount oSsAccount = oSS.Accounts[0];
                ProtokolStackL.IsVisible = oSsAccount.HasVoitingBlankFile;
                urlProtokol.TextColor = colorFromMobileSettings;
                urlProtokol.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => await Launcher.OpenAsync(oSsAccount.VoitingBlankFileLink))
                });
            }
            else
            {
                ProtokolStackL.IsVisible = false;
            }
            forsvg = false;
            this.BindingContext = this;

            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            if (userFinishPool)
            {
                listNeedUpdate = userFinishPool;
            }
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {    await Navigation.PushAsync(new AppPage());};
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
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            var dH = DeviceDisplay.MainDisplayInfo.Height;
            if (dH < 1400)
            {
                titleLabel.FontSize = 18;
            }

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { ClosePage(); };
            BackStackLayout.GestureRecognizers.Add(backClick);

            SetDecorations();



            SetData(oSS);

        }

        void SetDecorations()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            Btn.BackgroundColor = colorFromMobileSettings;


            
            StackLayout statusNameIcon = new StackLayout() { Orientation = StackOrientation.Vertical, HorizontalOptions = LayoutOptions.FillAndExpand };

            Label statusName = new Label()
            {
                Text = AppResources.OSSInfoStatus,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.Start
            };
            statusName.SetAppThemeColor(Label.TextColorProperty, Color.Black, Color.White);
            statusNameIcon.Children.Add(statusName);

            StackLayout coloredStatus = new StackLayout() { HorizontalOptions = LayoutOptions.Start, Orientation = StackOrientation.Horizontal };
            IconView iconViewStatusNameIcon = new IconView();

            Color ColorStatusTextString;
            
            iconViewStatusNameIcon.Source = "ic_status_done";
            iconViewStatusNameIcon.Foreground = Color.FromHex("#50ac2f");
            ColorStatusTextString = Color.FromHex("#50ac2f");
            string textStatius = AppResources.OSSVoteChecked;

            iconViewStatusNameIcon.HeightRequest = 15;
            iconViewStatusNameIcon.WidthRequest = 15;
            iconViewStatusNameIcon.VerticalOptions = LayoutOptions.Center;
            iconViewStatusNameIcon.HorizontalOptions = LayoutOptions.Start;

            coloredStatus.Children.Add(iconViewStatusNameIcon);

            Label statSting = new Label() { Text = textStatius, FontSize = 12, TextColor = ColorStatusTextString, HorizontalOptions = LayoutOptions.Start };
            coloredStatus.Children.Add(statSting);

            statusNameIcon.Children.Add(coloredStatus);

            status.Children.Add(statusNameIcon);

        }

        private void SetData(OSS oSS)
        {
            //проголосовано * из *
            spanAnswersCnt.TextColor = colorFromMobileSettings;
            var cntVotes = "за "+ oSS.Questions.Where(_ => !string.IsNullOrWhiteSpace(_.Answer)).Count().ToString() + " / " + oSS.Questions.Count.ToString()+".";
            spanAnswersCnt.Text = cntVotes;

            //ответы по штукам
            lCntYes.Text = oSS.Questions.Where(_ => _.Answer == "0").Count().ToString();
            cntYes.Foreground = colorFromMobileSettings;
            lCntNo.Text = oSS.Questions.Where(_ => _.Answer == "1").Count().ToString();
            cntNo.Foreground = colorFromMobileSettings;

            lCntAbstained.Text = oSS.Questions.Where(_ => _.Answer == "2").Count().ToString();
            cntAbstained.Foreground = colorFromMobileSettings;


            delimColored.BackgroundColor = colorFromMobileSettings;

            TotalArea.Text = " "+ oSS.VoitingArea.ToString()+ $" {AppResources.OSSInfoMeasurmentArea} = 100%";
            decimal round = 0;
            try
            { 
                round = Math.Round(oSS.Accounts[0].Area/oSS.VoitingArea*100,3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Area.Text= " " + oSS.Accounts[0].Area.ToString() + $" {AppResources.OSSInfoMeasurmentArea} = "+ round + "%";
            dayEnd.Text = oSS.DateEnd.Split(' ')[0];
            var r1Date = oSS.ResultsReleaseDate.Split(' ')[0];
            var r1Time = oSS.ResultsReleaseDate.Split(' ')[1];
            dayEndPlus.Text =" " + AppResources.OSSText.Replace("{r1Date}", r1Date).Replace("{r1Time}", r1Time);
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            
            PancakeBot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            FrameResult.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);

        }

        bool listNeedUpdate = false;

        private async void Btn_Clicked(object sender, EventArgs e)
        {            
            if (listNeedUpdate)
                if (Navigation.NavigationStack.FirstOrDefault(x => x is OSSMain) == null)
                    await Navigation.PushAsync(new OSSMain());
                else
                    PopUntilDestination(typeof(OSSMain));
        }

        void PopUntilDestination(Type DestinationPage)
        {
            int LeastFoundIndex = 0;
            int PagesToRemove = 0;

            for (int index = Navigation.NavigationStack.Count - 2; index > 0; index--)
            {
                if (Navigation.NavigationStack[index].GetType().Equals(DestinationPage))
                {
                    break;
                }
                else
                {
                    LeastFoundIndex = index;
                    PagesToRemove++;
                }
            }

            for (int index = 0; index < PagesToRemove; index++)
            {
                Navigation.RemovePage(Navigation.NavigationStack[LeastFoundIndex]);
            }

            ClosePage();
        }

        async void ClosePage()
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
    }
}