using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using System.Collections.Generic;
using System.Linq;
using Plugin.Fingerprint;
using Rg.Plugins.Popup.Services;
using xamarinJKH.DialogViews;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        private LoginResult Person = new LoginResult();
        private RestClientMP _server = new RestClientMP();
        public bool isSave  {get;set;}

        public bool useBio { get; set; }

        public bool GoodsIsVisible  {get;set;}
        bool _convert;
        public bool Convert
        {
            get => _convert;
            set
            {
                _convert = value;
                OnPropertyChanged("Convert");
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public ProfilePage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Профиль жителя");
            
            Convert = true;
            GoodsIsVisible = Settings.GoodsIsVisible;
            isSave = Preferences.Get("isPass", false);
            var ub = Preferences.Get("FingerPrintsOn", "");

            if (ub == "" || ub == "false")
                useBio = false;
            else
                useBio = true;

            NavigationPage.SetHasNavigationBar(this, false);

            var exitClick = new TapGestureRecognizer();
            exitClick.Tapped += async (s, e) =>
            {
                App.isStart = false;
                _ = await Navigation.PopModalAsync();
            };
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

            var ep = new TapGestureRecognizer();
            ep.Tapped += async (s, e) =>
            {
                await PopupNavigation.Instance.PushAsync(new EnterPin());

            };
            EditPin.GestureRecognizers.Add(ep);

            var dp = new TapGestureRecognizer();
            dp.Tapped += async (s, e) =>
            {
                Preferences.Remove("PinCode");
                await DisplayAlert("", $"{AppResources.Info} {AppResources.PinDeleted}", "ОК");
            };
            DeletePin.GestureRecognizers.Add(dp);


            FrameBtnExit.GestureRecognizers.Add(exitClick);
            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);

            var tAutoClick = new TapGestureRecognizer();
            tAutoClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => RadioButtonAuto.IsChecked = true); };
            tAuto.GestureRecognizers.Add(tAutoClick);

            var tBlackClick = new TapGestureRecognizer();
            tBlackClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => RadioButtonDark.IsChecked = true); };
            tDark.GestureRecognizers.Add(tBlackClick);

            var tLightClick = new TapGestureRecognizer();
            tLightClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => RadioButtonLigth.IsChecked = true); };
            tLight.GestureRecognizers.Add(tLightClick);

            var lRuClick = new TapGestureRecognizer();
            lRuClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => Russian.IsChecked = true); };
            lRu.GestureRecognizers.Add(lRuClick);

            var lEnClick = new TapGestureRecognizer();
            lEnClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => English.IsChecked = true); };
            lEn.GestureRecognizers.Add(lEnClick);

            var lUaClick = new TapGestureRecognizer();
            lUaClick.Tapped += (s, e) => { Device.BeginInvokeOnMainThread(() => Ukranian.IsChecked = true); };
            lUa.GestureRecognizers.Add(lUaClick);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        EntryEmail.FontSize = 10;                        
                    }
                    break;
                default:
                    break;
            }
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //BackgroundColor = Color.White;
                    ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    break;
                case Device.Android:
                    break;
                default:
                    break;
            }
            SetText();
            SetColor();
            EntryFio.Text = Settings.Person.FIO;
            EntryEmail.Text = Settings.Person.Email;
            BindingContext = this;

            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) =>
            {
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                var colors = new Dictionary<string, string>();
                var arrowcolor = new Dictionary<string, string>();
                if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
                {
                    colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                    arrowcolor.Add("#000000", "#494949");
                }
                else
                {
                    colors.Add("#000000", "#FFFFFF");
                    arrowcolor.Add("#000000", "#FFFFFF");
                }

                ImageBack.ReplaceStringMap = colors;
            });
        }
        
        private async void ButtonClick(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"^([a-zA-Z0-9А-Яа-я_-]+\.)*[a-zA-Z0-9А-Яа-я_-]+@[a-zA-Z0-9А-Яа-я_-]+(\.[a-zA-Z0-9А-Яа-я_-]+)*\.[a-zA-ZА-Яа-я]{2,6}$");
            if (!string.IsNullOrEmpty(EntryEmail.Text))
            {
                if (regex.IsMatch(EntryEmail.Text))
                {
                    SaveInfoAccount(EntryFio.Text, EntryEmail.Text);
                }
                else
                {
                    await DisplayAlert(null, AppResources.CorrectEmail, "OK");
                }
            }
            else
            {
                 await DisplayAlert(null, AppResources.CorrectEmail, "OK");
            }
        }
        
        public async void SaveInfoAccount(string fio, string email)
        {
            if (fio != "" && email != "")
            {
                progress.IsVisible = true;
                FrameBtnLogin.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.UpdateProfile(email, fio);
                if (result.Error == null)
                {
                    Settings.Person.FIO = fio;
                    Settings.Person.Email = email;
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", AppResources.SuccessProfile, "OK");             
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert("ОШИБКА", result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
            }
            else
            {
                if (fio == "" && email == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFills} {AppResources.FIO} {AppResources.And} E-mail", "OK");
                }else if (fio == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFill} {AppResources.FIO}", "OK");
                }else if (email == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFill} E-mail", "OK");
                }
            }
        }
        
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            //if (DeviceInfo.Platform == DevicePlatform.Android)
            //{
            PushEnable.IsVisible = !DependencyService.Get<ISettingsService>().IsEnabledNotification();
            //}
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Xamarin.Essentials.AppInfo.ShowSettingsUI();
        }

        void SetColor()
        {
            Color hexColor = (Color) Application.Current.Resources["MainColor"];

            UkName.Text = Settings.MobileSettings.main_name;
            IconViewSave.Foreground = Color.White;

            FrameBtnExit.BackgroundColor = Color.White;
            FrameBtnExit.BorderColor = hexColor;
            FrameBtnLogin.BackgroundColor = hexColor;
            LabelseparatorEmail.BackgroundColor = hexColor;
            LabelseparatorFio.BackgroundColor = hexColor;
            SwitchSavePass.OnColor = hexColor;
            SwitchSavePass.ThumbColor = Color.Black;

            SwitchUseBio.OnColor = hexColor;
            SwitchUseBio.ThumbColor = Color.Black;

            BtnExit.TextColor = hexColor;
            progress.Color = hexColor;

            RadioButtonAuto.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            RadioButtonDark.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            RadioButtonLigth.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            Russian.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            Ukranian.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            English.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));

            if (!Application.Current.Properties.ContainsKey("Culture"))
            {
                Application.Current.Properties.Add("Culture", string.Empty);
            }

            int theme = Preferences.Get("Theme", 0);
            var culture = CultureInfo.InstalledUICulture;

            switch (Application.Current.Properties["Culture"])
            {
                case "en-EN":
                case "en":English.IsChecked = true;
                    break;
                case "ru-RU":
                case "ru":Russian.IsChecked = true;
                    break;
                case "uk-UA":
                case "uk":Ukranian.IsChecked = true;
                    break;
            }
            
            switch (theme)
            {
                case 0:
                    RadioButtonAuto.IsChecked = true;
                    break;
                case 1:
                    RadioButtonDark.IsChecked = true;
                    break;
                case 2:
                    RadioButtonLigth.IsChecked = true;
                    break;
            }
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            FrameSettings.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            PushEnable.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            
        }

        
        private void SwitchSavePass_OnPropertyChanged(object sender, ToggledEventArgs toggledEventArgs)
        {
            Preferences.Set("isPass",isSave);
        }

        private async void SwitchUseBio_OnPropertyChanged(object sender, ToggledEventArgs toggledEventArgs)
        {
            if (toggledEventArgs.Value == true)
            {
                var a = await CrossFingerprint.Current.IsAvailableAsync();
                if (!a)
                {
                    await DisplayAlert(AppResources.Attention, AppResources.BiometricEnableToUse, "OK");
                    SwitchUseBio.IsToggled = false;
                }
                else
                  Preferences.Set("FingerPrintsOn", useBio.ToString().ToLower());
            }
            else
            Preferences.Set("FingerPrintsOn", useBio.ToString().ToLower());
        }

        private void RadioButtonDark_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Application.Current.UserAppTheme = OSAppTheme.Dark;
            MessagingCenter.Send<Object>(this, "ChangeTheme");
            MessagingCenter.Send<Object>(this, "ChangeThemeCounter");
            Preferences.Set("Theme", 1);

            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var colors = new Dictionary<string, string>();
            var arrowcolor = new Dictionary<string, string>();
            if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
            {
                colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                arrowcolor.Add("#000000", "#494949");
            }
            else
            {
                colors.Add("#000000", "#FFFFFF");
                arrowcolor.Add("#000000", "#FFFFFF");
            }

        }

        private void RadioButtonAuto_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            //только темная тема в ios 
            //if (Xamarin.Essentials.DeviceInfo.Platform != DevicePlatform.iOS)            
            //{
            switch (Settings.MobileSettings.appTheme)
            {
                case "":
                    Application.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;
                case "light": Application.Current.UserAppTheme = OSAppTheme.Light; break;
                case "dark": Application.Current.UserAppTheme = OSAppTheme.Dark; break;
            }

            MessagingCenter.Send<Object>(this, "ChangeTheme");
            MessagingCenter.Send<Object>(this, "ChangeThemeCounter");
                Preferences.Set("Theme", 0);
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var colors = new Dictionary<string, string>();
            var arrowcolor = new Dictionary<string, string>();
            if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
            {
                colors.Add("#000000", ((Color)Application.Current.Resources["MainColor"]).ToHex());
                arrowcolor.Add("#000000", "#494949");
            }
            else
            {
                colors.Add("#000000", "#FFFFFF");
                arrowcolor.Add("#000000", "#FFFFFF");
            }



        }

        private void RadioButtonLigth_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Application.Current.UserAppTheme = OSAppTheme.Light;
            MessagingCenter.Send<Object>(this, "ChangeTheme");
            MessagingCenter.Send<Object>(this, "ChangeThemeCounter");
            Preferences.Set("Theme", 2);
        }

        private async void GoBack(object sender, EventArgs args)
        {
            try
            {
                _ = await Navigation.PopAsync();
            }
            catch { }
        }

        private async void Russian_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (Application.Current.Properties["Culture"].ToString() != "ru")
            {
                await DisplayAlert(null, "Для того, чтобы изменения вступили в силу, необходимо перезапустить приложение", "OK");
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru");

            AppResources.Culture = new CultureInfo("ru");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru");

            Application.Current.Properties["Culture"] = "ru";
            await Application.Current.SavePropertiesAsync();
        }

        private async void English_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (Application.Current.Properties["Culture"].ToString() != "en")
            {
                await DisplayAlert(null, "In order for the changes to take effect, you must restart the application", "OK");
            }
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            AppResources.Culture = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");
            Application.Current.Properties["Culture"] = "en";
            await Application.Current.SavePropertiesAsync();
        }
        
        private async void Ukranian_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (Application.Current.Properties["Culture"].ToString() != "uk")
            {
                await DisplayAlert(null, "Для того, щоб зміни вступили в силу, необхідно перезапустити програму", "OK");
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("uk");

            AppResources.Culture = new CultureInfo("uk");
            Application.Current.Properties["Culture"] = "uk";
            await Application.Current.SavePropertiesAsync();
        }
    }
}