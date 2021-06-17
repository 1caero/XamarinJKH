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
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace xamarinJKH.Apps
{
    /*!
\b Форма работы с заявкой
*/
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
        string TAKE_GALARY_Video = AppResources.AttachmentChooseVideo;
        string TAKE_FILE = AppResources.AttachmentChooseFile;


        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";
        /// <summary>
        /// Отборажения обновления
        /// </summary>
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }
        /// <summary>
        /// Команда обновления данных
        /// </summary>
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
        /// <summary>
        /// Такс обновления данных 
        /// </summary>
        public async Task RefreshData()
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
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="requestInfo">Заявка</param>
        /// <param name="closeAll">Закрыть все форммы в стеке</param>
        /// <param name="paid">Платная заявка</param>
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
            micService = DependencyService.Resolve<IMicrophoneService>();
            Analytics.TrackEvent("Заявка жителя №" + requestInfo.RequestNumber);
            MessagingCenter.Subscribe<Object>(this, "AutoUpdateComments",  (sender) => {  Device.BeginInvokeOnMainThread(async () =>  await RefreshData()); });

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

//            MessagingCenter.Subscribe<ISpeechToText, string>(this, "STT", (sender, args) =>
//            {
//                SpeechToTextFinalResultRecieved(args);
//            });

//            MessagingCenter.Subscribe<ISpeechToText>(this, "Final", (sender) =>
//            {
//                Device.BeginInvokeOnMainThread(() =>
//                {
//                    new PopupPage();
//                    IconViewMic.ReplaceStringMap = new Dictionary<string, string> { { "#000000", hex.ToHex() } };
//                });                
//            });

//            MessagingCenter.Subscribe<IMessageSender, string>(this, "STT", (sender, args) =>
//            {
//                SpeechToTextFinalResultRecieved(args);
//            });


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
            sendMess.Tapped += (s, e) =>
            {
                // sendMessage();
                Device.BeginInvokeOnMainThread(() => sendMessage());
            };
            IconViewSend.GestureRecognizers.Add(sendMess);
            var recordmic = new TapGestureRecognizer();
            recordmic.Tapped += async (s, e) => { RecordMic(); };
            // IconViewMic.GestureRecognizers.Add(recordmic);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { addFileApp(); };
            IconViewAddFile.GestureRecognizers.Add(addFile);
            var showInfo = new TapGestureRecognizer();
            showInfo.Tapped += async (s, e) => { ShowInfo(); };
            StackLayoutInfo.GestureRecognizers.Add(showInfo);
            var closeApp = new TapGestureRecognizer();
            closeApp.Tapped += async (s, e) =>
            {
                if (!_requestInfo.IsClosed)
                {
                    await PopupNavigation.Instance.PushAsync(new RatingBarContentView(hex, _requestInfo, this));
                }
                else
                {
                    await ShowToast(AppResources.AppIsClosed);
                }
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

            var hideKeyBoardgesture = new TapGestureRecognizer();
            hideKeyBoardgesture.Tapped += async (s, e) =>
            {
                if (Device.RuntimePlatform == Device.iOS)
                    MessagingCenter.Send<object>(this, "FocusKeyboardStatus");
                else
                    EntryMess.Unfocus();
                hideKeyBoard.IsVisible = false;
            };
            baseForApp.GestureRecognizers.Add(hideKeyBoardgesture);
        }
        /// <summary>
        /// Транскрибация голосо в текст
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        new string[] { "en-US", "ru-RU" });
                var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);
                recognizer = new SpeechRecognizer(config, autoDetectSourceLanguageConfig);
             
                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
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
                    Analytics.TrackEvent("ошибка при остановке распознавании речи " + ex.Message);
                }
                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                // Device.BeginInvokeOnMainThread(() =>
                // {
                //     InsertDateTimeRecord();
                // });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
#if DEBUG
                    UpdateTranscription(ex.Message);
