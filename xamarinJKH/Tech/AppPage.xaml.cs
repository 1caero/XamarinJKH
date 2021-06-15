using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using FFImageLoading;
using Microsoft.AppCenter.Analytics;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Apps;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.DialogViewModels;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace xamarinJKH.Tech
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppPage : ContentPage
    {
        private readonly bool _isDeviceId;
        private RequestInfo _requestInfo;
        private RequestContent request = new RequestContent
        {
            Messages = new List<RequestMessage>()
        };

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

            MessageBoxStartHeigth = EntryMess.Height;

            Device.BeginInvokeOnMainThread(() =>
                isRunning = true
            );
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;

            var UpdateTask = new Task(async () =>
            {
                try
                {
                    while (!Token.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        var update = await _server.GetRequestsDetailListTech(Settings.Person.Phone, GetLastIdMessage(),
                            _isDeviceId);
                        if (update.Error == null)
                        {
                            foreach (var each in update.Messages)
                            {
                                if (!messages.Contains(each))
                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        addAppMessage(each,
                                            messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                                        var lastChild = baseForApp.Children.LastOrDefault();

                                        if (lastChild != null)
                                        {
                                            await scrollFroAppMessages.ScrollToAsync(lastChild.X, lastChild.Y + 30,
                                                false);
                                        }
                                    });
                                request.Messages.Add(each);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }, Token);
            try
            {
                UpdateTask.Start();
            }
            catch
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            if (Device.RuntimePlatform == "Android")
            {
                return;
            }
        }

        public string DateUniq = "";


        protected override void OnDisappearing()
        {
            Device.BeginInvokeOnMainThread(() =>
                isRunning = false
            );
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
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            var update =
                await _server.GetRequestsDetailListTech(Settings.Person.Phone, GetLastIdMessage(), _isDeviceId);
            if (update.Error == null)
            {
                //Settings.DateUniq = "";

                foreach (var each in update.Messages)
                {
                    if (!messages.Contains(each))
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            addAppMessage(each, messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null);
                            var lastChild = baseForApp.Children.LastOrDefault();

                            if (lastChild != null)
                                Device.BeginInvokeOnMainThread(async () =>
                                    await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false));
                        });
                        messages.Add(each);
                        request.Messages.Add(each);
                    }
                }

                var lastChild = baseForApp.Children.LastOrDefault();

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (lastChild != null)
                        await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false);
                });
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK");
            }
        }

        string GetLastIdMessage()
        {
            String lastId = null;
            try
            {
                lastId = request.Messages.LastOrDefault()?.ID.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return lastId;
        }

        List<RequestMessage> messages = new List<RequestMessage>();

        public Color hex { get; set; }
        public bool isPayd { get; set; }

        public bool close = false;
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;

        public bool isUser { get; set; }

        double MessageBoxStartHeigth = -1;

        public AppPage(bool isDeviceId = false)
        {
            _isDeviceId = isDeviceId;
            Device.BeginInvokeOnMainThread(() =>
                isRunning = true
            );
            InitializeComponent();

            isUser = !Settings.ConstAuth;
            if (!isUser)
            {
                FrameMessage.CornerRadius = new CornerRadius(30);
            }

            Resources["hexColor"] = (Color) Application.Current.Resources["MainColor"];

            LabelUkP.IsVisible = LabelUKLink.IsVisible = LayoutCallUK.IsVisible = App.isStart && Settings.AppIsVisible;

            LabelUk.Text = LabelUk.Text.Replace("УК", Settings.MobileSettings.main_name);
            //LabelUKLink.Text = LabelUKLink.Text.Replace("УК", Settings.MobileSettings.main_name);
            micService = DependencyService.Resolve<IMicrophoneService>();
            Analytics.TrackEvent("Диалог с техподдержкой ");
//            try
//            {
//                _speechRecongnitionInstance = DependencyService.Get<ISpeechToText>();
//            }
//            catch (Exception ex)
//            {
//#if DEBUG
//                //ошибку выводить в сообщение для дебага
//                EntryMess.Text = ex.Message;
//#endif
//                throw ex;
//            }

            //MessagingCenter.Subscribe<ISpeechToText, string>(this, "STT",
            //    (sender, args) => { SpeechToTextFinalResultRecieved(args); });

            //MessagingCenter.Subscribe<ISpeechToText>(this, "Final", (sender) =>
            //{
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        new PopupPage();
            //        IconViewMic.ReplaceStringMap = new Dictionary<string, string> {{"#000000", hex.ToHex()}};
            //    });
            //});

            //MessagingCenter.Subscribe<IMessageSender, string>(this, "STT",
            //    (sender, args) => { SpeechToTextFinalResultRecieved(args); });


            messages =
                new List<RequestMessage>();

            hex = (Color) Application.Current.Resources["MainColor"];
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
                    Device.BeginInvokeOnMainThread(() => EntryMess.HeightRequest = -1);
                    break;
                default:
                    break;
            }
            var hideKeyBoardgesture = new TapGestureRecognizer();
            hideKeyBoardgesture.Tapped += async (s, e) =>
            {
                MessagingCenter.Send<object>(this, "FocusKeyboardStatus");
            };
            baseForApp.GestureRecognizers.Add(hideKeyBoardgesture);

            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
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
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var sendMess = new TapGestureRecognizer();
            sendMess.Tapped += (s, e) => { Device.BeginInvokeOnMainThread( ()=> sendMessage()); };
            IconViewSend.GestureRecognizers.Add(sendMess);

            var addApp = new TapGestureRecognizer();
            addApp.Tapped += async (s, e) =>
            {
                try
                {
                    if (Navigation.NavigationStack.FirstOrDefault(x => x is NewAppPage) == null)
                    {
                        await Navigation.PushAsync(new NewAppPage());
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            };
            //LabelUKLink.GestureRecognizers.Add(addApp);
            LayoutCallUK.GestureRecognizers.Add(addApp);
            // var recordmic = new TapGestureRecognizer();
            // recordmic.Tapped += async (s, e) => { RecordMic(); };
            // IconViewMic.GestureRecognizers.Add(recordmic);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += (s, e) => { addFileApp(); };
            IconViewAddFile.GestureRecognizers.Add(addFile);

            setText();

            EntryMess.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeSentence | KeyboardFlags.Spellcheck);
        }


//        private async void RecordMic()
//        {
//            try
//            {
//                _speechRecongnitionInstance.StartSpeechToText();
//            }
//            catch (Exception ex)
//            {
//#if DEBUG
//                EntryMess.Text = ex.Message;
//#endif
//            }

//            if (Device.RuntimePlatform == Device.iOS)
//            {
//                Device.BeginInvokeOnMainThread(() =>
//                {
//                    IconViewMic.ReplaceStringMap = new Dictionary<string, string> {{"#000000", "#A2A2A2"}};
//                });
//            }
//        }

        private void SpeechToTextFinalResultRecieved(string args)
        {
            if (!string.IsNullOrWhiteSpace(args))
                EntryMess.Text += " " + args;
        }

        //private ISpeechToText _speechRecongnitionInstance;

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
                            SaveToAlbum = true, // Device.RuntimePlatform==Device.iOS, //ios - сохраняем файлы
                            CompressionQuality = 90,
                            Directory = string.Format(
                                AppInfo.Name.Replace("\"", "") /*+ "_" + AppResources.Photo*/)
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
                await startLoadFile(FILE, null);

            Loading.Instance.Hide();
        }

        async Task getCameraFile(MediaFile file)
        {
            if (file == null)
                return;
            CommonResult commonResult = await _server.AddFileAppsTech(Settings.Person.Phone,
                getFileName(file.Path), StreamToByteArray(file.GetStream()),
                file.Path, _isDeviceId);
            if (commonResult != null)
            {
                if (string.IsNullOrEmpty(commonResult.Error))
                {
                    await ShowToast(AppResources.SuccessFileSent);
                    await RefreshData();
                }
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
            CommonResult commonResult = await _server.AddFileAppsTech(Settings.Person.Phone,
                getFileName(file.Path), StreamToByteArray(file.GetStream()),
                file.Path, _isDeviceId);
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
                FileResult fileResult = await FilePicker.PickAsync(new PickOptions());
                if (fileResult != null)
                {
                    Stream stream = await fileResult.OpenReadAsync();
                    if (stream.Length > 10000000)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.FileTooBig, "OK");
                        IconViewAddFile.IsVisible = true;
                        progressFile.IsVisible = false;
                        return;
                    }

                    CommonResult commonResult = await _server.AddFileAppsTech(Settings.Person.Phone,
                        fileResult.FileName, stream.ToByteArray(),
                        fileResult.FullPath, _isDeviceId);
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
                    // Device.BeginInvokeOnMainThread(() =>
                    //{
                        IconViewSend.IsVisible = false;
                        IconViewMic.IsVisible = false;
                    progress.IsVisible = true;
                    //});
                    //await Task.Delay(1000);
                    IsSucceed result = await _server.AddMessageTech(message, Settings.Person.Phone, _isDeviceId);
                    if (result.isSucceed)
                    {
                        //Device.BeginInvokeOnMainThread(() =>
                        //    {
                        //        {
                                    EntryMess.Text = "";
                                    
                                    // var lastChild = baseForApp.Children.LastOrDefault();
                                    // if (lastChild != null)
                                    //     Device.BeginInvokeOnMainThread(async () =>
                                    //         await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false));
                        //        }
                        //    }
                        //);
                    }
                }
                else
                {
                    await ShowToast(AppResources.ErrorMessageEmpty);
                }
            }
            catch (Exception e)
            {
                await ShowToast(AppResources.MessageNotSent);
            }
            finally
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                    IconViewSend.IsVisible = true;
                    IconViewMic.IsVisible = true;
                progress.IsVisible = false;

                //});
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

        public bool isRunning = false;

        async void getMessage2()
        {
            Analytics.TrackEvent("Запрос сообщений");
            if (!isRunning)
            {
                return;
            }

            new Task(async () =>
            {
                Configurations.LoadingConfig = new LoadingConfig
                {
                    IndicatorColor = (Color) Application.Current.Resources["MainColor"],
                    OverlayColor = Color.Black,
                    Opacity = 0.8,
                    DefaultMessage = AppResources.Loading,
                };
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                Device.BeginInvokeOnMainThread(async () =>
                    await Loading.Instance.StartAsync(async progress =>
                    {
                        Analytics.TrackEvent("Запрос сообщений " + Settings.Person.Phone);
                        request = await _server.GetRequestsDetailListTech(Settings.Person.Phone, null, _isDeviceId);
                        if (request.Error == null)
                        {
                            Analytics.TrackEvent("Результат запроса " + JsonConvert.SerializeObject(request));
                            Settings.DateUniq = "";
                            foreach (var message in request.Messages)
                            {
                                if (!messages.Contains(message))
                                {
                                    Device.BeginInvokeOnMainThread(() => addAppMessage(message,
                                        messages.Count > 1 ? messages[messages.Count - 2].AuthorName : null));
                                    messages.Add(message);
                                }
                            }
                        }
                        else
                        {
                            await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK");
                        }

                        await MethodWithDelayAsync(1000);
                    })
                );
            }).Start();
        }

        void addAppMessage(RequestMessage message, string prevAuthor)
        {
            StackLayout data;
            string newDate;
            if (message.IsSelf)
            {
                data = new MessageCellAuthor(message, this, DateUniq, out newDate, true);
            }
            else
            {
                data = new MessageCellService(message, this, DateUniq, out newDate, prevAuthor, true);
            }

            DateUniq = newDate;

            baseForApp.Children.Add(data);
        }


        public async Task MethodWithDelayAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);

            try
            {
                //additionalList.ScrollTo(messages[messages.Count - 1], 0, true);
                var lastChild = baseForApp.Children.LastOrDefault();
                if (lastChild != null)
                    //await scrollFroAppMessages.ScrollToAsync(lastChild.X, lastChild.Y + 30, true);// (lastChild.X, lastChild.Y + 30, true);
                    await scrollFroAppMessages.ScrollToAsync(lastChild, ScrollToPosition.End, false);
            }
            catch
            {
            }
        }

        async void setText()
        {
            await CrossMedia.Current.Initialize();
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            // FrameMessage.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.White);
        }

        private async void ShowInfo()
        {
            if (request != null)
            {
                string Status = request.Status;
                if (!string.IsNullOrEmpty(Status))
                {
                    string Source = Settings.GetStatusIcon(request.StatusID);
                    if (!string.IsNullOrWhiteSpace(request.Phone) && (request.Phone.Contains("+") == false &&
                                                                      request.Phone.Substring(0, 2) == "79"))
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
                            await Dialog.Instance.ShowAsync<InfoAppDialog>(new InfoAppDialogViewModel
                            {
                                _Request = request,
                                HexColor = this.hex,
                                SourceApp = Source,
                                isPass = IsPass,
                                isManType = isMan,
                            });
                    }
                    catch
                    {
                    }
                }
            }
        }

        public async Task ShowRating()
        {
            await Settings.StartOverlayBackground(hex);
        }

        private async void OpenInfo(object sender, EventArgs e)
        {
            ShowInfo();
        }

        private async void OpenReceipt(object sender, EventArgs args)
        {
            await Dialog.Instance.ShowAsync(
                new AppReceiptDialogWindow(new AppRecieptViewModel(request.ReceiptItems)));
        }


        private void EntryMess_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as BordlessEditorChat;
            if(e.NewTextValue==""&& Device.RuntimePlatform != Device.Android)
            {
              Device.BeginInvokeOnMainThread(()=>  entry.HeightRequest = MessageBoxStartHeigth);
            }
            else
                Device.BeginInvokeOnMainThread(() => entry.HeightRequest = -1);
            //entry.HeightRequest = -1;


            //entry.MinimumHeightRequest = 100;

            //if (entry != null)
            //{
            //    if (entry.Height > 121)
            //    {
            //        entry.HeightRequest = 120;
            //        entry.AutoSize = EditorAutoSizeOption.Disabled;
            //    }
            //    else
            //    {
            //        if (e.OldTextValue != null && e.NewTextValue.Length < e.OldTextValue.Length &&
            //            e.NewTextValue.Length < 100)
            //        {
            //            entry.HeightRequest = -1;
            //        }

            //        entry.AutoSize = EditorAutoSizeOption.TextChanges;
            //    }
            //}
        }

        private void ImageButton_OnPressed(object sender, EventArgs e)
        {
            TranscribeClicked(sender, e);
        }

        private void ImageButton_OnReleased(object sender, EventArgs e)
        {
            TranscribeClicked(sender, e);
        }

        async void TranscribeClicked(object sender, EventArgs e)
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                // UpdateTranscription("Please grant access to the microphone!");
                ShowToast("Пожалуйста, предоставьте доступ к микрофону!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(
                    new string[] {"en-US", "ru-RU"});
                var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey,
                    Constants.CognitiveServicesRegion);
                recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig);

                recognizer.Recognized += (obj, args) => { UpdateTranscription(args.Result.Text); };
            }

            // if already transcribing, stop speech recognizer
            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
#if DEBUG
                    UpdateTranscription(ex.Message);
#endif
                    Analytics.TrackEvent("ошибка при остановке распознавании речи: " + ex.Message);
                }

                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
#if DEBUG
                    UpdateTranscription(ex.Message);
#endif
                    Analytics.TrackEvent("ошибка при старте распознавании речи: " + ex.Message);
                }

                isTranscribing = true;
            }

            UpdateDisplayState();
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    EntryMess.Text += $"{newText} ";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"=================\n{DateTime.Now.ToString()}\n=================";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isTranscribing)
                {
                    // transcribeButton.Text = "Stop";
                    // IconViewMic.BackgroundColor = Color.Red;
                    progressRecognition.IsVisible = true;
                }
                else
                {
                    // transcribeButton.Text = "Transcribe";
                    // IconViewMic.BackgroundColor = Color.Green;
                    progressRecognition.IsVisible = false;
                }
            });
        }

        private void EntryMess_Focused(object sender, FocusEventArgs e)
        {
            MessagingCenter.Send<object>(this, "SetKeyboardFocusStatic");
        }
    }
}