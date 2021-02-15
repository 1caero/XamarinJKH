using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Messaging;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.AppsConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppConstPage : ContentPage
    {
        private RequestInfo _requestInfo;
        private RequestContent request;

        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        string TAKE_PHOTO = AppResources.AttachmentTakePhoto;
        string TAKE_GALRY = AppResources.AttachmentChoosePhoto;
        string TAKE_GALARY_Video = AppResources.AttachmentChooseVideo;
        string TAKE_FILE = AppResources.AttachmentChooseFile;
        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";
        bool isPaid;
        private string updatekey = Settings.UpdateKey;
        public bool IsRequestPaid
        {
            get => isPaid;
            set
            {
                isPaid = value;
                OnPropertyChanged(nameof(IsRequestPaid));
            }
        }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    await RefreshData();

                    IsRefreshing = false;
                });
            }
        }

        CancellationTokenSource TokenSource { get; set; }
        CancellationToken Token { get; set; }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(TimeSpan.FromSeconds(1));
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            RequestsUpdate requestsUpdate =
                await _server.GetRequestsUpdatesConst(Settings.UpdateKey, _requestInfo.ID.ToString());
            if (requestsUpdate.Error == null)
            {
                Settings.UpdateKey = requestsUpdate.NewUpdateKey;
            }

            var UpdateTask = new Task(async () =>
            {
                try
                {
                   
                        await Task.Delay(TimeSpan.FromSeconds(2));
#if DEBUG
                    Console.WriteLine($"UpdateTask autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
                    if (autoUpdateRun)
                        return;

#if DEBUG
                    Console.WriteLine($"автообновление выполняется UpdateTask {DateTime.Now}");
#endif

                    //Device.BeginInvokeOnMainThread(() => autoUpdateRun = true);
                    autoUpdateRun = true;
#if DEBUG
                    Console.WriteLine($"UpdateTask autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
                    var update =
                            await _server.GetRequestsUpdatesConst(Settings.UpdateKey, _requestInfo.ID.ToString());
                        if (update.Error == null)
                        {
                            Settings.UpdateKey = update.NewUpdateKey;
                            if (update.CurrentRequestUpdates != null)
                            {

                                request = update.CurrentRequestUpdates;
                                if (request.Messages != null)
                                if (request.Messages.Count() > 0)
                                foreach (var each in update.CurrentRequestUpdates.Messages)
                                {
                                    if (!messages.Contains(each) && !messages.Any(_ => _.ID == each.ID))
                                        {
                                            messages.Add(each);
                                            Device.BeginInvokeOnMainThread(() =>
                                            {
                                                //messages.Add(each);
                                                addAppMessage(each,
                                                    messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                                            });
                                        }

                                    }
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    var lastChild = baseForApp.Children.LastOrDefault();
                                    if (FrameMessage.Height < baseForApp.Height)
                                    {
                                        if (lastChild != null)
                                        {
                                            if (baseForApp.Height < lastChild.Y)
                                                await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false);
                                            else
                                                await scrollFroAppMessages.ScrollToAsync(lastChild.X, lastChild.Y + 30, false);
                                        }
                                    }
                                });

                            }
                        }
                    //Device.BeginInvokeOnMainThread(() => autoUpdateRun = false);
                    autoUpdateRun = false;
#if DEBUG
                    Console.WriteLine($"UpdateTask autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
                }
                catch (Exception e)
                {
                    //Device.BeginInvokeOnMainThread(() => autoUpdateRun = false);
                    autoUpdateRun = false;
#if DEBUG
                    Console.WriteLine($"UpdateTask exc autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
                }
            });
            try
            {
                UpdateTask.Start();
            }
            catch
            {
            }
        }

        static bool autoUpdateRun = false;

        async void SetReadedApp()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!_requestInfo.IsReaded)
                {
                    CommonResult commonResult = await _server.SetReadedFlag(_requestInfo.ID);
                    if (commonResult.Error == null)
                    {
                        _requestInfo.IsReaded = true;
                    }
                }
            });
        }

        protected override void OnDisappearing()
        {
            try
            {
                MessagingCenter.Unsubscribe<Object, KeyValuePair<int, string>>(this, "performApp");

                    TokenSource.Cancel();
                TokenSource.Dispose();
            }
            catch
            {
            }

            base.OnDisappearing();
        }

        private async Task RefreshData([CallerMemberName] string cmn="")
        {
            try
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                    return;
                }

