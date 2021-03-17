using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration;
using xamarinJKH.CustomRenderers;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;
using Xamarin.Forms.PancakeView;
using xamarinJKH.DialogViews;
using Microsoft.AppCenter.Crashes;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrForm : ContentPage
    {
        private LoginResult Person = new LoginResult();
        private RestClientMP _server = new RestClientMP();
        public string LoginAuth { get; set; }
        public string passAuth { get; set; }
        private bool isDate = false;
        private int step = 0;

        bool TimerStart = false;
        int TimerTime = 59;

        private MainPage _mainPage;

        public Color hex { get; set; }
        public Color hexWhite { get; set; }

        private bool isNext = false;
        RegistrFormViewModel viewModel { get; set; }
        public bool ShowBirthDay => Settings.MobileSettings.requireBirthDate;

        public RegistrForm(MainPage mainPage)
        {
            Analytics.TrackEvent("Регистрация");
            InitializeComponent();
            viewModel = new RegistrFormViewModel(this.Navigation);
            this.BindingContext = viewModel;
            UkName.Text = viewModel.Title;
            hex = (Color) Application.Current.Resources["MainColor"];
            hexWhite = hex.AddLuminosity(0.3);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //BackgroundColor = Color.White;
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    BackStackLayout.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    //BackStackLayout.Margin = new Thickness(10, 20, 0, 0);
                    RegLbl.Margin = new Thickness(20, 60, 0, 0);

                    //DOB.IsVisible = false;
                    //DOBSeparator.IsVisible = false;

                    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        DOBLabel.FontSize = 12;
                        LabelTimer.FontSize = 10;
                        lblConfirmPass.FontSize = 10;
                        lblCreatePass.FontSize = 10;
                    }


                    break;
                case Device.Android:
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        BackStackLayout.Margin = new Thickness(10, -150, 0, 0);
                    }

                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += BackClick;
            BackStackLayout.GestureRecognizers.Add(backClick);
            FrameTimer.IsVisible = false;
            var nextReg = new TapGestureRecognizer();
            nextReg.Tapped += async (s, e) => { FirstStepReg(); };
            FrameBtnLogin.GestureRecognizers.Add(nextReg);
            var sendCheckCode = new TapGestureRecognizer();
            sendCheckCode.Tapped += async (s, e) => { SendCheckCode(false); };
            ToSms.GestureRecognizers.Add(sendCheckCode); 
            var sendCheckCodeWhatsApp = new TapGestureRecognizer();
            sendCheckCodeWhatsApp.Tapped += async (s, e) => { SendCheckCode(true); };
            ToSmsWhatsApp.GestureRecognizers.Add(sendCheckCodeWhatsApp); 
            var personPolicity = new TapGestureRecognizer();
            personPolicity.Tapped += PersonPolicity;
            LabelPersonPolicity.GestureRecognizers.Add(personPolicity);  
            
            var callClick = new TapGestureRecognizer();
            callClick.Tapped += async (s, e) => { RegCodeRequest(FrameBtnReg, null); };
            FrameBtnReg.GestureRecognizers.Add(callClick);

            var nextReg2 = new TapGestureRecognizer();
            nextReg2.Tapped += async (s, e) =>
            {
                if (isNext)
                    SecondStep();
            };
            FrameBtnNextTwo.GestureRecognizers.Add(nextReg2);

            var finalReg = new TapGestureRecognizer();
            finalReg.Tapped += async (s, e) => { FinalRegg(); };
            FrameBtnRegFinal.GestureRecognizers.Add(finalReg);

            _mainPage = mainPage;
            var passwordVisible = new TapGestureRecognizer();
            passwordVisible.Tapped += async (s, e) =>
            {
                EntryPassNew.IsPassword = !EntryPassNew.IsPassword;
                if (EntryPassNew.IsPassword)
                {
                    LayoutConfirm.IsVisible = true;
                    LayoutConfirmLines.IsVisible = true;
                    ImageClosePass.ReplaceStringMap = new Dictionary<string, string>
                    {
                        {"#000000", $"#{Settings.MobileSettings.color}"}
                    };
                }
                else
                {
                    LayoutConfirm.IsVisible = false;
                    LayoutConfirmLines.IsVisible = false;
                    ImageClosePass.ReplaceStringMap = new Dictionary<string, string>
                    {
                        {"#000000", Color.DarkSlateGray.ToHex()}
                    };
                }
            };
            ImageClosePass.GestureRecognizers.Add(passwordVisible);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    if (App.ScreenHeight < 600)
                    {
                        EntryPassNew.FontSize = 12;
                        EntryPassCommit.FontSize = 12;
                    }

                    break;
                case Device.Android:
                    break;
                default:
                    break;
            }

            BootomFrame.SetAppThemeColor(Frame.BorderColorProperty, hex, Color.LightGray);

            RegistrationFrameStep1.SetAppThemeColor(PancakeView.BorderColorProperty, hex, Color.White);
            RegistrationFrameStep2.SetAppThemeColor(PancakeView.BorderColorProperty, hex, Color.White);
            RegistrationFrameStep3.SetAppThemeColor(PancakeView.BorderColorProperty, hex, Color.White);

            //RegistrationFrameStep1.SetAppThemeColor(MaterialFrame.BorderColorProperty, hex, Color.White);
            //RegistrationFrameStep2.SetAppThemeColor(MaterialFrame.BorderColorProperty, hex, Color.White);
            //RegistrationFrameStep3.SetAppThemeColor(MaterialFrame.BorderColorProperty, hex, Color.White);
            BindingContext = this;
        }

        private async void SendCheckCode(bool isWhatsApp)
        {
            isSmsOrCall = true;
             Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };
            if (!pressed)
                try
                {
                    pressed = true;
                    await Loading.Instance.StartAsync(async progress =>
                    {
                        if (isWhatsApp)
                        {
                            bool loadUrl = await LoadUrl("com.whatsapp");
                        }
                        CommonResult result = await _server.SendCheckCode(Person.Phone, isWhatsApp);
                        if (result.Error == null)
                        {
                            Console.WriteLine("Отправлено");
                            TimerStart = true;
                            SendCode.IsVisible = false;
                            FrameTimer.IsVisible = true;
                            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
                            setDisabledSms(true);
                           
                            if (Device.RuntimePlatform == Device.iOS)
                            {
                                await DisplayAlert("", AppResources.AlertCodeSent, "OK");
                            }
                            else
                            {
                                DependencyService.Get<IMessage>().ShortAlert(AppResources.AlertCodeSent);
                            }

                            // FrameBtnReg.IsVisible = true;
                            // progress.IsVisible = false;

                            Loading.Instance.Hide();
                        }
                        else
                        {
                            // FrameBtnReg.IsVisible = true;
                            // progress.IsVisible = false;
                            Loading.Instance.Hide();
                            if (Device.RuntimePlatform == Device.iOS)
                            {
                                await DisplayAlert("", result.Error, "OK");
                            }
                            else
                            {
                                DependencyService.Get<IMessage>().ShortAlert(result.Error);
                            }
                        }

                        pressed = false;
                    });
                }
                catch
                {
                }
        }
        private async Task<bool> LoadUrl(string package)
        {
            try
            {
                if (Device.RuntimePlatform == "Android")
                {
                    if (DependencyService.Get<IOpenApp>().IsOpenApp(package))
                    {
                        return true;
                    }
                    else
                    {
                        await Launcher.OpenAsync($"https://play.google.com/store/apps/details?id={package}");
                    }
                }
                else
                {
                    if (DependencyService.Get<IOpenApp>().IsOpenApp(package))
                    {
                        return true;
                    }
                    else
                    {
                        var uri = $"https://itunes.apple.com/us/app/";

                        if (package.Contains("what"))
                            uri += "whatsapp-messenger/id310633997";

                        if (package.Contains("teleg"))
                            uri += "telegram-messenger/id686449807";

                        if (package.Contains("viber"))
                            uri += "viber-messenger-chats-calls/id382617920";

                        //await Launcher.OpenAsync(url);
                        await Launcher.OpenAsync(uri);

                    }

                }
                // await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAdditionalLink, "OK");
            }
            return false;
        }

        private void NextReg(object sender, EventArgs e)
        {
            FirstStepReg();
        }

        private async void Tech(object sender, EventArgs e)
        {
            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            string phone = Preferences.Get("techPhone", Settings.Person.Phone);
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
            {
                Settings.SetPhoneTech(phone);
                await _server.RegisterDeviceNotAvtorization(Settings.Person.Phone);
                await Navigation.PushModalAsync(new AppPage(true));
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog(true,true));
            }
        }
        private async void PersonPolicity(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new PersonalPolicityDialog());
        }

        void returnOneStep()
        {
            RegistrationFrameStep1.IsVisible = true;
            RegistrationFrameStep2.IsVisible = false;
            tech.IsVisible = false;
            LabelSteps.Text = $"{AppResources.Step} 1";
            StepsImage.Source = ImageSource.FromFile("ic_steps_one");
            step = 0;
        }

        void returnTwoStep()
        {
            RegistrationFrameStep2.IsVisible = true;
            RegistrationFrameStep3.IsVisible = false;
            tech.IsVisible = true;
            StepsImage.Source = ImageSource.FromFile("ic_steps_two");
            LabelSteps.Text = $"{AppResources.Step} 2";
            step = 1;
        }

        private async void FirstStepReg()
        {
            Analytics.TrackEvent("Певый шаг регистрации");
            string phone = EntryPhone.Text
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "");
            string fio = EntryFio.Text;
            string date = DatePicker.Date.ToString("dd.MM.yyyy");

            try
            {
                date = DateEntry.Text;
                DateTime dateTime =  DateTime.ParseExact(date, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
                isDate = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                isDate = false;
            }
            
            if (phone.Equals(""))
            {
                await DisplayAlert(AppResources.ErrorTitle, $"{AppResources.ErrorFill} {AppResources.Phone}", "OK");
            }
            else if (phone.Length < 11)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorTechNumberFormat, "OK");
            }
            else if (fio.Equals(""))
            {
                await DisplayAlert(AppResources.ErrorTitle, $"{AppResources.ErrorFill} {AppResources.FIO}", "OK");
            }
            else if (!SwitchConsent.IsToggled)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ProcessUserData, "OK");
            }
            else if (!isDate && Settings.MobileSettings.requireBirthDate)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.DatePick, "OK");
            }
            else
            {
                Person.Phone = phone;
                Person.FIO = fio;
                Person.Birthday = date;
                FormattedString formatted = new FormattedString();
                var message = AppResources.CodeInfo.Split("PHONE");
                formatted.Spans.Add(new Span
                {
                    Text =
                        message[0],
                });
                formatted.Spans.Add(new Span
                {
                    Text = EntryPhone.Text,
                    TextColor = Color.Black,
                });
                // formatted.Spans.Add(new Span
                // {
                //     Text = message[1],
                // });

                LabelTitleRequestCode.FormattedText = formatted;
                StepsImage.Source = ImageSource.FromFile("ic_steps_two");
                RegistrationFrameStep1.IsVisible = false;
                RegistrationFrameStep2.IsVisible = true;
                tech.IsVisible = true;
                step = 1;
                LabelSteps.Text = $"{AppResources.Step} 2";
                if (Settings.TimerStart)
                {
                    Settings.TimerStart = false;
                    TimerStart = true;
                    TimerTime = Settings.TimerTime;
                    LabelTimer.Text = AppResources.AskForCodeAgain.Replace("TimerTime", TimerTime.ToString());
                    SendCode.IsVisible = false;
                    StackLayoutSms.IsVisible = false;
                    FrameTimer.IsVisible = true;
                    Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
                }
            }
        }

        private async void BackClick(object sender, EventArgs e)
        {
            switch (step)
            {
                case 0:
                    if (TimerStart)
                    {
                        TimerStart = false;
                        Settings.TimerStart = true;
                        Settings.TimerTime = TimerTime;
                        Device.StartTimer(TimeSpan.FromSeconds(1), Settings.OnTimerTick);
                    }

                    _ = await Navigation.PopModalAsync();
                    break;
                case 1:
                    returnOneStep();
                    break;
                case 2:
                    returnTwoStep();
                    break;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            switch (step)
            {
                case 0:
                    if (TimerStart)
                    {
                        TimerStart = false;
                        Settings.TimerStart = true;
                        Settings.TimerTime = TimerTime;
                        Device.StartTimer(TimeSpan.FromSeconds(1), Settings.OnTimerTick);
                    }

                    return base.OnBackButtonPressed();
                    break;
                case 1:
                    returnOneStep();
                    return true;
                    break;
                case 2:
                    returnTwoStep();
                    return true;
                    break;
                default:
                    return true;
            }
        }


        private async void NextTwoReg(object sender, EventArgs e)
        {
            SecondStep();
        }

        private async void SecondStep()
        {
            Analytics.TrackEvent("Второй шаг регистрации");
            var entryCodeText = EntryCode.Text;
            CheckResult result = await _server.RequestChechCode(Person.Phone, entryCodeText);
            if (result.IsCorrect)
            {
                StepsImage.Source = ImageSource.FromFile("ic_steps_three");
                RegistrationFrameStep2.IsVisible = false;
                RegistrationFrameStep3.IsVisible = true;
                tech.IsVisible = false;
                step = 2;
                LabelSteps.Text = $"{AppResources.Step} 3";
                Person.Code = entryCodeText;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorRegisterWrongCode, "OK");
            }
        }

        void setDisabledSms(bool isEnabled)
        {
            StackLayoutSms.IsEnabled = !isEnabled;
            if (isEnabled)
            {
                ToSmsWhatsApp.BorderColor = hexWhite;
                LabelWhatsApp.TextColor = hexWhite;
                ImageWhatsApp.ReplaceStringMap  = new Dictionary<string, string>
                {
                    { "#000000", hexWhite.ToHex()}
                }; 
                
                ToSms.BorderColor = hexWhite;
                LabelSms.TextColor = hexWhite;
                ImageSms.ReplaceStringMap  = new Dictionary<string, string>
                {
                    { "#000000", hexWhite.ToHex()}
                };
            }
            else
            {
                ToSmsWhatsApp.BorderColor = hex;
                LabelWhatsApp.TextColor = hex;
                ImageWhatsApp.ReplaceStringMap  = new Dictionary<string, string>
                {
                    { "#000000", hex.ToHex()}
                }; 
                
                ToSms.BorderColor = hex;
                LabelSms.TextColor = hex;
                ImageSms.ReplaceStringMap  = new Dictionary<string, string>
                {
                    { "#000000", hex.ToHex()}
                };
            }
        }
        
        private bool isSms = false;
        private void EntryCode_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!EntryCode.Text.Equals(""))
            {
                FrameBtnNextTwo.BackgroundColor = (Color) Application.Current.Resources["MainColor"];
                isNext = true;
                if (isSms)
                {
                    FrameBtnNextTwo.IsVisible = true;
                    StackLayoutSms.IsVisible = false;
                }
            }
            else
            {
                FrameBtnNextTwo.BackgroundColor = hexWhite;
                isNext = false;
                if (isSms)
                {
                    // FrameBtnNextTwo.IsVisible = false;
                    StackLayoutSms.IsVisible = false;
                }
            }
        }

        private bool isSmsOrCall = false;
        private bool OnTimerTick()
        {
            string smsOrCall = AppResources.AskForCodeAgain;
            if (isSmsOrCall)
            {
                smsOrCall = AppResources.AskForCodeSmsAgain;
            }
            
            if (TimerStart)
            {
                if (LabelTimer != null)
                {
                    LabelTimer.Text = smsOrCall.Replace("TimerTime", TimerTime.ToString());
                }

                TimerTime -= 1;
                if (TimerTime < 0)
                {
                    TimerStart = false;
                    TimerTime = 59;
                    if (FrameTimer != null)
                    {
                        FrameTimer.IsVisible = false;
                        SendCode.IsVisible = true;
                        setDisabledSms(false);
                        if (EntryCode.Text.Equals(""))
                        {
                            StackLayoutSms.IsVisible = false;
                            // FrameBtnNextTwo.IsVisible = false;
                        }
                        isSms = true;
                    }
                }
            }

            if (TimerStart == false)
            {
                if (FrameTimer != null)
                {
                    FrameTimer.IsVisible = false;
                    SendCode.IsVisible = true;
                }
            }

            return TimerStart;
        }

        private async void RegCodeRequest(object sender, EventArgs e)
        {
            await RequestCodeTask();
        }


        bool pressed;

        private async Task RequestCodeTask()
        {
            isSmsOrCall = false;
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.Loading,
            };
            if (!pressed)
                try
                {
                    pressed = true;
                    await Loading.Instance.StartAsync(async progress =>
                    {
                        CommonResult result = await _server.RequestAccessCode(Person.Phone);
                        if (result.Error == null)
                        {
                            Console.WriteLine("Отправлено");
                            TimerStart = true;
                            SendCode.IsVisible = false;
                            FrameTimer.IsVisible = true;
                            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
                            if (Device.RuntimePlatform == Device.iOS)
                            {
                                await DisplayAlert("", AppResources.AlertCodeSent, "OK");
                            }
                            else
                            {
                                DependencyService.Get<IMessage>().ShortAlert(AppResources.AlertCodeSent);
                            }

                            // FrameBtnReg.IsVisible = true;
                            // progress.IsVisible = false;

                            Loading.Instance.Hide();
                        }
                        else
                        {
                            // FrameBtnReg.IsVisible = true;
                            // progress.IsVisible = false;
                            Loading.Instance.Hide();
                            if (Device.RuntimePlatform == Device.iOS)
                            {
                                await DisplayAlert("", result.Error, "OK");
                            }
                            else
                            {
                                DependencyService.Get<IMessage>().ShortAlert(result.Error);
                            }
                        }

                        pressed = false;
                    });
                }
                catch
                {
                }
        }

        //private async void ButtonReg(object sender, EventArgs e)
        //{
        //    FinalRegg();
        //}

        private async void FinalRegg()
        {
            try {
                FrameBtnRegFinal.IsEnabled = false;

                Analytics.TrackEvent("Финальный шаг регистрации");
                string pass = EntryPassNew.Text;
                string passConfirm = EntryPassCommit.Text;
                if (pass.Equals(""))
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorRegisterFillPassword, "OK");
                }
                else if (EntryPassNew.IsPassword && passConfirm.Equals(""))
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorRegisterFillPasswordConfirm, "OK");
                }
                else if (EntryPassNew.IsPassword && !pass.Equals(passConfirm))
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorRegisterPassword, "OK");
                }
                else
                {
                    CommonResult result =
                        await _server.RegisterByPhone(Person.FIO, Person.Phone, pass, Person.Code, Person.Birthday);
                    if (result.Error == null)
                    {
                        if (Navigation.ModalStack.Count > 0)
                            await Navigation.PopModalAsync();
                        _mainPage.Login(Person.Phone, pass);
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                throw;
            }
            finally
            {
                FrameBtnRegFinal.IsEnabled = true;
            }

          
        }

        private void datePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            isDate = true;
        }

        private void DateEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DateEntry.Text.Contains(","))
            {
                Device.BeginInvokeOnMainThread(async () => { DateEntry.Text = DateEntry.Text.Replace(",", ""); });
            }
        }
    }
}