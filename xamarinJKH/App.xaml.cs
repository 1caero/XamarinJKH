using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Plugin.FirebasePushNotification;
using Xamarin.Essentials;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using Application = Xamarin.Forms.Application;
using System.Linq;
using System.Text;
using Akavache;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using xamarinJKH.Apps;
using xamarinJKH.Notifications;
using Device = Xamarin.Forms.Device;
using System.Threading.Tasks;
using Badge.Plugin;
using Syncfusion.SfPdfViewer.XForms;
using System.Resources;
using System.Reflection;
using Rg.Plugins.Popup.Services;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;

namespace xamarinJKH
{
    public partial class App : Application
    {
        public static int ScreenHeight { get; set; }
        public static int ScreenWidth { get; set; }
        
        public static double ScreenHeight2 { get; set; }
        public static double ScreenWidth2 { get; set; }
        public static string version { get; set; }
        public static string model { get; set; }
        public static string token { get; set; }
        
        public static bool isStart { get; set; } = false;
        public static bool isCons { get; set; } = false;
        public static bool isConnected { get; set; } = true;
        private RestClientMP server = new RestClientMP();
        public static bool MessageNoInternet { get; set; }


        private async Task getSettingsAsync()
        {
            try
            {
                var vers = Xamarin.Essentials.AppInfo.VersionString;
                var s = await server.MobileAppSettings(vers, "0");

                if (Settings.MobileSettings.Error == null)
                    Settings.MobileSettings = s;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);                
            }
        }

        static int badgeCount=0;

        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzU2OTY1QDMxMzgyZTMzMmUzMEtpcFNHRnBKOUppMFJ1RUxjWTlsbUt6QzFOY3JyMUlGVi9McDJSSmQxVW89");
            PdfViewerResourceManager.Manager = new ResourceManager("xamarinJKH.Resources.Syncfusion.SfPdfViewer.XForms", GetType().GetTypeInfo().Assembly);
            CrossBadge.Current.ClearBadge();
            Crashes.SendingErrorReport += SendingErrorReportHandler;
            Crashes.SentErrorReport += SentErrorReportHandler;
            Crashes.FailedToSendErrorReport += FailedToSendErrorReportHandler;
            Crashes.GetErrorAttachments += GetErrorAttachmentHandler;

            var task = Task.Run(async () => await getSettingsAsync());
            task.Wait();

            
            if (/*Device.RuntimePlatform == Device.iOS &&*/Settings.MobileSettings?.appTheme != null && Application.Current.UserAppTheme == OSAppTheme.Unspecified)
            {
               switch (Settings.MobileSettings.appTheme)
                {
                    case "": Application.Current.UserAppTheme = OSAppTheme.Light; break;
                    case "light": Application.Current.UserAppTheme = OSAppTheme.Light; break;
                    case "dark": Application.Current.UserAppTheme = OSAppTheme.Dark; break;
                }
                
            } 


            DependencyService.Register<RestClientMP>();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:

                    Color color= Color.White; 

                    int theme = Preferences.Get("Theme", 0);

                    switch (theme)
                    {
                        case 1: color = Color.White; break;
                        case 2: color = Color.Black; break;
                        case 0:
                            switch (Settings.MobileSettings.appTheme)
                            {
                                case "": color = Color.Black; break;
                                case "light": color = Color.Black; break;
                                case "dark": color = Color.White; break;
                            }
                            break;                        
                    }

                    var c2 = Settings.MobileSettings.appTheme == "light" ? Color.Black : Color.White;

                    var nav = new Xamarin.Forms.NavigationPage(new MainPage())
                    {
                        BarBackgroundColor = c2,
                        BarTextColor = color
                    };

                    nav.On<iOS>().SetIsNavigationBarTranslucent(true);
                    nav.On<iOS>().SetStatusBarTextColorMode(StatusBarTextColorMode.MatchNavigationBarTextLuminosity);

