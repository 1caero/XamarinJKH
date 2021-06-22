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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Badge.Plugin;
using Microsoft.AppCenter.Analytics;
using xamarinJKH.InterfacesIntegration;
using Plugin.Fingerprint;
using Realms;
using xamarinJKH.Utils.ReqiestUtils;

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
/// <summary>
/// Точка входа в приложение и экран авторизации
/// </summary>
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
            if (RestClientMP.SERVER_ADDR.Contains("kapitalinvest_uk_kapitalinvest"))
            {
                IconViewNameUk.Source = "logo_app_invest.png";
                IconViewNameUk.Aspect = Aspect.AspectFill;
                IconViewNameUk.WidthRequest = 150;
            }
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
            // EntryLogin.Text = "";
            EntryLoginConst.Text = "";
            EntryPass.Text = "";
          
            // OpenCarousel();
            AutoLogin();

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

        async void OpenCarousel()
        {
            if (Preferences.Get("IsFirstStart", true) && Settings.MobileSettings.MockupCount > 0)
            {
                await Navigation.PushModalAsync(new MockupPage());
                Preferences.Set("IsFirstStart", false);
            }
        }
        /// <summary>
        /// Функция для автоматического входа в приложение
        /// </summary>
        private void AutoLogin()
        {
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
                IconViewNameUkLoad.IsVisible = true;
                StackLayoutContent.IsVisible = false;
                LoginDispatcher(loginConst, passConst);
                Settings.IsFirsStart = false;
                EntryLogin.Text = login;
                EntryLoginConst.Text = loginConst;
                EntryPass.Text = pass;
                EntryPassConst.Text = passConst;
            }
            else if (Settings.IsFirsStart && !pass.Equals("") && !login.Equals("") && !isSave)
            {
                IconViewNameUkLoad.IsVisible = true;
                StackLayoutContent.IsVisible = false;
                Login(login, pass);
                Settings.IsFirsStart = false;
                EntryLogin.Text = login;
                EntryLoginConst.Text = loginConst;
                EntryPass.Text = pass;
                EntryPassConst.Text = passConst;
            }
            Device.StartTimer(new TimeSpan(0, 1, 0), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IconViewNameUkLoad.IsVisible = false;
                    StackLayoutContent.IsVisible = true;
                    BottomStackLayout.IsVisible = true;
                });
                return false; // runs again, or false to stop
            });
        }

        /// <summary>
        /// Обработка нажатия физической кнопки назад на устройстве
        /// </summary>
        /// <returns>true если успешная обработка иначе false</returns>
        protected override bool OnBackButtonPressed()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                Analytics.TrackEvent("Выход из приложения");
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }

            return base.OnBackButtonPressed();
        }
        /// <summary>
        /// Проверка наличия более новых версий приложения в магазинах
        /// </summary>
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
        /// <summary>
        /// Функция получения настроек приложения с сервера
        /// </summary>
        private async void getSettings()
        {
            Analytics.TrackEvent("Запрос настроек");
            string versionString = Xamarin.Essentials.AppInfo.VersionString;
            Version.Text = "ver " + versionString;


            Settings.MobileSettings = await server.MobileAppSettings(versionString, "0");
            if (Settings.MobileSettings.Error == null)
            {
                
                UkName.Text = UkNameLoading .Text = Settings.MobileSettings.main_name;
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

                // StackLayoutContent.IsVisible = true;
                // progress2.IsVisible = false;
                // IconViewNameUkLoad.IsVisible = false;
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
                EntryLabel.Text =AppResources.ConstLogin + "\n";
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
                EntryLabel.Text = AppResources.LoginAuth + "\n";
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
        /// <summary>
        /// Обработчик нажатия на кнопку "вход для сотрудника" или "вход для жителя"
        /// </summary>
        /// <param name="sender">Целевой объект нажатия</param>
        /// <param name="e">События</param>
        private async void ChoiceAuth(object sender, EventArgs e)
        {
            Analytics.TrackEvent("Переход к входу сотрудника");
            if (Settings.ConstAuth)
            {
                Settings.ConstAuth = false;
                EntryLabel.Text = AppResources.LoginAuth+ "\n";
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
                EntryLabel.Text = AppResources.ConstLogin+ "\n";
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
        /// <summary>
        /// Обработка нажатия на кнопку "Обратиться в тех.поддержку"
        /// открывается форма обращения в тех поддержку
        /// </summary>
        /// <param name="sender">Целевой объект нажатия</param>
        /// <param name="e">События</param>
        private async void TechSend(object sender, EventArgs e)
        {
            
            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            string phone = Preferences.Get("techPhone", Settings.Person.Phone);
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
            {
                Settings.SetPhoneTech(phone);
                await server.RegisterDeviceNotAvtorization(Settings.Person.Phone);
                if (Navigation.ModalStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushModalAsync(new AppPage(true));
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog(true, true));
            }
        }
        
        /// <summary>
        /// Обработка нажатия на кнопку войти
        /// </summary>
        /// <param name="sender">Целевой объект нажатия</param>
        /// <param name="e">События</param>
        private async void ButtonClick(object sender, EventArgs e)
        {
            if (Settings.ConstAuth)
            {
                LoginDispatcher(EntryLoginConst.Text, EntryPassConst.Text,true);
            }
            else
            {
                Login((string)EntryLogin.Text, EntryPass.Text, true);
            }            
        }

        async Task AddPin()
        {            
            if(pin=="")
            {
                //var addPin = await DisplayActionSheet("Хотите установить пин-код для входа в приложение?", AppResources.NoThanks, null, new string[] { AppResources.Yes });
                //if (addPin == AppResources.Yes)
                Preferences.Set("DisplayPinAdd", true);
                if (Preferences.Get("DisplayPinAdd", true))
                {
                    await PopupNavigation.Instance.PushAsync(new PinEnableCheck());
                }                
            }
           
        }

        async Task<bool> PinCorrectAsync()
        {
                var pinEntered = await DisplayPromptAsync("Вход", "Введите пин-код для входа", AppResources.LoginAuth, AppResources.Cancel, "ваш пин-код", -1, Keyboard.Numeric, "");

                return pinEntered!=null && pinEntered.Equals(pin);                
        }

        string pin = "";
        bool cleanFilelds = false;
        /// <summary>
        /// Авторизация жителя в приложении
        /// </summary>
        /// <param name="loginAuth">Логин</param>
        /// <param name="pass">Пароль</param>
        /// <param name="isButtonClick">Флаг отвечающий за проверку использования авторизации по биометрии</param>
        public async void Login(string loginAuth, string pass, bool isButtonClick=false)
        {
            Analytics.TrackEvent("Авторизация пользователя");
            progress.IsVisible = true;
            FrameBtnLogin.IsVisible = false;

            //Биометрия
            var displayPassAlert = true;

            var a = await CrossFingerprint.Current.IsAvailableAsync();
            //var aA = await CrossFingerprint.Current.GetAvailabilityAsync();
            //var at = await CrossFingerprint.Current.GetAuthenticationTypeAsync();
#if DEBUG
           // Preferences.Set("PinCode", "");
#endif

            pin = Preferences.Get("PinCode", "");

            if (!isButtonClick)
            {
                var b = Preferences.Get("FingerPrintsOn", "");
                await AddPin();

                pin = Preferences.Get("PinCode", "");
                //биометрия не установлена вообще, предлогаем ее включить, если доступна  
                if (b == "")
                {                                      
                    if (!a)
                    {
                        await DisplayAlert(AppResources.Attention, AppResources.BiometricNA, "OK");
                    }
                    else
                    {
                        bool answer = await DisplayAlert(AppResources.Attention, AppResources.BiometricAddDialog, AppResources.Yes, AppResources.No);
                        if (answer)
                        {
                            Preferences.Set("FingerPrintsOn", "true");
                            b = "true";
                        }
                        else
                        {
                            Preferences.Set("FingerPrintsOn", "false");
                            b = "false";
                        }
                    }
                    displayPassAlert = await PinCheckAsync();
                }

                if (b == "true")
                {//биометрия назначена ранее                    
                    if (!a)
                    {
                        await DisplayAlert(AppResources.Attention, AppResources.BiometricNA, "OK");
                    }
                    else
                    {
                        var ar = await CrossFingerprint.Current.AuthenticateAsync(
                       new Plugin.Fingerprint.Abstractions.AuthenticationRequestConfiguration(AppResources.Attention, AppResources.BiometricUseDialog));
                        if (!ar.Authenticated)
                        {
                            await DisplayAlert(AppResources.Attention, AppResources.BiometricNotRecognizedDialog, "OK");

                            displayPassAlert = await PinCheckAsync();
                        }
                    }                   
                }

                if (b == "false")
                {//биометрия отключена пользователем, делаем автовход если как это было раньше
                    displayPassAlert = await PinCheckAsync();
                }

                if (cleanFilelds)
                {
                    loginAuth = "";
                    pass = "";
                }
            }           

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
                if(displayPassAlert)
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFills, "OK");
            }
            IconViewNameUkLoad.IsVisible = false;
            StackLayoutContent.IsVisible = true;
            progress.IsVisible = false;
            FrameBtnLogin.IsVisible = true;
        }
        /// <summary>
        /// Авторизация сотрудника УК в приложении
        /// </summary>
        /// <param name="loginAuth">Логин</param>
        /// <param name="pass">Пароль</param>
        /// <param name="isButtonClick">Флаг отвечающий за проверку использования авторизации по биометрии</param>
        public async void LoginDispatcher(string loginAuth, string pass, bool isButtonClick = false)
        {
            Analytics.TrackEvent("Авторизация сотрудника");
            progress.IsVisible = true;
            FrameBtnLogin.IsVisible = false;

            //Биометрия
#if DEBUG
            Preferences.Set("PinCode", "");
#endif
            pin = Preferences.Get("PinCode", "");

            var displayPassAlert = true;
            if (!isButtonClick)
            {
                var b = Preferences.Get("FingerPrintsOnCo", "");

                await AddPin();

                pin = Preferences.Get("PinCode", "");

                if (b == "")
                {
                    //биометрия не установлена вообще, предлогаем ее включить, если доступна
                    var a = await CrossFingerprint.Current.IsAvailableAsync();
                    if (!a)
                    {
                        await DisplayAlert(AppResources.Attention, AppResources.BiometricNA, "OK");
                    }
                    else
                    {
                        bool answer = await DisplayAlert(AppResources.Attention, AppResources.BiometricAddDialog, AppResources.Yes, AppResources.No);
                        if (answer)
                        {
                            Preferences.Set("FingerPrintsOnCo", "true");
                            b = "true";
                        }
                        else
                        {
                            Preferences.Set("FingerPrintsOnCo", "false");
                            b = "false";
                        }
                    }

                    displayPassAlert = await PinCheckAsync();
                }

                if (b == "true")
                {//биометрия назначена ранее
                    var ar = await CrossFingerprint.Current.AuthenticateAsync(
                        new Plugin.Fingerprint.Abstractions.AuthenticationRequestConfiguration(AppResources.Attention, AppResources.BiometricUseDialog));
                    if (!ar.Authenticated)
                    {
                        await DisplayAlert(AppResources.Attention, AppResources.BiometricNotRecognizedDialog, "OK");
                        
                        displayPassAlert = await PinCheckAsync();
                    }
                }

                if (b == "false")
                {//биометрия отключена пользователем, делаем автовход если как это было раньше
                    displayPassAlert = await PinCheckAsync();
                }              
                
               if(cleanFilelds)
                {
                    loginAuth = "";
                    pass = "";
                }

            }


            var replace = loginAuth;
            if (!string.IsNullOrEmpty(replace) && !string.IsNullOrEmpty(pass)) 
            {

                LoginResult login = await server.LoginDispatcher(replace, pass);
                if (login.Error == null)
                {
                    App.isCons = true;
                    Settings.Person = login;
                    await Task.Factory.StartNew(async () =>
                    {
                        ItemsList<RequestType> result = await server.GetRequestsTypesConst();
                        Settings.TypeApp = result.Data;
                    });
                    await Task.Factory.StartNew(async () =>
                    {
                        ItemsList<NamedValue> resultStatus = await server.RequestStatuses();
                        Settings.StatusApp = resultStatus.Data;
                    });
                    await Task.Factory.StartNew(async () =>
                    {
                      ItemsList<NamedValue> resultPrioritets = await server.RequestPriorities();
                      Settings.PrioritetsApp = resultPrioritets.Data;
                    });
                   
                    await Task.Factory.StartNew( RequestUtils.UpdateRequestCons);
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
                if(displayPassAlert)
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFills, "OK");
            }
            IconViewNameUkLoad.IsVisible = false;
            StackLayoutContent.IsVisible = true;
            BottomStackLayout.IsVisible = true;
            
            progress.IsVisible = false;
            FrameBtnLogin.IsVisible = true;
        }

        async Task<bool> PinCheckAsync()
        {
            if (pin != "")
            {
                bool pinCorrect = false;
                pinCorrect = await PinCorrectAsync();
                if (!pinCorrect )
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        EntryLogin.Text = "";
                        EntryPass.Text = "";
                        EntryLoginConst.Text = "";                        
                        EntryPassConst.Text= "";
                    });

                    cleanFilelds = true;

                    //return true;
                }
                //return false;
            }
            return false;
        }
    }
}