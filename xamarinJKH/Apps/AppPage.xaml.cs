using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.DialogViewModels;

namespace xamarinJKH.Apps
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppPage : ContentPage
    {
        private RequestInfo _requestInfo;
        private RequestContent request;

        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        string TAKE_PHOTO = AppResources.AttachmentTakePhoto;
        string TAKE_GALRY = AppResources.AttachmentChoosePhoto;
        string TAKE_FILE = AppResources.AttachmentChooseFile;
        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";

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

        public bool closeMessages { get; set; } = !Settings.MobileSettings.disableCommentingRequests;
        CancellationTokenSource TokenSource { get; set; }
        CancellationToken Token { get; set; }
        bool PermissionAsked;
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            RequestsUpdate requestsUpdate =
                await _server.GetRequestsUpdates(Settings.UpdateKey, _requestInfo.ID.ToString());
            if (requestsUpdate.Error == null)
            {
                Settings.UpdateKey = requestsUpdate.NewUpdateKey;
            }
                var UpdateTask = new Task(async () =>
            {
                try
                {
                    
                        var update = await _server.GetRequestsUpdates(Settings.UpdateKey, _requestInfo.ID.ToString());
                        if (update.Error == null)
                        {
                            Settings.UpdateKey = update.NewUpdateKey;
                            if (update.CurrentRequestUpdates != null)
                            {

                                request = update.CurrentRequestUpdates;
                                foreach (var each in update.CurrentRequestUpdates.Messages)
                                {
                                    if (!messages.Contains(each))
                                        Device.BeginInvokeOnMainThread(async () =>
                                        {
                                            addAppMessage(each, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                                        });
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
                }
                catch (Exception e)
                {

                }

            });
            try
            {
                UpdateTask.Start();
            }
            catch { }
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        public string DateUniq = "";


        protected override void OnDisappearing()
        {
            try
            {
                TokenSource.Cancel();
                TokenSource.Dispose();
            }
            catch
            {

            }
            MessagingCenter.Send<Object>(this, "AutoUpdate");
            base.OnDisappearing();
        }

        private async Task RefreshData()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            RequestsUpdate requestsUpdate =
                await _server.GetRequestsUpdates(Settings.UpdateKey, _requestInfo.ID.ToString());
            if (requestsUpdate.Error == null)
            {
                Settings.UpdateKey = requestsUpdate.NewUpdateKey;
                if (requestsUpdate.CurrentRequestUpdates != null)
                {
                    request = requestsUpdate.CurrentRequestUpdates;
                    foreach (var each in requestsUpdate.CurrentRequestUpdates.Messages)
                    {
                        if (!messages.Contains(each))
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                addAppMessage(each, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                            });
                            messages.Add(each);
                        }

                    }
                    Device.BeginInvokeOnMainThread(async () =>
                    {                        
                        var lastChild = baseForApp.Children.LastOrDefault();
                        if(FrameMessage.Height< baseForApp.Height)
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

        }

        List<RequestMessage> messages = new List<RequestMessage>();

        public Color hex { get; set; }
        public bool isPayd { get; set; }

        public bool close = false;

        public AppPage(RequestInfo requestInfo, bool closeAll = false, bool paid = false)
        {
            close = closeAll;
            isPayd = requestInfo.IsPaid;
            if (paid)
            {
                isPayd = paid;
            }
            _requestInfo = requestInfo;
            InitializeComponent();
            Analytics.TrackEvent("Заявка жителя №" + requestInfo.RequestNumber);
            MessagingCenter.Subscribe<Object>(this, "AutoUpdateComments",  (sender) => {  Device.BeginInvokeOnMainThread(async () =>  await RefreshData()); });

            try
            {
                _speechRecongnitionInstance = DependencyService.Get<ISpeechToText>();
            }
            catch (Exception ex)
            {
#if DEBUG
                //ошибку выводить в сообщение для дебага
                EntryMess.Text = ex.Message;
#endif
                throw ex;
            }

            MessagingCenter.Subscribe<ISpeechToText, string>(this, "STT", (sender, args) =>
            {
                SpeechToTextFinalResultRecieved(args);
            });

            MessagingCenter.Subscribe<ISpeechToText>(this, "Final", (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    new PopupPage();
                    IconViewMic.ReplaceStringMap = new Dictionary<string, string> { { "#000000", hex.ToHex() } };
                });                
            });

            MessagingCenter.Subscribe<IMessageSender, string>(this, "STT", (sender, args) =>
            {
                SpeechToTextFinalResultRecieved(args);
            });


            if (!isPayd)
            {
                ScrollView.WidthRequest = 100;
            }
            messages = new List<RequestMessage>();

            hex = (Color)Application.Current.Resources["MainColor"];
            getMessage2();
            this.BindingContext = this;

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
                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                if (close)
                {
                    MessagingCenter.Send<Object>(this, "LoadGoods");
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
                    catch { _ = await Navigation.PopModalAsync(); }
                }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var sendMess = new TapGestureRecognizer();
            sendMess.Tapped += async (s, e) => { sendMessage(); };
            IconViewSend.GestureRecognizers.Add(sendMess);
            var recordmic = new TapGestureRecognizer();
            recordmic.Tapped += async (s, e) => { RecordMic(); };
            IconViewMic.GestureRecognizers.Add(recordmic);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { addFileApp(); };
            IconViewAddFile.GestureRecognizers.Add(addFile);
            var showInfo = new TapGestureRecognizer();
            showInfo.Tapped += async (s, e) => { ShowInfo(); };
            StackLayoutInfo.GestureRecognizers.Add(showInfo);
            var closeApp = new TapGestureRecognizer();
            closeApp.Tapped += async (s, e) =>
            {
                await PopupNavigation.Instance.PushAsync(new RatingBarContentView(hex, _requestInfo, false));
                await RefreshData();
            };
            StackLayoutClose.GestureRecognizers.Add(closeApp);

            var pay = new TapGestureRecognizer();
            pay.Tapped += async (s, e) =>
            {
                await PopupNavigation.Instance.PushAsync(new PayAppDialog(hex, request, this));
                await RefreshData();
            };
            StackLayoutPlay.GestureRecognizers.Add(pay);
            setText();
            if (!requestInfo.IsReadedByClient)
            {
                Task.Run(async () =>
                {
                    var res = await _server.SetReadedFlag(requestInfo.ID, false);
                    MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", -1);
                    MessagingCenter.Send<Object, int>(this, "SetAppRead", requestInfo.ID);
                    requestInfo.IsReadedByClient = true;
                });
            }
        }


        private async void RecordMic()
        {
            try
            {
                _speechRecongnitionInstance.StartSpeechToText();
            }
            catch (Exception ex)
            {
#if DEBUG
                EntryMess.Text = ex.Message;
#endif
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IconViewMic.ReplaceStringMap = new Dictionary<string, string> { { "#000000", "#A2A2A2" } };
                });                
            }
        }

        private void SpeechToTextFinalResultRecieved(string args)
        {
            if(!string.IsNullOrWhiteSpace(args))
                EntryMess.Text += " " + args;            
        }

        private ISpeechToText _speechRecongnitionInstance;
        
        protected override bool OnBackButtonPressed()
        {
            if (close)
            {
                MessagingCenter.Send<Object>(this, "LoadGoods");
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
        async void addFileApp()
        {
            MediaFile file = null;
            if (Device.RuntimePlatform == "Android")
            {
                try
                {
                    var camera_perm = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                    var storage_perm = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                    if (camera_perm != PermissionStatus.Granted || storage_perm != PermissionStatus.Granted)
                    {
                        var status = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera, Permission.Storage);
                        if (status[Permission.Camera] == PermissionStatus.Denied && status[Permission.Storage] == PermissionStatus.Denied)
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var result = await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoPermissions, "OK", AppResources.Cancel);
                        if (result)
                            CrossPermissions.Current.OpenAppSettings();

                    });
                    return;
                }
            }
            var action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
                TAKE_PHOTO,
                TAKE_GALRY, TAKE_FILE);
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
                            Directory = "Demo"
                        });
                    if (file != null)
                        await startLoadFile(CAMERA, file);
                }
                catch (Exception e)
                {
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
                    file = await CrossMedia.Current.PickPhotoAsync();
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
                await startLoadFile(FILE, null);

            Loading.Instance.Hide();
        }

        async Task getCameraFile(MediaFile file)
        {
            if (file == null)
                return;
            CommonResult commonResult = await _server.AddFileApps(_requestInfo.ID.ToString(),
                getFileName(file.Path), StreamToByteArray(file.GetStream()),
                file.Path);
            if (commonResult == null)
            {
                await ShowToast(AppResources.SuccessFileSent);
                await RefreshData();
            }
        }

        public async Task startLoadFile(string metod, MediaFile file)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = hex,
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.FileSending,
            };

            await Loading.Instance.StartAsync(async progress =>
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
            });
        }

        async Task GetGalaryFile(MediaFile file)
        {
            CommonResult commonResult = await _server.AddFileApps(_requestInfo.ID.ToString(),
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
                return ((MemoryStream)stream).ToArray();
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


                    CommonResult commonResult = await _server.AddFileApps(_requestInfo.ID.ToString(),
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

        async void sendMessage()
        {
            try
            {
                string message = EntryMess.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    progress.IsVisible = true;
                    IconViewSend.IsVisible = false;
                    CommonResult result = await _server.AddMessage(message, _requestInfo.ID.ToString());
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
            catch (Exception e)
            {
                await ShowToast(AppResources.MessageNotSent);


                progress.IsVisible = false;
                IconViewSend.IsVisible = true;
            }

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

            request = await _server.GetRequestsDetailList(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                Settings.DateUniq = "";
                StackLayoutPlay.IsVisible = request.IsPaid;
                LayoutResipt.IsVisible = request.IsPaid;
                foreach (var message in request.Messages)
                {
                    if (!messages.Contains(message))
                    {
                        Device.BeginInvokeOnMainThread(() => addAppMessage(message, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null));
                        messages.Add(message);
                    }

                }
                LabelNumber.Text = "№ " + request.RequestNumber;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK");
            }

            await MethodWithDelayAsync(1000);

        }

        void addAppMessage(RequestMessage message, string prevAuthor)
        {
            StackLayout data;
            string newDate;
            if (message.IsSelf)
            {
                data = new MessageCellAuthor(message, this, DateUniq, out newDate);
            }
            else
            {
                data = new MessageCellService(message, this, DateUniq, out newDate, prevAuthor);
            }

            DateUniq = newDate;

            baseForApp.Children.Add(data);
        }



        public async Task MethodWithDelayAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);

            try
            {
                var lastChild = baseForApp.Children.LastOrDefault();
                if (lastChild != null)
                    await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, true);
            }
            catch { }
        }

        async void setText()
        {
            await CrossMedia.Current.Initialize();

            try
            {
                LabelNumber.Text = "№ " + _requestInfo.RequestNumber;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Color hexColor = (Color)Application.Current.Resources["MainColor"];
            FrameKeys.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#B5B5B5"));
            FrameMessage.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);

        }

        private async void ShowInfo()
        {
            if (request != null)
            {
                string Status = request.Status;
                if (!string.IsNullOrEmpty(Status))
                {
                    string Source = Settings.GetStatusIcon(request.StatusID);
                    if (!string.IsNullOrWhiteSpace(request.Phone) && (request.Phone.Contains("+") == false && request.Phone.Substring(0, 2) == "79"))
                    {
                        request.Phone = "+" + request.Phone;
                    }

                    bool IsPass = request.PassInfo != null;
                    bool isMan = false;
                    if (IsPass)
                    {
                        isMan = request.PassInfo.CategoryId == 1;
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(Source))
                            await Dialog.Instance.ShowAsync<InfoAppDialog>(new
                            {
                                _Request = request,
                                HexColor = this.hex,
                                SourceApp = Source,
                                isPass = IsPass,
                                isManType = isMan,
                                IsCons = false
                            });
                    }
                    catch { }
                }
            }
            
            
        }

        private async void OpenReceipt(object sender, EventArgs args)
        {
            await Dialog.Instance.ShowAsync(new AppReceiptDialogWindow(new AppRecieptViewModel(request.ReceiptItems)));
        }

    }
}