#endif
                    Analytics.TrackEvent("ошибка при старте распознавании речи " + ex.Message);
                }
                isTranscribing = true;
            }
            UpdateDisplayState();
        }
        /// <summary>
        /// Обновление транскрибации
        /// </summary>
        /// <param name="newText">Новый текст</param>
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
        /// <summary>
        /// Отображение прогресса отапрвки сообщения
        /// </summary>
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
        /// <summary>
        /// Запуск записи с микрофона
        /// </summary>
        private async void RecordMic()
        {
            TranscribeClicked(this, null);
//             try
//             {
//                 _speechRecongnitionInstance.StartSpeechToText();
//             }
//             catch (Exception ex)
//             {
// #if DEBUG
//                 EntryMess.Text = ex.Message;
// #endif
//             }
//
//             if (Device.RuntimePlatform == Device.iOS)
//             {
//                 Device.BeginInvokeOnMainThread(() =>
//                 {
//                     IconViewMic.ReplaceStringMap = new Dictionary<string, string> { { "#000000", "#A2A2A2" } };
//                 });                
//             }
        }

        private void SpeechToTextFinalResultRecieved(string args)
        {
            if(!string.IsNullOrWhiteSpace(args))
                EntryMess.Text += " " + args;            
        }

        //private ISpeechToText _speechRecongnitionInstance;
        /// <summary>
        /// Обработка нажатия физической кнопкия назад
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Добавление файла к заявке
        /// </summary>
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
            string action;
            if (Device.RuntimePlatform == Device.Android)
                 action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
                TAKE_PHOTO,
                TAKE_GALRY,  TAKE_FILE);
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
                            SaveToAlbum = true,// Device.RuntimePlatform==Device.iOS, //ios - сохраняем файлы
                            CompressionQuality =  90,
                            Directory = string.Format(AppInfo.Name.Replace("\"", "") /*+ "_" + AppResources.Photo*/)
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
        /// <summary>
        /// Добавление фото с камеры к заявке
        /// </summary>
        /// <param name="file"></param>
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
        /// <summary>
        /// Старт загрузки файла на сервер
        /// </summary>
        /// <param name="metod">Фото с камеры,Галерея,Файл</param>
        /// <param name="file">сам файл</param>
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
        /// <summary>
        /// Получение файлы из галереи
        /// </summary>
        /// <param name="file">файл</param>
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
        /// <summary>
        /// Перевод потока в байты
        /// </summary>
        /// <param name="stream">поток</param>
        /// <returns>байты</returns>
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
        /// <summary>
        /// Получение имени файла
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Прочтение входного потока
        /// </summary>
        /// <param name="input">входной поток</param>
        /// <returns>байты</returns>
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
                PickOptions pickOptions = new PickOptions
                {
                    PickerTitle = "Выбор файла"
                };
                IconViewAddFile.IsVisible = false;
                progressFile.IsVisible = true;
                FileResult fileResult = await FilePicker.PickAsync(pickOptions);
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


                    CommonResult commonResult = await _server.AddFileApps(_requestInfo.ID.ToString(),
                        fileResult.FileName, stream.ToByteArray(),
                        fileResult.FullPath);
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
        /// <summary>
        /// Отправка сообщения
        /// </summary>
        async void sendMessage()
        {
            try
            {
                string message = EntryMess.Text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    IconViewSend.IsVisible = false;
                    IconViewMic.IsVisible = false;
                    progress.IsVisible = true;
                    //await Task.Delay(3000);
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
                IconViewMic.IsVisible = true;
            }
            catch (Exception e)
            {
                await ShowToast(AppResources.MessageNotSent);

                IconViewMic.IsVisible = true;
                progress.IsVisible = false;
                IconViewSend.IsVisible = true;
            }

        }
        /// <summary>
        /// Отображение короткого всплывающего сообщения
        /// </summary>
        /// <param name="text">текст</param>
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

        /// <summary>
        /// Получение сообщений по заявке
        /// </summary>
        async void getMessage2()
        {

            request = await _server.GetRequestsDetailList(_requestInfo.ID.ToString());
            if (request.Error == null)
            {
                Settings.DateUniq = "";
                _requestInfo = request;
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
                Device.BeginInvokeOnMainThread(async () =>await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorComments, "OK"));
            }

            await MethodWithDelayAsync(1000);

        }
        /// <summary>
        /// Добавление сообщения к отображению
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="prevAuthor">Автор сообщения</param>
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


        /// <summary>
        /// Автоскролл к последнему сообщению
        /// </summary>
        /// <param name="milliseconds"></param>
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
        /// <summary>
        /// Установка текста
        /// </summary>
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
        /// <summary>
        /// Отображение диалога с информацией по заявке
        /// </summary>
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
                            await Dialog.Instance.ShowAsync<InfoAppDialog>(new InfoAppDialogViewModel
                            {
                                _Request = request,
                                HexColor = this.hex,
                                SourceApp = Source,
                                ColorBlack = Color.Black,
                                PassExpiration = _requestInfo.PassExpiration,
                                isPass = IsPass,
                                isManType = isMan,
                                IsCons = false,
                                ShowDispAccepted=!string.IsNullOrEmpty(request.AcceptedDispatcher)
                            });
                    }
                    catch { }
                }
            }
            
            
        }
        /// <summary>
        /// Отображения чека по платной заявке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OpenReceipt(object sender, EventArgs args)
        {
            await Dialog.Instance.ShowAsync(new AppReceiptDialogWindow(new AppRecieptViewModel(request.ReceiptItems)));
        }
        /// <summary>
        /// Обработка нажатия на микрофон
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageButton_OnPressed(object sender, EventArgs e)
        {
            TranscribeClicked(sender, e);
        }
        /// <summary>
        /// Обработка отпускания микрофона
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageButton_OnReleased(object sender, EventArgs e)
        {
            TranscribeClicked(sender, e);
        }
        /// <summary>
        /// Обработка смены фокуса текстового поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntryMess_Focused(object sender, FocusEventArgs e)
        {
            MessagingCenter.Send<object>(this, "SetKeyboardFocusStatic");
            hideKeyBoard.IsVisible = true;
        }

        private void hideKeyBoard_Clicked(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
                MessagingCenter.Send<object>(this, "FocusKeyboardStatus");
            else
                EntryMess.Unfocus();

            hideKeyBoard.IsVisible = false;
        }
    }
}