                    MainPage = nav;
                    break;
                case Device.Android:
                    MainPage = new MainPage();
                    break;
                default:
                    break;
            }

            CrossFirebasePushNotification.Current.Subscribe("general");
            CrossFirebasePushNotification.Current.OnTokenRefresh += async (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
                token = p.Token;
                await server.RegisterDevice(isCons);
            };

            
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Received");
                if(Device.RuntimePlatform==Device.iOS)
                {
                    if (DependencyService.Get<IAppState>().IsAppBackbround())
                    {
                        badgeCount++;
                        CrossBadge.Current.SetBadge(badgeCount);
                    }                    
                }
                else
                { 
                    CrossBadge.Current.ClearBadge();
                }

                    Device.BeginInvokeOnMainThread(async () =>
                {
                    if (isCons)
                    {
                        MessagingCenter.Send<Object>(this, "UpdateAppCons");
                    }
                    else
                    {
                        MessagingCenter.Send<Object>(this, "AutoUpdate");
                    }

                    try
                    {
                        bool displayAlert = false;
                        string o = string.Empty;

                        if ( o.ToLower().Equals("осс"))
                        {
                            await MainPage.Navigation.PushModalAsync(new OSSMain());
                        }

                        if (o.ToLower().Equals("comment"))
                        {
                            if (isCons)
                            {
                                await Task.Delay(500);
                                MessagingCenter.Send<Object>(this, "RefreshApp");
                            }
                            else
                            {
                                MessagingCenter.Send<Object>(this, "AutoUpdateComments");
                            }
                        }
                        
                        if (o.ToLower().Equals("comment"))
                        {
                            var tabbedpage = App.Current.MainPage.Navigation.ModalStack.ToList()[0];
                            if (tabbedpage is xamarinJKH.Main.BottomNavigationPage)
                            {
                                var stack = (tabbedpage as Xamarin.Forms.TabbedPage).Children[3].Navigation
                                    .NavigationStack;
                                if (stack.Count == 2)
                                {
                                    var app_page = stack.ToList()[0];
                                }
                                else
                                {
                                    if (!isCons)
                                    MessagingCenter.Send<Object, int>(this, "SwitchToApps",
                                        int.Parse(p.Data["id_request"].ToString()));
                                    else
                                        MessagingCenter.Send<Object, int>(this, "SwitchToAppsConst",
                                            int.Parse(p.Data["id_request"].ToString()));
                                }
                            }

                            if (tabbedpage is xamarinJKH.MainConst.BottomNavigationConstPage)
                            {
                                var stack = (tabbedpage as Xamarin.Forms.TabbedPage).Children[0].Navigation
                                    .NavigationStack;
                                if (stack.Count == 2)
                                {
                                    var app_page = stack.ToList()[0];
                                }
                                else
                                {
                                    if (!isCons)
                                        MessagingCenter.Send<Object, int>(this, "SwitchToApps",
                                            int.Parse(p.Data["id_request"].ToString()));
                                    else
                                        MessagingCenter.Send<Object, int>(this, "SwitchToAppsConst",
                                            int.Parse(p.Data["id_request"].ToString()));
                                }
                            }
                        }

                        if (o.ToLower() == "announcement")
                        {
                            var tabbedpage = App.Current.MainPage.Navigation.ModalStack.ToList()[0];
                            if (tabbedpage is xamarinJKH.Main.BottomNavigationPage)
                            {
                                var stack = (tabbedpage as Xamarin.Forms.TabbedPage).Children[0].Navigation
                                    .NavigationStack;
                                if (stack.Count == 2 && !isCons)
                                {
                                    await (tabbedpage as Xamarin.Forms.TabbedPage).Children[0].Navigation.PopToRootAsync();
                                }
                                if (!isCons)
                                {
                                    MessagingCenter.Send<Object, (string,string)>(this, "OpenNotification", (p.Data["body"].ToString(),p.Data["title"].ToString()));
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                });
            };
            CrossFirebasePushNotification.Current.OnNotificationOpened += async (s, rea) =>
            {
                Analytics.TrackEvent("открыт пуш");
                System.Diagnostics.Debug.WriteLine("Opened");
                if (rea != null)
                    if (rea.Data != null)
                        
                        if (rea.Data.ContainsKey("type_push") || rea.Data.ContainsKey("gcm.notification.type_push"))
                        {
                            string o = "";
                            if (Device.RuntimePlatform == Device.Android)
                                o = rea.Data["type_push"].ToString();
                            else
                                o = rea.Data["gcm.notification.type_push"].ToString();

                            Analytics.TrackEvent($"тип пуша {o}");

                            if (o.ToLower().Equals("announcement"))
                            {
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    string login = Preferences.Get("login", "");
                                    string pass = Preferences.Get("pass", "");
                                    if (!pass.Equals("") && !login.Equals(""))
                                    {
                                        LoginResult loginResult = await server.Login(login, pass);
                                        if (loginResult.Error == null)
                                        {
                                            Settings.EventBlockData = await server.GetEventBlockData();
                                            if (Settings.EventBlockData != null)
                                            {
                                                if (Settings.EventBlockData.Announcements != null)
                                                {
                                                    foreach (var each in Settings.EventBlockData.Announcements)
                                                    {
                                                        if (rea.Data.ContainsKey("aps.alert.title") && rea.Data.ContainsKey("aps.alert.body"))
                                                        {
                                                            if (rea.Data["aps.alert.title"].Equals(each.Header) & rea.Data["aps.alert.body"].Equals(each.Text))
                                                            {
                                                                await MainPage.Navigation.PushModalAsync(new NotificationOnePage(each));
                                                            }
                                                        }
                                                        if (rea.Data.ContainsKey("title") && rea.Data.ContainsKey("body"))
                                                        {
                                                            if (rea.Data["title"].Equals(each.Header) & rea.Data["body"].Equals(each.Text))
                                                            {
                                                                await MainPage.Navigation.PushModalAsync(new NotificationOnePage(each));
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Analytics.TrackEvent("Объявления пустые");
                                                }
                                            }
                                            else
                                            {
                                                Analytics.TrackEvent($"Сервер вернул ошибку при загрузке данных: {Settings.EventBlockData}");
                                            }
                                        
                                        }
                                        else
                                        {
                                            Analytics.TrackEvent($"сервер вернул ошибку: {loginResult.Error}");
                                        }
                                    }
                                });
                            }
                            
                            if (o.ToLower().Equals("осс"))
                            {
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    string login = Preferences.Get("login", "");
                                    string pass = Preferences.Get("pass", "");
                                    if (!pass.Equals("") && !login.Equals(""))
                                    {
                                        LoginResult loginResult = await server.Login(login, pass);
                                        if (loginResult.Error == null)
                                        {
                                            Settings.Person = loginResult;
                                            await MainPage.Navigation.PushModalAsync(new OSSMain());
                                        }
                                    }
                                });
                            }
                            
                            if (o.ToLower().Equals("comment"))
                            {
                                if (rea.Data.ContainsKey("id_request"))
                                {
                                    var id = int.Parse(rea.Data["id_request"].ToString());
                                    Analytics.TrackEvent("ключ id_request=" + id);
                                    Analytics.TrackEvent("isCons=" + isCons);
                            
                                    string login = Preferences.Get("login", "");
                                    string pass = Preferences.Get("pass", "");
                                    string loginConst = Preferences.Get("loginConst", "");
                                    string passConst = Preferences.Get("passConst", "");
                                
                                    LoginResult loginResult = isCons ? await server.LoginDispatcher(loginConst, passConst) : await server.Login(login, pass);
                                    if (loginResult.Error == null)
                                    {
                                        if (isCons)
                                        {                                            
                                            RequestList requestList = await server.GetRequestsListConst();
                                            Analytics.TrackEvent("получен requestList");
                                            if(requestList?.Requests != null)
                                            {
                                                Analytics.TrackEvent("список Requests не null");
                                                var request = requestList.Requests.FirstOrDefault(x => x.ID == id);
                                                if (request != null)
                                                    await MainPage.Navigation.PushModalAsync(new AppConstPage(request));
                                                else
                                                {
                                                    Analytics.TrackEvent("в списке Requests нет id="+ id);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            RequestList requestsList = await server.GetRequestsList();
                                            Analytics.TrackEvent("получен requestsList");
                                            if (requestsList?.Requests != null)
                                            {
                                                Analytics.TrackEvent("список Requests не null");
                                                var request = requestsList.Requests.FirstOrDefault(x => x.ID == id);
                                                if (request != null)
                                                    await MainPage.Navigation.PushModalAsync(new AppPage(request, false,
                                                        request.IsPaid));
                                                else
                                                {
                                                    Analytics.TrackEvent("в списке Requests нет id=" + id);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Analytics.TrackEvent("ключ id_request не найден");
                                }
                            
                            }
                            
                            if (o.ToLower().Equals("support"))
                            {
                                
                                string login = Preferences.Get("login", "");
                                string pass = Preferences.Get("pass", "");
                                string loginConst = Preferences.Get("loginConst", "");
                                string passConst = Preferences.Get("passConst", "");
                                
                                LoginResult loginResult = isCons ? await server.LoginDispatcher(loginConst, passConst) : await server.Login(login, pass);
                                string phone = Preferences.Get("techPhone", loginResult.Phone);
                                if (!string.IsNullOrWhiteSpace(phone))
                                {
                                    Settings.SetPhoneTech(phone);
                                    await server.RegisterDeviceNotAvtorization(Settings.Person?.Phone);
                                    if (MainPage.Navigation.NavigationStack?.FirstOrDefault(x => x is Tech.AppPage) == null)
                                        await MainPage.Navigation.PushModalAsync(new Tech.AppPage());
                                }
                                else
                                {
                                    
                                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
                                }                                
                            }
                        }
            };
            CrossFirebasePushNotification.Current.OnNotificationAction += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Action");

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    System.Diagnostics.Debug.WriteLine($"ActionId: {p.Identifier}");
                    foreach (var data in p.Data)
                    {
                        System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                    }
                }
            };
        }

      

        async void pExec(FirebasePushNotificationResponseEventArgs p)
        {
            if (p.Data.ContainsKey("id_request"))
            {
                var id = int.Parse(p.Data["id_request"].ToString());
                Analytics.TrackEvent("ключ id_request=" + id);
                Analytics.TrackEvent("isCons=" + isCons);
                if (!isCons)
                    MessagingCenter.Send<Object, int>(this, "SwitchToApps", id);
                else
                    MessagingCenter.Send<Object, int>(this, "SwitchToAppsConst", id);
            }
            else
            {
                Analytics.TrackEvent("ключ id_request не найден");
            }
        }

        private IEnumerable<ErrorAttachmentLog> GetErrorAttachmentHandler(ErrorReport report)
        {
            string accountsJson = Newtonsoft.Json.JsonConvert.SerializeObject(Settings.Person.Accounts);
            // Your code goes here.
            return new ErrorAttachmentLog[]
            {
                ErrorAttachmentLog.AttachmentWithBinary(Encoding.UTF8.GetBytes(accountsJson), "Accounts.json", "application/json")
            };
        }

        private void FailedToSendErrorReportHandler(object sender, FailedToSendErrorReportEventArgs e)
        {
            AppCenterLog.Info(LogTag, "Failed to send error report");

            var args = e as FailedToSendErrorReportEventArgs;
            ErrorReport report = args.Report;
            string AccountsJson = Newtonsoft.Json.JsonConvert.SerializeObject(Settings.Person.Accounts);
            //test some values
            if (report.Exception != null)
            {
                AppCenterLog.Info(LogTag, report.Exception.ToString());
            }
            else if (report.AndroidDetails != null)
            {
                AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName );
            }

            if (e.Exception != null)
            {
                AppCenterLog.Info(LogTag, "There is an exception associated with the failure");
            }
        }

        private void SentErrorReportHandler(object sender, SentErrorReportEventArgs e)
        {
            AppCenterLog.Info(LogTag, "Sent error report");

            var args = e as SentErrorReportEventArgs;
            ErrorReport report = args.Report;
            string AccountsJson = Newtonsoft.Json.JsonConvert.SerializeObject(Settings.Person.Accounts);

            //test some values
            if (report.Exception != null)
            {
                AppCenterLog.Info(LogTag, report.Exception.ToString());
            }
            else
            {
                AppCenterLog.Info(LogTag, "No system exception was found" );
            }

            if (report.AndroidDetails != null)
            {
                AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName );
            }
        }

        private void SendingErrorReportHandler(object sender, SendingErrorReportEventArgs e)
        {
            AppCenterLog.Info(LogTag, "Sending error report");

            var args = e as SendingErrorReportEventArgs;
            ErrorReport report = args.Report;

            //test some values
            if (report.Exception != null)
            {
                AppCenterLog.Info(LogTag, report.Exception.ToString());
            }
            else if (report.AndroidDetails != null)
            {
                AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName);
            }
        }

        public string LogTag { get; set; } = "CrashLog";

        protected override void OnStart()
        {
            Registrations.Start("XamarinJKH");
                                    
            int theme = Preferences.Get("Theme", 0);

            switch (theme)
                {
                    case 0:
                    switch (Settings.MobileSettings.appTheme)
                    {
                        case "": Current.UserAppTheme = OSAppTheme.Unspecified;  break;
                        case "light": Current.UserAppTheme = OSAppTheme.Light; break;
                        case "dark": Current.UserAppTheme = OSAppTheme.Dark; break;
                    }
                    break;
                    
                        break;
                    case 1:
                        Current.UserAppTheme = OSAppTheme.Dark;
                        break;
                    case 2:
                        Current.UserAppTheme = OSAppTheme.Light;
                        break;
                }



            AppCenter.Start("android=4384b8c4-8639-411c-b011-9d9e8408acde;ios=4a45a15f-a591-4860-b748-a856636cf982;", typeof(Analytics), typeof(Crashes));
            //AppCenter.Start("39dce15b-0d85-46a2-bf90-a28af9fb8795", typeof(Analytics), typeof(Crashes)); //тихая гавань иос
            AppCenter.LogLevel = LogLevel.Verbose;
            
        
            
            // Handle when your app starts
            CrossFirebasePushNotification.Current.Subscribe("general");
            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");
            };
            System.Diagnostics.Debug.WriteLine($"TOKEN: {CrossFirebasePushNotification.Current.Token}");

            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Received");
                    if (p.Data.ContainsKey("body"))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            System.Diagnostics.Debug.WriteLine($"{p.Data["body"]}");
                        });
                    }

                    if (p.Data.ContainsKey("aps.alert.body"))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            System.Diagnostics.Debug.WriteLine($"{p.Data["aps.alert.body"]}");
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            };

            CrossFirebasePushNotification.Current.OnNotificationOpened += (s, rea) =>
            {

                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in rea.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

                if (!string.IsNullOrEmpty(rea.Identifier))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // MainPage.Message = p.Identifier;
                        System.Diagnostics.Debug.WriteLine("123");
                    });
                }
                else if (rea.Data.ContainsKey("color"))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MainPage.Navigation.PushAsync(new ContentPage()
                        {
                            BackgroundColor = Color.FromHex($"{rea.Data["color"]}")
                        });
                    });
                }
                else if (rea.Data.ContainsKey("aps.alert.title"))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        // MainPage.Message = $"{p.Data["aps.alert.title"]}";
                        System.Diagnostics.Debug.WriteLine($"Пушшшш2 ==== {rea.Data["aps.alert.title"]}");
                    });
                }
            };

            CrossFirebasePushNotification.Current.OnNotificationAction += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Action");

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    System.Diagnostics.Debug.WriteLine($"ActionId: {p.Identifier}");
                    foreach (var data in p.Data)
                    {
                        System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                    }
                }
            };

            CrossFirebasePushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            };
        }

        protected override void OnSleep()
        {
           
        }

        protected override void OnResume()
        {            
            if (Device.RuntimePlatform == Device.iOS)
            {
                CrossBadge.Current.ClearBadge();
                badgeCount = 0;                 
            }
        }
        
        
    }
}