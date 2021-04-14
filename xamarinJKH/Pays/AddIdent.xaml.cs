using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AiForms.Dialogs;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddIdent : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        private PaysPage _paysPage;
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
        public AddIdent(PaysPage paysPage)
        {
            InitializeComponent();
            Convert = true;
            Analytics.TrackEvent("Добавление ЛС");
            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await Navigation.PushAsync(new AppPage()); };
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

            var scanQr = new TapGestureRecognizer();
            scanQr.Tapped += async (s, e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string scanAsync;
                    
                    scanAsync = await DependencyService.Get<IQrScanningService>().ScanAsync();
                    if (scanAsync != null)
                    {
#if DEBUG
                        Toast.Instance.Show<ToastDialog>(new { Title = scanAsync, Duration = 5500, ColorB = Color.Gray, ColorT = Color.White });
#endif
                        var qr =scanAsync.Split('|');
                        var ls = qr.FirstOrDefault(_ => _.ToLower().Contains("persacc"));
                        if (ls!=null)
                        {
                            EntryIdent.Text = ls.Replace("persAcc=", "");
                        }
                        else
                        {
                            Toast.Instance.Show<ToastDialog>(new { Title = AppResources.NothingFound, Duration = 5500, ColorB = Color.Gray, ColorT = Color.White });
                        }
                    }
                    else
                    {
                        Toast.Instance.Show<ToastDialog>(new { Title = AppResources.NothingFound, Duration = 5500, ColorB = Color.Gray, ColorT = Color.White });
                    }
                });
            };
            FrameBtnAddFromQr.GestureRecognizers.Add(scanQr);


            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            _paysPage = paysPage;
            SetText();
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            MessagingCenter.Subscribe<Object>(this, "ClosePage", async (sender) =>
            {
                _ = await Navigation.PopAsync();
            });

            SetIconColor();

            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) =>
            {
                SetIconColor();
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            GreySepLine.WidthRequest = IconViewFio.Width + LabelPhone2.Width +20;
        }

        void SetIconColor()
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
        }
        
        private async void AddButtonClick(object sender, EventArgs e)
        {
            AddIdentAccount(EntryIdent.Text);
        }
        public bool hex { get; set; }
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
           
            FrameBtnAdd.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            progress.Color = (Color)Application.Current.Resources["MainColor"];
            Labelseparator.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            IconViewFio.Foreground = (Color)Application.Current.Resources["MainColor"];
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            Frame.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);

            
        }

        async void AddIdentToList(AccountInfo ident)
        {
            string login = Preferences.Get("login", "");
            string pass = Preferences.Get("pass", "");
            LoginResult person = await _server.Login(login, pass);
            Settings.Person = person;
            MessagingCenter.Send<Object, AccountInfo>(this, "AddIdent", ident);

            MessagingCenter.Send<Object>(this, "UpdateCounters");
            MessagingCenter.Send<Object>(this, "AutoUpdate");

            MessagingCenter.Send<Object>(this, "StartRefresh");
        }
        
        public async void AddIdentAccount(string ident)
        {
            if (ident != "")
            {
                progress.IsVisible = true;
                FrameBtnAdd.IsVisible = false;
                progress.IsVisible = true;
                AddAccountResult result = await _server.AddIdent(ident, true);
                if (result.Error == null)
                {
                    Console.WriteLine(result.Address);
                    Console.WriteLine("Отправлено");
                    bool answer = await DisplayAlert("", result.Address, AppResources.AddIdent, AppResources.Cancel);
                    if (answer)
                    {
                        if (Settings.MobileSettings.useAccountPinCode)
                        {
                            await PopupNavigation.Instance.PushAsync(
                                new EnterPinAddLs(ident, _paysPage, this));
                        }
                        else
                        {
                            AcceptAddIdentAccount(ident, Settings.Person.Email);
                        }
                    }
                    else
                    {
                        FrameBtnAdd.IsVisible = true;
                        progress.IsVisible = false;
                    }
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnAdd.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnAdd.IsVisible = true;
            }
            else
            {
                if (ident == "")
                {
                    await DisplayAlert("", AppResources.ErrorFillIdent , "OK");
                }
            }
        }
        
        public async void AcceptAddIdentAccount(string ident, string email)
        {
            if (ident != "")
            {
                Device.BeginInvokeOnMainThread( ()=> { progress.IsVisible = true; FrameBtnAdd.IsVisible = false;});
                
                progress.IsVisible = true;
                AddAccountResult result = await _server.AddIdent(ident);
                if (result.Error == null)
                {
                    
                    Console.WriteLine(result.Address);
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", $"{AppResources.Acc} " + ident + $"{AppResources.AddLsString}", "ОК");
                    FrameBtnAdd.IsVisible = true;
                    Device.BeginInvokeOnMainThread(() => progress.IsVisible = false);
                    if (!string.IsNullOrWhiteSpace(result.acx))
                    {
                       
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            string login = Preferences.Get("login", "");
                            string pass = Preferences.Get("pass", "");
                            if (!pass.Equals("") && !login.Equals(""))
                            {
                                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                                {
                                    Device.BeginInvokeOnMainThread(async () =>
                                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                                    return;
                                }

                                LoginResult loginResult = await _server.Login(login, pass);
                                if (loginResult.Error == null)
                                {
                                    Settings.Person = loginResult;
                                }
                                Settings.Person.acx = result.acx;
                                try
                                {
                                    AddIdentToList(Settings.Person.Accounts.Where(x => x.Ident == ident).FirstOrDefault());
                                    _ = await Navigation.PopAsync();
                                }
                                catch { }
                            }
                        });
                    }
                    

                    if (_paysPage != null) await _paysPage.RefreshPaysData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnAdd.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnAdd.IsVisible = true;
            }
            else
            {
                if (ident == "")
                {
                    await DisplayAlert("", AppResources.ErrorFillIdent, "OK");
                }
            }
        }
    }
}