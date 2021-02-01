using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Essentials;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using Xamarin.Forms;
using xamarinJKH.Main;
using xamarinJKH.MainConst;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using NavigationPage = Xamarin.Forms.NavigationPage;
using AiForms.Dialogs;
using xamarinJKH.DialogViews;

using Rg.Plugins.Popup.Services;
using xamarinJKH.CustomRenderers;
using System.Globalization;
using System.Threading;
using Badge.Plugin;
using Microsoft.AppCenter.Analytics;

namespace xamarinJKH
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]

    public partial class MainPage : ContentPage
    {
        private RestClientMP server = new RestClientMP();
        Color _hex;
        public Dictionary<string, string> ColorHex {get;set;}
        public string adress
        {
            get;
            set;
        }
        public Color hex
        {
            get => _hex;
            set
            {
                _hex = value;
                OnPropertyChanged("hex");
            }
        }

        public MainPage()
        {
            adress = "sdf";
            CrossBadge.Current.ClearBadge();
            //Application.Current.Properties.Remove("Culture");
            if (Application.Current.Properties.ContainsKey("Culture"))
            {
                var culture = Application.Current.Properties["Culture"];
                if (culture != null)
                {

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture as string);

                    AppResources.Culture = new CultureInfo(culture as string);
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture as string);
                }

            }
            else
            {
                try
                {
                    Application.Current.Properties.Add("Culture", CultureInfo.CurrentUICulture.Name.Substring(0,2));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            InitializeComponent();
            getSettings();
            NavigationPage.SetHasNavigationBar(this, false);
            var startRegForm = new TapGestureRecognizer();
            startRegForm.Tapped += async (s, e) => { await Navigation.PushModalAsync(new RegistrForm(this)); };
            RegistLabel.GestureRecognizers.Add(startRegForm);
            
            var forgotPass = new TapGestureRecognizer();
            forgotPass.Tapped += async (s, e) =>
            {
                await DisplayAlert("Информация", "Для восстановления пароля, пройдите регистрацию повторно", "OK");
            };
            ForgotPass.GestureRecognizers.Add(forgotPass);

            var authConst = new TapGestureRecognizer();
            authConst.Tapped += ChoiceAuth;
            LabelSotr.GestureRecognizers.Add(authConst); 
            
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend;
            LabelTech.GestureRecognizers.Add(techSend);

            var startLogin = new TapGestureRecognizer();
            startLogin.Tapped += ButtonClick;
            FrameBtnLogin.GestureRecognizers.Add(startLogin);

            var forgetPasswordVisible = new TapGestureRecognizer();
            forgetPasswordVisible.Tapped += async (s, e) =>
            {
                EntryPass.IsPassword = !EntryPass.IsPassword;
                EntryPassConst.IsPassword = !EntryPassConst.IsPassword;
                if (EntryPass.IsPassword)
                {
                    ImageClosePass.ReplaceStringMap = new Dictionary<string, string>
                    {
                        {"#000000", $"#{Settings.MobileSettings.color}"}
                    }; 
                }
                else
                {
                    ImageClosePass.ReplaceStringMap = new Dictionary<string, string>
                    {
                        { "#000000", Color.DarkSlateGray.ToHex()}
                    }; 
                    
                }

            };
            ImageClosePass.GestureRecognizers.Add(forgetPasswordVisible);
            EntryLogin.Text = "";
            EntryLoginConst.Text = "";
            EntryPass.Text = "";
         
            string login = Preferences.Get("login", "");
            string pass = Preferences.Get("pass", "");
            string loginConst = Preferences.Get("loginConst", "");
            string passConst = Preferences.Get("passConst", "");
            bool isSave = Preferences.Get("isPass", false);
            if (Settings.MobileSettings.blockUserAuth)
            {
                Settings.ConstAuth = true;
                LabelSotr.IsVisible = false;
            }
            else
            {
                if (!Settings.MobileSettings.useDispatcherAuth)
                    LabelSotr.IsVisible = false;
                Settings.ConstAuth = Preferences.Get("constAuth", false);
            }
            if (Settings.ConstAuth && Settings.IsFirsStart && !passConst.Equals("") && !loginConst.Equals("") && !isSave)
            {
                LoginDispatcher(loginConst, passConst);
                Settings.IsFirsStart = false;
                EntryLogin.Text = login;
                EntryLoginConst.Text = loginConst;
                EntryPass.Text = pass;
                EntryPassConst.Text = passConst;
            }
            else if (Settings.IsFirsStart && !pass.Equals("") && !login.Equals("") && !isSave)
            {
                Login(login, pass);
                Settings.IsFirsStart = false;
                EntryLogin.Text = login;
                EntryLoginConst.Text = loginConst;
                EntryPass.Text = pass;
                EntryPassConst.Text = passConst;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                   
                    if (App.ScreenHeight < 600 || Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width<700)
                    {
                        RegistLabel.FontSize = 12;
                        ForgotPass.FontSize = 12;
                        RegStackLayout.Margin = new Thickness(0, 5, 0, 0);
                        BottomStackLayout.Margin = new Thickness(0, -20, 0, 20);
                    }

                    if(Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width<700)
                    {
                        LabelPhone.FontSize = 13;
                        LabelPassword.FontSize = 13;
                    }
                    break;
                case Device.Android:
                    break;
                default:
                    break;
            }
            var t = Application.Current.UserAppTheme;
        }


        protected override bool OnBackButtonPressed()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                Analytics.TrackEvent("Выход из приложения");
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }

            return base.OnBackButtonPressed();
        }

        async void CheckForUpdate()
        {
            Analytics.TrackEvent("Проверка обновлений");
            var version = Xamarin.Essentials.AppInfo.VersionString;
            var settings = await server.MobileAppSettings(version, "1");

            if (settings.Error != null && settings.Error.Contains("обновить"))
            {
                Device.BeginInvokeOnMainThread(async () => await Dialog.Instance.ShowAsync<UpdateNotificationDialog>());
            }
        }

        private async void getSettings()
        {
            Analytics.TrackEvent("Запрос настроек");
            Version.Text = "ver " + Xamarin.Essentials.AppInfo.VersionString;


            Settings.MobileSettings = await server.MobileAppSettings("4.02", "0");
            if (Settings.MobileSettings.Error == null)
            {
                
                UkName.Text = Settings.MobileSettings.main_name;
                var color = !string.IsNullOrEmpty(Settings.MobileSettings.color) ? $"#{Settings.MobileSettings.color}" :"#FF0000";
                try
                {
                    hex = Color.FromHex(color);
                }
                catch
                {
                    hex = Color.FromHex("#FF0000");
                }
                Application.Current.Resources["MainColor"] = hex;
                
                ColorHex = new Dictionary<string, string>
                {
                    { "#000000",  $"#{Settings.MobileSettings.color}" }
                };
                IconViewPass.ReplaceStringMap = ColorHex;
                ImageClosePass.ReplaceStringMap = ColorHex;
                ic_questions.ReplaceStringMap = ColorHex;
                BindingContext = this;
                SwitchLogin.ThumbColor = Color.White;
             
                Color.SetAccent(hex);
                FrameLogin.SetAppThemeColor(MaterialFrame.BorderColorProperty, hex, Color.White);
                BootomFrame.SetAppThemeColor(Frame.BorderColorProperty, hex, Color.LightGray);

                StackLayoutContent.IsVisible = true;
                progress2.IsVisible = false;
                IconViewNameUkLoad.IsVisible = false;
                FormattedString formatted = new FormattedString();
                formatted.Spans.Add(new Span
                {
                    Text = AppResources.Troub,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 13
                });
                formatted.Spans.Add(new Span
                {
                    Text = AppResources.WriteUs,
                    TextColor = hex,
                    FontSize = 13,
                    TextDecorations = TextDecorations.Underline
                });
                LabelTech.FormattedText = formatted;
                BindingContext = this;
            }
            else
            {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK");
                    getSettings();

            }

            
            if (Settings.ConstAuth)
            {
                Settings.ConstAuth = true;
                EntryLabel.Text =AppResources.ConstLogin;
                LabelSotr.Text = AppResources.DefaultLogin;
                LabelPhone.Text = AppResources.Login;
                RegStackLayout.IsVisible = false;
                EntryLogin.IsVisible = false;
                EntryLoginConst.IsVisible = true;
                EntryPass.IsVisible = false;
                EntryPassConst.IsVisible = true;
                LabelTitle.IsVisible = false;
                IconViewLogin.Source = "resource://xamarinJKH.Resources.ic_fio_reg.svg";
            }
            else
            {
                Settings.ConstAuth = false;
                EntryLabel.Text = AppResources.LoginAuth;
                LabelSotr.Text = AppResources.ConstLogin;
                LabelPhone.Text = AppResources.PhoneLabel;
                RegStackLayout.IsVisible = true;
                EntryLogin.IsVisible = true;
                EntryLoginConst.IsVisible = false;
                EntryPass.IsVisible = true;
                EntryPassConst.IsVisible = false;
                LabelTitle.IsVisible = true;
                IconViewLogin.Source = "resource://xamarinJKH.Resources.ic_phone_login.svg";
            }
        }

        private async void ChoiceAuth(object sender, EventArgs e)
        {
            Analytics.TrackEvent("Переход к входу сотрудника");
            if (Settings.ConstAuth)
            {
                Settings.ConstAuth = false;
                EntryLabel.Text = AppResources.LoginAuth;
                LabelSotr.Text = AppResources.ConstLogin;
                LabelPhone.Text = AppResources.PhoneLabel;
                RegStackLayout.IsVisible = true;
                EntryLogin.IsVisible = true;
                EntryLoginConst.IsVisible = false;
                EntryPass.IsVisible = true;
                EntryPassConst.IsVisible = false;
                LabelTitle.IsVisible = true;
                IconViewLogin.Source = "resource://xamarinJKH.Resources.ic_phone_login.svg";
            }
            else
            {
                Settings.ConstAuth = true;
                EntryLabel.Text = AppResources.ConstLogin;
                LabelSotr.Text = AppResources.DefaultLogin;
                LabelPhone.Text = AppResources.Login;
                RegStackLayout.IsVisible = false;
                EntryLogin.IsVisible = false;
                EntryLoginConst.IsVisible = true;
                EntryPass.IsVisible = false;
                EntryPassConst.IsVisible = true;
                LabelTitle.IsVisible = false;
                IconViewLogin.Source = "resource://xamarinJKH.Resources.ic_fio_reg.svg";
            }
        }

        private async void TechSend(object sender, EventArgs e)
        {
            
            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            string phone = Preferences.Get("techPhone", Settings.Person.Phone);
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
            {
                Settings.SetPhoneTech(phone);
                await server.RegisterDeviceNotAvtorization(Settings.Person.Phone);
                await Navigation.PushModalAsync(new AppPage());
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }
        

        private async void ButtonClick(object sender, EventArgs e)
        {
            if (Settings.ConstAuth)
            {
                LoginDispatcher(EntryLoginConst.Text, EntryPassConst.Text);
            }
            else
            {
                Login(EntryLogin.Text, EntryPass.Text);
            }            
        }

        public async void Login(string loginAuth, string pass)
        {
            Analytics.TrackEvent("Авторизация пользователя");
            progress.IsVisible = true;
            FrameBtnLogin.IsVisible = false;

            var replace = !string.IsNullOrEmpty(loginAuth) ? loginAuth
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "") : null;

            if (!string.IsNullOrEmpty(replace) && !string.IsNullOrEmpty(pass))
            {
                if (replace.Length < 11)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorTechNumberFormat, "OK");
                    progress.IsVisible = false;
                    FrameBtnLogin.IsVisible = true;
                    return;
                }

                LoginResult login = await server.Login(replace, pass);
                if (login.Error == null)
                {
                    Settings.Person = login;
                    ItemsList<RequestType> result = await server.GetRequestsTypes();
                    Settings.TypeApp = result.Data;
                    Preferences.Set("login", replace);
                    Preferences.Set("pass", pass);
                    Preferences.Set("constAuth", false);
                    
                    await Navigation.PushModalAsync(new BottomNavigationPage());
                }
                else
                {
                    if (login.Error.ToLower().Contains("unauthorized"))
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorUserNotFound, "OK");
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFills, "OK");
            }

            progress.IsVisible = false;
            FrameBtnLogin.IsVisible = true;
        }

        public async void LoginDispatcher(string loginAuth, string pass)
        {
            Analytics.TrackEvent("Авторизация сотрудника");
            progress.IsVisible = true;
            FrameBtnLogin.IsVisible = false;

            var replace = loginAuth;
            if (!string.IsNullOrEmpty(replace) && !string.IsNullOrEmpty(pass)) 
            {

                LoginResult login = await server.LoginDispatcher(replace, pass);
                if (login.Error == null)
                {
                    App.isCons = true;
                    Settings.Person = login;
                    ItemsList<RequestType> result = await server.GetRequestsTypesConst();
                    Settings.TypeApp = result.Data;
                    Preferences.Set("loginConst", replace);
                    Preferences.Set("passConst", pass);
                    Preferences.Set("constAuth", true);
                    await Navigation.PushModalAsync(new BottomNavigationConstPage());
                }
                else
                {
                    if (login.Error.ToLower().Contains("unauthorized"))
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorUserNotFound, "OK");
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, login.Error, "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFills, "OK");
            }

            progress.IsVisible = false;
            FrameBtnLogin.IsVisible = true;
        }
    }
}