#if DEBUG
                Console.WriteLine($"RefreshData cmn={cmn} autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif

                if (autoUpdateRun)
                {
                    return;



                }

#if DEBUG
                Console.WriteLine($"автообновление выполняется RefreshData {DateTime.Now}");
#endif
                //Device.BeginInvokeOnMainThread(() => autoUpdateRun = true);
                autoUpdateRun = true;
                Console.WriteLine($"RefreshData autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");

                RequestsUpdate requestsUpdate =
                    await _server.GetRequestsUpdatesConst(updatekey, _requestInfo.ID.ToString());
                if (requestsUpdate.Error == null)
                {
                    updatekey = requestsUpdate.NewUpdateKey;
                    if (requestsUpdate.CurrentRequestUpdates != null)
                    {
                        request = requestsUpdate.CurrentRequestUpdates;

                        if (request.Messages != null)
                            if (request.Messages.Count() > 0)
                                foreach (var each in requestsUpdate.CurrentRequestUpdates.Messages)
                                {
                                    if (!messages.Contains(each) && !messages.Any(_=>_.ID==each.ID))
                                    {
                                        messages.Add(each);
                                        Device.BeginInvokeOnMainThread( () =>
                                        {
                                            //messages.Add(each);
                                            addAppMessage(each, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                                        });
                                    }

                                }

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (DeviceInfo.Platform == DevicePlatform.iOS)
                            {
                                await Task.Delay(500);
                            }
                            var lastChild = baseForApp.Children.LastOrDefault();
                            if (FrameMessage.Height < baseForApp.Height)
                            {
                                if (lastChild != null)
                                {
                                    if (baseForApp.Height < lastChild.Y)
                                        await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false);
                                    else
                                        await scrollFroAppMessages.ScrollToAsync(lastChild.X, lastChild.Y + 30, false);
                                }
                            }
                        });
                    }
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK");
                }

                //Device.BeginInvokeOnMainThread(() => autoUpdateRun = false);
                autoUpdateRun = false;
#if DEBUG
                Console.WriteLine($"RefreshData autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
            }
            catch (Exception ex)
            {
                //Device.BeginInvokeOnMainThread(() => autoUpdateRun = false);
                autoUpdateRun = false;

#if DEBUG
                Console.WriteLine($"RefreshData exc autoUpdateRun={autoUpdateRun} {DateTime.Now.ToString("F.fffff")}");
#endif
                throw;
            }
           

        }

        public string DateUniq = "";
        public bool CanComplete => Settings.Person.UserSettings.RightPerformRequest;
        public bool CanClose => Settings.Person.UserSettings.RightCloseRequest;


        void addAppMessage(RequestMessage message, string prevAuthor)
        {
            StackLayout data;
            string newDate;
            if (message.IsSelf)
            {
                data = new MessageCellAuthor(message, this, DateUniq, out newDate, _requestInfo.ID.ToString());
            }
            else
            {
                data = new MessageCellService(message, this, DateUniq, out newDate, prevAuthor);
            }

            DateUniq = newDate;
            baseForApp.Children.Add(data);
            //messages.Add(message);
        }


        public List<RequestMessage> messages { get; set; }
        public Color hex { get; set; }

        public bool close = false;
        public bool isNotRead { get; set; }
        public ObservableCollection<OptionModel> Options { get; set; }

        public AppConstPage(RequestInfo requestInfo, bool isNotRead = true, bool closeAll = false)
        {
            close = closeAll;
            _requestInfo = requestInfo;
            this.isNotRead = isNotRead;
            Options = new ObservableCollection<OptionModel>();
            InitializeComponent();
            Analytics.TrackEvent("Заявка сотрудника №" + requestInfo.RequestNumber);
            IsRequestPaid = requestInfo.IsPaid;
            var theme = Application.Current.RequestedTheme;
            var color = theme == OSAppTheme.Dark ? "#FFFFFF" : "#000000";
            Dictionary<string, string> replace = new Dictionary<string, string> { { "#000000", color } };
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                        mainStack.Padding = new Thickness(0, statusBarHeight * 2, 0, 0);
                    else
                        mainStack.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                case Device.Android:
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                    }

                    break;
                default:
                    break;
            }

            SetReadedApp();
            Options.Add(new OptionModel
            {
                Name = AppResources.InfoApp, Image = "resource://xamarinJKH.Resources.ic_info_app1.svg", Command = new Command(() => ShowInfo()),
                IsVisible = true,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.AcceptApp, Image = "resource://xamarinJKH.Resources.ic_accept_app.svg", Command = new Command(() => acceptApp()),
                IsVisible = true,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.Rejection_App, Image = "resource://xamarinJKH.Resources.crossed.svg", Command = new Command(async () =>
                {
                    var result2 = await DisplayAlert("", AppResources.UserRejectionApp, AppResources.Yes,
                        AppResources.No);
                    if (result2)
                    {
                        CommonResult result = await _server.CloseAppConst(_requestInfo.ID.ToString());
                        if (result.Error == null)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await ShowToast(AppResources.CloseAppUserDone);
                                await ClosePage();
                                await RefreshData();
                            });
                            
                        }
                        else
                        {
                            await ShowToast(result.Error);
                        }
                    }
                }),
                IsVisible = true,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.CompleteApp, Image = "resource://xamarinJKH.Resources.ic_check_mark.svg", Command = new Command(() => ComplicationRun()  /*performApp()*/),
                IsVisible = CanComplete,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.PassApp,
                Image = "resource://xamarinJKH.Resources.ic_next_disp.svg",
                Command = new Command(async () =>
                {
                    //Device.BeginInvokeOnMainThread(async () =>
                    //{
                        await PopupNavigation.Instance.PushAsync(new MoveDispatcherView(hex, _requestInfo, true));
                        await RefreshData();
                    //});
                }),
                IsVisible = true,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.Transit,
                Image = "resource://xamarinJKH.Resources.ic_in_way.svg",
                Command = new Command(async () =>
                {
                    progress.IsVisible = true;
                    var request = await _server.SetPaidRequestStatusOnTheWay(_requestInfo.ID.ToString());
                    if (request.Error == null)
                    {
                        await ShowToast(AppResources.TransitOrder);
                        await RefreshData();
                    }
                    else
                    {
                        await DisplayAlert(AppResources.Order, request.Error, "OK");
                    }

                    progress.IsVisible = false;
                }),
                IsVisible = IsRequestPaid,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.SendCodeApp, Image = "resource://xamarinJKH.Resources.ic_send_code.svg",
                Command = new Command(async () =>
                    await Dialog.Instance.ShowAsync(
                        new EnterCodeDialogView(this._requestInfo.ID.ToString()))),
                IsVisible = IsRequestPaid,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.Receipt,
                Image = "resource://xamarinJKH.Resources.ic_receipt.svg",
                Command = new Command(async () =>
                {
                    List<RequestsReceiptItem> Items = new List<RequestsReceiptItem>();
                    if (request.ReceiptItems != null)
                        if (request.ReceiptItems.Count() > 0)
                    foreach (var item in request.ReceiptItems)
                    {
                        Items.Add(item.Copy());
                    }

                    await Dialog.Instance.ShowAsync(new AppConstDialogWindow(Items, request.ID, request.ShopId));
                }),
                IsVisible = IsRequestPaid,
                ReplaceMap = replace
            });
            Options.Add(new OptionModel
            {
                Name = AppResources.CloseApp,
                Image = "resource://xamarinJKH.Resources.ic_close_app1.svg",
                Command = new Command(async () =>
                {

                    CommonResult result = await _server.CloseAppConst(_requestInfo.ID.ToString());
                    if (result.Error == null)
                    {
                        var result2 = await DisplayAlert("", AppResources.RatingBarClose, "OK", AppResources.Cancel);
                        if (result2)
                        {
                            await ClosePage();
                            await ShowToast(AppResources.AppClosed);
                            await RefreshData();
                        }
                    }
                    else
                    {
                        await ShowToast(result.Error);
                    }
                }),
                IsVisible = CanClose,
                ReplaceMap = replace
            });

            MessagingCenter.Subscribe<Object>(this, "ClosePage", async (sender) =>
            {
                try
                {
                    await Navigation.PopAsync();
                }
                catch
                {

                }
            });
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { await ClosePage(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var sendMess = new TapGestureRecognizer();
            sendMess.Tapped += (s, e) => {
                //sendMessage(); 
                Device.BeginInvokeOnMainThread(() => sendMessage());
            };
            IconViewSend.GestureRecognizers.Add(sendMess);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { addFileApp(); };
            IconViewAddFile.GestureRecognizers.Add(addFile);
            var showInfo = new TapGestureRecognizer();
            showInfo.Tapped += async (s, e) => { ShowInfo(); };
            StackLayoutInfo.GestureRecognizers.Add(showInfo);
            var acceptAp = new TapGestureRecognizer();
            acceptAp.Tapped += async (s, e) => { acceptApp(); };
            StackLayoutAccept.GestureRecognizers.Add(acceptAp);
            var performAp = new TapGestureRecognizer();
            performAp.Tapped += (s, e) =>
            {
                ComplicationRun();

                //performApp();
            };

            MessagingCenter.Subscribe<Object, KeyValuePair<int, string>>(this, "performApp", (sender, kvp) => {
                Device.BeginInvokeOnMainThread(() => {
                    EntryMess.Text = kvp.Value;
                    performApp();
                });
                //EntryMess.Text = kvp.Value;
                //performApp(); 
            });


            StackLayoutExecute.GestureRecognizers.Add(performAp);
            var moveDisp = new TapGestureRecognizer();
            moveDisp.Tapped += async (s, e) =>
            {
                await PopupNavigation.Instance.PushAsync(new MoveDispatcherView(hex, _requestInfo, true));
                await RefreshData();
            };
            StackLayoutMoveDisp.GestureRecognizers.Add(moveDisp);
            var closeApp = new TapGestureRecognizer();
            closeApp.Tapped += async (s, e) =>
            {

                CommonResult result = await _server.CloseAppConst(_requestInfo.ID.ToString());
                if (result.Error == null)
                {
                    var result2 = await DisplayAlert("", AppResources.RatingBarClose, "OK", AppResources.Cancel);
                    if (result2)
                    {
                        await ClosePage();
                        await ShowToast(AppResources.AppClosed);
                        await RefreshData();
                    }
                }
                else
                {
                    await ShowToast(result.Error);
                }
            };
            StackLayoutClose.GestureRecognizers.Add(closeApp);
            hex = (Color) Application.Current.Resources["MainColor"];

            setText();

            messages = new List<RequestMessage>();

            getMessage2();
            MessagingCenter.Subscribe<Object>(this, "RefreshApp", async (sender) => await RefreshData());
            MessagingCenter.Subscribe<Object>(this, "RefreshAppList", async (sender) =>
                request = await _server.GetRequestsDetailListConst(_requestInfo.ID.ToString()));
            MessagingCenter.Subscribe<Object, string>(this, "OpenFileConst", async (sender, args) =>
            {
                string[] arg = args.Split(",");
                getFile(arg[0], arg[1]);
            });
            MessagingCenter.Subscribe<Object>(this, "CloseAPP", callback: async (sender) =>
            {
                MessagingCenter.Send<Object>(this, "UpdateAppCons");
                if (close)
                {
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    Settings.isSelf = null;
                    Settings.DateUniq = "";
                    try
                    {
                        _ = await Navigation.PopAsync();
                    }
                    catch
                    {
                    }
                }
            });

            if (!requestInfo.IsReaded)
            {
                Task.Run(async () =>
                {
                    var res = await _server.SetReadedFlag(requestInfo.ID);
                    MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", -1);
                    MessagingCenter.Send<Object, int>(this, "SetAppReadConst", requestInfo.ID);
                    requestInfo.IsReaded = true;
                });
            }

            hiddenComent.IsVisible = !Settings.Person.UserSettings.AlwaysPostHiddenMessage;
            if (Settings.Person.UserSettings.AlwaysPostHiddenMessage)
            {
                IconViewSend.Column(3);
            }
        }

        private void ComplicationRun()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                Configurations.LoadingConfig = new LoadingConfig
                {
                    IndicatorColor = Color.Transparent,
                    OverlayColor = Color.Black,
                    Opacity = 0.8,
                    DefaultMessage = "",
                };
                await Loading.Instance.StartAsync(async progress =>
                {
                    var ret = await Dialog.Instance.ShowAsync<AppCompliteDialog>(new
                    {
                        HexColor = hex,
                        Id = _requestInfo.ID
                    });
                });
            }
                                   );
        }

        async void getFile(string id, string fileName)
        {
            var UpdateTask = new Task(async () => { await GetFile(id, fileName); });
            UpdateTask.Start();
        }

        static bool fileGettingNow = false;
        public async Task GetFile(string id, string fileName)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (fileGettingNow)
                        return;
                    fileGettingNow = true;
                    // Loading settings
                    Configurations.LoadingConfig = new LoadingConfig
                    {
                        IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                        OverlayColor = Color.Black,
                        Opacity = 0.8,
                        DefaultMessage = AppResources.Loading,
                    };

                    await Loading.Instance.StartAsync(async progress =>
                    {
                        byte[] memoryStream = await _server.GetFileAPPConst(id);
                        if (memoryStream != null)
                        {
                            await DependencyService.Get<IFileWorker>().SaveTextAsync(fileName, memoryStream);
                            await Launcher.OpenAsync(new OpenFileRequest
                            {
                                File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(fileName))
                            });
                        }
                        else
                        {
                            await DisplayAlert("Ошибка", "Не удалось скачать файл", "OK");
                        }
                    });
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    fileGettingNow = false;
                }
            });            
        }

        private async Task ClosePage()
        {
            MessagingCenter.Unsubscribe<Object, KeyValuePair<int, string>>(this, "performApp");
            // MessagingCenter.Send<Object>(this, "UpdateAppCons");
            if (close)
            {
                await Navigation.PopToRootAsync();
            }
            else
            {
                Settings.isSelf = null;
                Settings.DateUniq = "";
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch
                {
                    _ = await Navigation.PopModalAsync();
                }
            }
        }

        protected async void SendCode(object sender, EventArgs args)
        {
            await Dialog.Instance.ShowAsync(new EnterCodeDialogView(this._requestInfo.ID.ToString()));
        }

        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
               PopupNavigation.Instance.PopAllAsync();
               return true;
            }
            else
            {
                if (close)
                {
                    Navigation.PopToRootAsync();
                    return true;
                }
                else
                {
                    Settings.isSelf = null;
                    Settings.DateUniq = "";
                    return base.OnBackButtonPressed();
                }
            }
        }


        async void addFileApp()
        {
            if (Device.RuntimePlatform == "Android")
            {
                try
                {
                    var camera_perm =
                        await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                    var storage_perm =
                        await CrossPermissions.Current
                            .CheckPermissionStatusAsync(Permission.Storage);
                    if (camera_perm != PermissionStatus.Granted || storage_perm != PermissionStatus.Granted)
                    {
                        var status =
                            await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera,
                                Permission.Storage);
                        if (status[Permission.Camera] == PermissionStatus.Denied &&
                            status[Permission.Storage] == PermissionStatus.Denied)
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var result = await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoPermissions, "OK",
                            AppResources.Cancel);
                        if (result)
                            CrossPermissions.Current.OpenAppSettings();
                    });
                    return;
                }
            }

            MediaFile file = null;
            //var action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
            //    TAKE_PHOTO,
            //    TAKE_GALRY, TAKE_FILE);

            string action;
            if (Device.RuntimePlatform == Device.Android)
                action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
               TAKE_PHOTO,
               TAKE_GALRY, TAKE_FILE);
            else
                action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
                   TAKE_PHOTO,
                   TAKE_GALRY, TAKE_GALARY_Video, TAKE_FILE);

            if (action == TAKE_PHOTO)
            {
                if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCameraNotAvailable, "OK");

                    return;
                }

                try
                {
                    file = await CrossMedia.Current.TakePhotoAsync(
                        new StoreCameraMediaOptions
                        {
                            SaveToAlbum = false,
                            Directory = "Demo",
                            PhotoSize = PhotoSize.Medium,
                        });

                    if (file != null)
                        await startLoadFile(CAMERA, file);
                }
                catch (Exception e)
                {
                    await DisplayAlert(AppResources.ErrorTitle, $"{e.Message}\n{e.StackTrace}", "OK");
                    Console.WriteLine(e);
                }

                return;
            }

            if (action == TAKE_GALRY)
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorGalleryNotAvailable, "OK");

                    return;
                }

                try
                {
                    file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions {PhotoSize = PhotoSize.Medium});
                    if (file == null)
                        return;
                    await startLoadFile(GALERY, file);
                }
                catch (Exception e)
                {
                    await DisplayAlert(AppResources.ErrorTitle, $"{e.Message}\n{e.StackTrace}", "OK");
                    Console.WriteLine(e);
                }

                return;
            }

            if (action == TAKE_GALARY_Video)
            {
                if (!CrossMedia.Current.IsPickVideoSupported)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorGalleryNotAvailable, "OK");

                    return;
                }

                try
                {
                    file = await CrossMedia.Current.PickVideoAsync();
                    if (file == null)
                        return;
                    await startLoadFile(GALERY, file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return;
            }

            if (action == TAKE_FILE)
            {
                await startLoadFile(FILE, null);
            }
        }

        async Task getCameraFile(MediaFile file)
        {
            if (file == null)
                return;
            CommonResult commonResult = await _server.AddFileAppsConst(_requestInfo.ID.ToString(),
                getFileName(file.Path), StreamToByteArray(file.GetStream()),
                file.Path);
            if (commonResult == null)
            {
                await ShowToast(AppResources.SuccessFileSent);
                await RefreshData();
            }
        }

        MediaFile photo { get; set; }

        public async Task startLoadFile(string metod, MediaFile file)
        {
            try
            {
                switch (metod)
                {
                    case CAMERA:
                        await getCameraFile(file);
                        break;
                    case GALERY:
                        await GetGalaryFile(file);
                        break;
                    case FILE:
                        await PickAndShowFile(null);
                        break;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(null, $"{ex.Message}\n{ex.StackTrace}", "OK");
                Console.WriteLine(ex.Message);
            }
        }

        async Task GetGalaryFile(MediaFile file)
        {
            CommonResult commonResult = await _server.AddFileAppsConst(_requestInfo.ID.ToString(),
                getFileName(file.Path), StreamToByteArray(file.GetStream()),
                file.Path);
            if (commonResult == null)
            {
                await ShowToast(AppResources.SuccessFileSent);
                await RefreshData();
            }
        }

        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream) stream).ToArray();
            }
            else
            {
                // Jon Skeet's accepted answer 
                return ReadFully(stream);
            }
        }

        string getFileName(string path)
        {
            try
            {
                string[] fileName = path.Split('/');
                return fileName[fileName.Length - 1];
            }
            catch (Exception ex)
            {
                return "filename";
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private async Task PickAndShowFile(string[] fileTypes)
        {
            try
            {
                IconViewAddFile.IsVisible = false;
                progressFile.IsVisible = true;
                FileData pickedFile = await CrossFilePicker.Current.PickFile(fileTypes);

                if (pickedFile != null)
                {
                    if (pickedFile.DataArray.Length > 10000000)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.FileTooBig, "OK");
                        IconViewAddFile.IsVisible = true;
                        progressFile.IsVisible = false;
                        return;
                    }


                    CommonResult commonResult = await _server.AddFileAppsConst(_requestInfo.ID.ToString(),
                        pickedFile.FileName, pickedFile.DataArray,
                        pickedFile.FilePath);
                    if (commonResult == null)
                    {
                        await ShowToast(AppResources.SuccessFileSent);
                        await RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, ex.ToString(), "OK");
            }

            IconViewAddFile.IsVisible = true;
            progressFile.IsVisible = false;
        }

        DateTime checkBot = DateTime.Now.AddMinutes(-1);
        async void sendMessage()
        {
            //чаще 1 раза в секунду отправлять не давать. спам. 
            if (checkBot > DateTime.Now.AddSeconds(-1))
                return;
            checkBot = DateTime.Now;

            string message = EntryMess.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                progress.IsVisible = true;
                IconViewSend.IsVisible = false;
                await Task.Delay(300);

                bool hidden = CheckBoxHidden.IsChecked;
                if (Settings.Person.UserSettings.AlwaysPostHiddenMessage)
                {
                    hidden = true;
                }
                
                CommonResult result =
                    await _server.AddMessageConst(message, _requestInfo.ID.ToString(), hidden);
                if (result.Error == null)
                {
                    EntryMess.Text = "";
                    await RefreshData();
                }
            }
            else
            {
                await ShowToast(AppResources.ErrorMessageEmpty);
            }

            progress.IsVisible = false;
            IconViewSend.IsVisible = true;
        }

        private async Task ShowToast(string text)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                await DisplayAlert("", text, "OK");
            }
            else
            {
                DependencyService.Get<IMessage>().ShortAlert(text);
            }
        }


        async void getMessage2()
        {
            request = await _server.GetRequestsDetailListConst(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                Settings.DateUniq = "";

                if (request.Messages != null)
                    if (request.Messages.Count() > 0)
                        foreach (var message in request.Messages)
                {
                    messages.Add(message);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        addAppMessage(message, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                    });
                }

                LabelNumber.Text = "№ " + request.RequestNumber;
                IsRequestPaid = request.IsPaid;
                this.BindingContext = this;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK");
            }

            await MethodWithDelayAsync(1000);
        }

        async void acceptApp()
        {
            progress.IsVisible = true;
            var request = await _server.LockAppConst(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                await ShowToast(AppResources.AppAccepted);
//                if (!autoUpdateRun)
//                {
//#if DEBUG
//                    Console.WriteLine($"автообновление выполняется acceptApp {DateTime.Now}");
//#endif
//                    Device.BeginInvokeOnMainThread(()=> autoUpdateRun = true);
//                    await RefreshData();
//                    Device.BeginInvokeOnMainThread(() => autoUpdateRun = false);
//                }
                await RefreshData();

            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, request.Error, "OK");
            }

            progress.IsVisible = false;
        }

        async void performApp()
        {
            progress.IsVisible = true;
            var request = await _server.PerformAppConst(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                sendMessage();
                await ClosePage();
                await ShowToast(AppResources.AppCompleted);
                await RefreshData();
            }
            else
            {
                 EntryMess.Text = "";
                await DisplayAlert(AppResources.ErrorTitle, request.Error, "OK");
            }

            progress.IsVisible = false;
        }

        public async Task MethodWithDelayAsync(int milliseconds)
        {
            try
            {
                await Task.Delay(milliseconds);

                var lastChild = baseForApp.Children.LastOrDefault();
                if (lastChild != null)
                    await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void setText()
        {
            try
            {
                LabelNumber.Text = "№ " + _requestInfo.RequestNumber;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameKeys.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#B5B5B5"));
            FrameMessage.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        private async void ShowInfo()
        {
            if (request != null)
            {
                string Status = request.Status;
                string Source = Settings.GetStatusIcon(request.StatusID);
                if (!string.IsNullOrEmpty(Source))
                {
                    if (!string.IsNullOrWhiteSpace(request.Phone) && (request.Phone.Contains("+") == false &&
                                                                      request.Phone.Substring(0, 2) == "79"))
                    {
                        request.Phone = "+" + request.Phone;
                    }

                    Call = new Command<string>(async (phone) =>
                    {
                        if (!string.IsNullOrWhiteSpace(phone))
                        {
                            IPhoneCallTask phoneDialer;
                            phoneDialer = CrossMessaging.Current.PhoneDialer;
                            if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(request.Phone))
                                phoneDialer.MakePhoneCall(phone);
                        }
                    });
                    bool IsPass = request.PassInfo != null;
                    bool isMan = false;
                    if (IsPass)
                    {
                        isMan = request.PassInfo.CategoryId == 1;
                    }
                    var ret = await Dialog.Instance.ShowAsync<InfoAppDialog>(new
                    {
                        _Request = request,
                        HexColor = this.hex,
                        SourceApp = Source,
                        Calling = Call,
                        isPass = IsPass,
                        isManType = isMan,
                        IsCons = true
                    });
                }
            }
        }

        public Command<string> Call { get; set; }

        private async void Transit_OnTapped(object sender, EventArgs e)
        {
            progress.IsVisible = true;
            var request = await _server.SetPaidRequestStatusOnTheWay(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                await ShowToast(AppResources.TransitOrder);
                await RefreshData();
            }
            else
            {
                await DisplayAlert(AppResources.Order, request.Error, "OK");
            }

            progress.IsVisible = false;
        }

        private async void ReceiptEdit(object sender, EventArgs e)
        {
            List<RequestsReceiptItem> Items = new List<RequestsReceiptItem>();

            if (request.ReceiptItems != null)
                if (request.ReceiptItems.Count() > 0)
                    foreach (var item in request.ReceiptItems)
            {
                Items.Add(item.Copy());
            }

            await Dialog.Instance.ShowAsync(new AppConstDialogWindow(Items, request.ID, request.ShopId));
        }
    }
}