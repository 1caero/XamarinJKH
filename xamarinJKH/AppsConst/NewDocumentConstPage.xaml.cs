using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs.Abstractions;
using FFImageLoading;
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
using xamarinJKH.MainConst;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.Utils.FileUtils;
using xamarinJKH.Utils.ReqiestUtils;
using xamarinJKH.ViewModels;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace xamarinJKH.AppsConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewDocumentConstPage : ContentPage
    {
        #region AddDocumentModel

        private AddDocumentModel _addDocumentModel;

        public AddDocumentModel AddDocumentModel
        {
            get { return _addDocumentModel; }
            set
            {
                _addDocumentModel = value;
                OnPropertyChanged("AddDocumentModel");
            }
        }

        #endregion

        #region ClosePageCommand

        private ICommand _closePageCommand;

        public ICommand ClosePageCommand
        {
            get { return _closePageCommand; }
            set
            {
                _closePageCommand = value;
                OnPropertyChanged("ClosePageCommand");
            }
        }

        #endregion

        public NewDocumentConstPage()
        {
            InitializeComponent();
            ClosePageCommand = new Command(async () => await Navigation.PopAsync());
            BindingContext = AddDocumentModel = new AddDocumentModel(ClosePageCommand);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;                
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);
            var focusPicker = new TapGestureRecognizer();
            focusPicker.Tapped += async (s, e) =>
            {
                try
                {
                    StackLayout container = (StackLayout) s;
                    StackLayout pickerContainer = (StackLayout) container.Children[0];
                    BorderlessPicker picker = (BorderlessPicker) pickerContainer.Children[0];
                    Device.BeginInvokeOnMainThread(action: async () => picker.Focus());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            };
            StackLayoutType.GestureRecognizers.Add(focusPicker);
            StackLayoutPriority.GestureRecognizers.Add(focusPicker);
            StackLayoutHouse.GestureRecognizers.Add(focusPicker);
            LayoutPremises.GestureRecognizers.Add(focusPicker);
            StackLayoutIdent.GestureRecognizers.Add(focusPicker);
            StackLayoutSource.GestureRecognizers.Add(focusPicker);
            StackLayoutExecutor.GestureRecognizers.Add(focusPicker);
            
            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);
            
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend; 
            LabelTech.GestureRecognizers.Add(techSend);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => ClosePageCommand.Execute(null);
            BackStackLayout.GestureRecognizers.Add(backClick);
        }

        private async void TechSend(object sender, EventArgs e)
        {

            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            string phone = Preferences.Get("techPhone", Settings.Person.Phone);
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(phone))
            {
                Settings.SetPhoneTech(phone);
                await Navigation.PushModalAsync(new AppPage());
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }
        private void BordlessEditor_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AddDocumentModel.TextChangedCommand.Execute(null);
        }
        
        
    }

    public class AddDocumentModel : BaseViewModel
    {
        #region Priority

        private ObservableCollection<NamedValue> _priority;

        public ObservableCollection<NamedValue> Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged("Priority");
            }
        }

        #endregion

        #region DocumentTypes

        private ObservableCollection<DocumentType> _documentTypes;

        public ObservableCollection<DocumentType> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
                OnPropertyChanged("DocumentTypes");
            }
        }

        #endregion

        #region SelectedPriority

        private NamedValue _SelectedPriority;

        public NamedValue SelectedPriority
        {
            get { return _SelectedPriority; }
            set
            {
                _SelectedPriority = value;
                OnPropertyChanged("SelectedPriority");
            }
        }

        #endregion

        #region SelectedDocumentType

        private DocumentType _selectedDocumentType;

        public DocumentType SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
                OnDocumentTypeChanged();
                IsEnabledCreate();
            }
        }

        #endregion

        #region Approvers

        private ObservableCollection<DocumentApproverViewModel> _approvers;

        public ObservableCollection<DocumentApproverViewModel> Approvers
        {
            get { return _approvers; }
            set
            {
                _approvers = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Dialog

        private IDialogNotifier _dialog;

        public IDialogNotifier Dialog
        {
            get { return _dialog; }
            set
            {
                _dialog = value;
                OnPropertyChanged("Dialog");
            }
        }

        #endregion

        #region FileDialog

        private IDialogNotifier _fileDialog;

        public IDialogNotifier FileDialog
        {
            get { return _fileDialog; }
            set
            {
                _fileDialog = value;
                OnPropertyChanged("FileDialog");
            }
        }

        #endregion

        #region CloseFileDialog

        private ICommand _closeFileDialogCommand;

        public ICommand CloseFileDialogCommand
        {
            get { return _closeFileDialogCommand; }
            set
            {
                _closeFileDialogCommand = value;
                OnPropertyChanged("CloseFileDialogCommand");
            }
        }

        #endregion

        #region OpenFileDialogCommand

        private ICommand _openFileDialogCommand;

        public ICommand OpenFileDialogCommand
        {
            get { return _openFileDialogCommand; }
            set
            {
                _openFileDialogCommand = value;
                OnPropertyChanged("OpenFileDialogCommand");
            }
        }

        #endregion

        #region CloseDialogCommand

        private ICommand _closeDialogCommand;

        public ICommand CloseDialogCommand
        {
            get { return _closeDialogCommand; }
            set
            {
                _closeDialogCommand = value;
                OnPropertyChanged("CloseDialogCommand");
            }
        }

        #endregion

        #region OpenApprovedDialogCommand

        private ICommand _openApprovedDialogCommand;

        public ICommand OpenApprovedDialogCommand
        {
            get { return _openApprovedDialogCommand; }
            set
            {
                _openApprovedDialogCommand = value;
                OnPropertyChanged("OpenApprovedDialogCommand");
            }
        }

        #endregion

        #region Houses

        private ObservableCollection<HouseProfile> _hpuses;

        public ObservableCollection<HouseProfile> Houses
        {
            get { return _hpuses; }
            set
            {
                _hpuses = value;
                OnPropertyChanged("Houses");
            }
        }

        #endregion

        #region SelectedHouse

        private HouseProfile _selectedHouse;

        public HouseProfile SelectedHouse
        {
            get { return _selectedHouse; }
            set
            {
                _selectedHouse = value;
                OnPropertyChanged("SelectedHouse");
                OnHouseChanged();
            }
        }

        #endregion

        #region Premises

        private ObservableCollection<R731PremiseWithAccounts> _premises;

        public ObservableCollection<R731PremiseWithAccounts> Premises
        {
            get { return _premises; }
            set
            {
                _premises = value;
                OnPropertyChanged("Premises");
            }
        }

        #endregion

        #region SelectedPremises

        private R731PremiseWithAccounts _selectedPremises;

        public R731PremiseWithAccounts SelectedPremises
        {
            get { return _selectedPremises; }
            set
            {
                _selectedPremises = value;
                Idents = new ObservableCollection<Account>(_selectedPremises.Accounts);
                SelectedIdent = _selectedPremises.Accounts.FirstOrDefault();
                OnPropertyChanged("SelectedPremises");
            }
        }

        #endregion

        #region SelectedIdent

        private Account _selectedIdent;

        #region Idents

        private ObservableCollection<Account> _idents;

        public ObservableCollection<Account> Idents
        {
            get { return _idents; }
            set
            {
                _idents = value;
                OnPropertyChanged("Idents");
            }
        }

        #endregion

        
        
        public Account SelectedIdent
        {
            get { return _selectedIdent; }
            set
            {
                _selectedIdent = value;
                OnPropertyChanged("SelectedIdent");
                if (_selectedIdent != null)
                {
                    if (string.IsNullOrEmpty(Phone))
                    {
                        Phone = _selectedIdent?.Phone;
                    }

                    FIO = _selectedIdent?.FIO;
                }

            }
        }

        #endregion

        #region SourceTypes

        private ObservableCollection<NamedValue> _SourceTypes;

        public ObservableCollection<NamedValue> SourceTypes
        {
            get { return _SourceTypes; }
            set
            {
                _SourceTypes = value;
                OnPropertyChanged("SourceTypes");
            }
        }

        #endregion

        #region SelectedSourceType

        private NamedValue _selectedSourceType;

        public NamedValue SelectedSourceType
        {
            get { return _selectedSourceType; }
            set
            {
                _selectedSourceType = value;
                OnPropertyChanged("SelectedSourceType");
            }
        }

        #endregion

        #region Dispatchers

        private ObservableCollection<ConsultantInfo> _dispatchers;

        public ObservableCollection<ConsultantInfo> Dispatchers
        {
            get { return _dispatchers; }
            set
            {
                _dispatchers = value;
                OnPropertyChanged("Dispatchers");
            }
        }

        #endregion

        #region SelectedDispatcher

        private ConsultantInfo _selectedDispatcher;

        public ConsultantInfo SelectedDispatcher
        {
            get { return _selectedDispatcher; }
            set
            {
                _selectedDispatcher = value;
                OnPropertyChanged("SelectedDispatcher");
            }
        }

        #endregion

        #region Files

        private ObservableCollection<FileData> _files;

        public ObservableCollection<FileData> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        #endregion

        public List<byte[]> Byteses = new List<byte[]>();

        #region AddFileCommand

        private ICommand _addFileCommand;

        public ICommand AddFileCommand
        {
            get { return _addFileCommand; }
            set
            {
                _addFileCommand = value;
                OnPropertyChanged("AddFileCommand");
            }
        }

        #endregion

        #region RemoveFileCommand

        private ICommand _removeFileCommand;

        public ICommand RemoveFileCommand
        {
            get { return _removeFileCommand; }
            set
            {
                _removeFileCommand = value;
                OnPropertyChanged("RemoveFileCommand");
            }
        }

        #endregion

        #region IsEnabled

        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        #endregion

        #region TextDocument

        private string _textDocument;

        public string TextDocument
        {
            get { return _textDocument; }
            set
            {
                _textDocument = value;
                OnPropertyChanged("TextDocument");
            }
        }

        #endregion

        #region TextChangedCommand

        private ICommand _textChangedCommand;

        public ICommand TextChangedCommand
        {
            get { return _textChangedCommand; }
            set
            {
                _textChangedCommand = value;
                OnPropertyChanged("TextChangedCommand");
            }
        }

        #endregion

        #region TitleDocument

        private string _titleDocument;

        public string TitleDocument
        {
            get { return _titleDocument; }
            set
            {
                _titleDocument = value;
                OnPropertyChanged("TitleDocument");
            }
        }

        #endregion

        #region IsDocumentHouses

        private bool _isDocumentHouses;

        public bool IsDocumentHouses
        {
            get { return _isDocumentHouses; }
            set
            {
                _isDocumentHouses = value;
                OnPropertyChanged("IsDocumentHouses");
            }
        }

        #endregion

        #region Floor

        private string _floor;

        public string Floor
        {
            get { return _floor; }
            set
            {
                _floor = value;
                OnPropertyChanged("Floor");
            }
        }

        #endregion

        #region Intercom

        private string _intercom;

        public string Intercom
        {
            get { return _intercom; }
            set
            {
                _intercom = value;
                OnPropertyChanged("Intercom");
            }
        }

        #endregion

        #region FIO

        private string _fio;

        public string FIO
        {
            get { return _fio; }
            set
            {
                _fio = value;
                OnPropertyChanged("FIO");
            }
        }

        #endregion

        #region Phone

        private string _phone;

        public string Phone
        {
            get
            {
                return _phone;
                // string.IsNullOrWhiteSpace(_phone) ? "" : _phone.Replace("+", "")
                // .Replace(" ", "")
                // .Replace("(", "")
                // .Replace(")", "")
                // .Replace("-", "");
            }
            set
            {
                _phone = value;
                OnPropertyChanged("Phone");
            }
        }

        #endregion

        #region DateDocument

        private DateTime _dateDocument = DateTime.Now;

        public DateTime DateDocument
        {
            get { return _dateDocument; }
            set
            {
                _dateDocument = value;
                OnPropertyChanged("DateDocument");
            }
        }

        #endregion

        #region TimeDocument

        private TimeSpan _timeDocument;

        public TimeSpan TimeDocument
        {
            get { return _timeDocument; }
            set
            {
                _timeDocument = value;
                DateDocument = DateDocument.Date + _timeDocument;
                OnPropertyChanged("TimeDocument");
            }
        }

        #endregion

        #region DateTerm

        private DateTime _dateTerm = DateTime.Now;

        public DateTime DateTerm
        {
            get { return _dateTerm; }
            set
            {
                _dateTerm = value;
                _dateTerm = _dateTerm.Date + _timeTerm;
                OnPropertyChanged("DateTerm");
            }
        }

        #endregion

        #region TimeTerm

        private TimeSpan _timeTerm;

        public TimeSpan TimeTerm
        {
            get { return _timeTerm; }
            set
            {
                _timeTerm = value;
                DateTerm = DateTerm.Date + _timeTerm;
                OnPropertyChanged("TimeTerm");
            }
        }

        #endregion

        #region AddDocumentCommand

        private ICommand _addDocumentCommand;

        public ICommand AddDocumentCommand
        {
            get { return _addDocumentCommand; }
            set
            {
                _addDocumentCommand = value;
                OnPropertyChanged("AddDocumentCommand");
            }
        }

        #endregion

        #region ClosePageCommand

        private ICommand _closePageCommand;

        public ICommand ClosePageCommand
        {
            get { return _closePageCommand; }
            set
            {
                _closePageCommand = value;
                OnPropertyChanged("ClosePageCommand");
            }
        }

        #endregion

        public AddDocumentModel(ICommand command)
        {
            ClosePageCommand = command;
            Init();
        }

        private async void Init()
        {
            Files = new ObservableCollection<FileData>();
            List<NamedValue> requestSourceTypes = await Server.GetRequestSourceTypes();
            List<DocumentType> resultTypes = await Server.GetDocumentTypes();
            var dispatchers = await Server.GetConsultants();
            ItemsList<HouseProfile> itemsList = await Server.GetHouse();
            Priority = new ObservableCollection<NamedValue>(Settings.PrioritetsApp);
            DocumentTypes = new ObservableCollection<DocumentType>(resultTypes);
            SelectedPriority = Priority.Count > 1 ? Priority[1] : Priority.FirstOrDefault();
            Approvers = new ObservableCollection<DocumentApproverViewModel>(dispatchers.Where(x => x.Name != null)
                .Select(x => new DocumentApproverViewModel(x)).OrderBy(x => x.Name));
            Houses = new ObservableCollection<HouseProfile>(itemsList.Data.Where(x => x.Address != null));
            SourceTypes = new ObservableCollection<NamedValue>(requestSourceTypes);
            Dispatchers = new ObservableCollection<ConsultantInfo>(dispatchers.Where(x => x.Name != null));
            CloseDialogCommand = new Command(CloseDialogAction);
            CloseFileDialogCommand = new Command(CloseFileDialogAction);
            OpenApprovedDialogCommand = new Command(OpenApprovedAction);
            OpenFileDialogCommand = new Command(OpenFileDialogActions);
            AddFileCommand = new Command(AddFileActions);
            RemoveFileCommand = new Command(RemoveFileAction);
            TextChangedCommand = new Command(IsEnabledCreate);
            AddDocumentCommand = new Command(AddDocumentAction);
        }

        private async void AddDocumentAction()
        {
            IsBusy = true;

            var arguments = new AddDocumentArguments()
            {
                AccountId = SelectedIdent?.ID,
                Approvers = Approvers.Where(x => x.IsChecked).Select(x => x.ID).ToList(),
                Fio = FIO,
                Floor = string.IsNullOrWhiteSpace(Floor) ? "" : Floor.Replace($"{AppResources.Floor} № ", ""),
                HouseId = SelectedHouse?.ID,
                id_SourceType = SelectedSourceType?.ID,
                Intercom = string.IsNullOrWhiteSpace(Intercom)
                    ? ""
                    : Intercom.Replace($"{AppResources.Intercom} № ", ""),
                Name = GetNameOrAutoFilled(),
                Phone = string.IsNullOrWhiteSpace(Phone)
                    ? ""
                    : Phone.Replace("+", "")
                        .Replace(" ", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", ""),
                Priority = SelectedPriority.ID,
                Text = TextDocument,
                Type = SelectedDocumentType.ID,
                DateStartReport = DateTerm
            };

            var createdRequestId = await Server.CreateNewDocument(arguments);
            if (createdRequestId.ID.HasValue)
            {
                int i = 0;
                foreach (var each in Files)
                {
                    CommonResult commonResult = await Server.AddFileAppsConst(createdRequestId.ID.Value.ToString(),
                        each.GetFileName, Byteses[i],
                        each.FilePath);
                    i++;
                }

                if (SelectedDispatcher != null)
                {
                    await Server.LockRequest(createdRequestId.ID.Value, SelectedDispatcher.ID);
                }

                ShowToast("Успешно");
                await Task.Factory.StartNew( RequestUtils.UpdateRequestCons);
                Thread.Sleep(100);
                ClosePageCommand.Execute(null);
            }
            else
            {
                ShowError(createdRequestId.Error);
            }
            IsBusy = false;
        }

        public string GetNameOrAutoFilled()
        {
            if (!string.IsNullOrEmpty(TitleDocument))
            {
                return TitleDocument;
            }

            // Form name from text
            int NameLength = 50;
            if (TextDocument.Length < NameLength)
            {
                return TextDocument;
            }

            var lastSpace = TextDocument.Substring(0, NameLength).LastIndexOf(' ');
            if (lastSpace < 0)
            {
                return TextDocument.Substring(0, NameLength) + "...";
            }

            return TextDocument.Substring(0, lastSpace) + "...";
        }

        private void IsEnabledCreate()
        {
            IsEnabled = !string.IsNullOrWhiteSpace(TextDocument) && SelectedDocumentType != null;
        }

        private async void RemoveFileAction(object obj)
        {
            var choose = await ShowChoose(AppResources.DeleteFile);
            if (obj != null && choose)
            {
                FileData select = (FileData) obj;
                Device.BeginInvokeOnMainThread(action: async () => Files.Remove(select));
            }
        }

        private async void AddFileActions()
        {
            var action = await Application.Current.MainPage.DisplayActionSheet(AppResources.AttachmentTitle,
                AppResources.Cancel, null,
                AppResources.AttachmentTakePhoto,
                AppResources.AttachmentChoosePhoto, AppResources.AttachmentChooseFile);
            if (action == AppResources.AttachmentTakePhoto)
            {
                await GetCameraFile();
                return;
            }

            if (action == AppResources.AttachmentChoosePhoto)
            {
                await GetGalaryFile();
                return;
            }

            if (action == AppResources.AttachmentChooseFile)
            {
                await PickAndShowFile();
            }
        }

        private async Task PickAndShowFile()
        {
            try
            {
                FileResult fileResult = await FilePicker.PickAsync(new PickOptions());
                if (fileResult != null)
                {
                    Stream stream = await fileResult.OpenReadAsync();
                    if (stream.Length > 10000000)
                    {
                        ShowError(AppResources.FileTooBig);
                        return;
                    }

                    Device.BeginInvokeOnMainThread(action: async () =>
                        Files.Add(new FileData(fileResult.FullPath, fileResult.FileName, stream)));
                    Byteses.Add(stream.ToByteArray());
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.ToString());
            }
        }

        async Task GetGalaryFile()
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    ShowError(AppResources.ErrorCameraNotAvailable);

                    return;
                }

                var file = await CrossMedia.Current.PickPhotoAsync();
                if (file == null)
                    return;
                FileData fileData = new FileData(file.Path, FileData.getFileName(file.Path), file.GetStream());
                Byteses.Add(FileData.StreamToByteArray(file.GetStream()));
                Device.BeginInvokeOnMainThread(action: async () => Files.Add(fileData));
            }
            catch (Exception ex)
            {
                ShowError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        async Task GetCameraFile()
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
                {
                    ShowError(AppResources.ErrorCameraNotAvailable);
                    return;
                }

                MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                    new StoreCameraMediaOptions
                    {
                        SaveToAlbum = true,
                        CompressionQuality = 90,
                        Directory = AppInfo.Name.Replace("\"", "")
                    });

                if (file == null)
                    return;
                FileData fileData = new FileData(file.Path, FileData.getFileName(file.Path), file.GetStream());
                Byteses.Add(FileData.StreamToByteArray(file.GetStream()));
                Device.BeginInvokeOnMainThread(action: async () => Files.Add(fileData));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowError($"{e.Message}\n{e.StackTrace}");
            }
        }

        private async void OpenFileDialogActions()
        {
            await ShowDialog<FilesDialog>(this);
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
                        var result = await Application.Current.MainPage.DisplayAlert(AppResources.ErrorTitle,
                            AppResources.ErrorNoPermissions, "OK",
                            AppResources.Cancel);
                        if (result)
                            CrossPermissions.Current.OpenAppSettings();
                    });
                    return;
                }
            }
        }

        private void CloseFileDialogAction()
        {
            if (FileDialog != null) FileDialog.Cancel();
        }

        private async void OpenApprovedAction()
        {
            await ShowDialog<ApproversDialog>(this);
        }

        private void CloseDialogAction()
        {
            if (Dialog != null) Dialog.Cancel();
        }

        private async void OnHouseChanged()
        {
            if (SelectedHouse != null)
            {
                ItemsList<R731PremiseWithAccounts> houseData = await Server.GetHouseData(SelectedHouse.ID.ToString());
                Premises = new ObservableCollection<R731PremiseWithAccounts>(
                    houseData.Data.Where(x => x.Number != null));
            }
        }

        private void OnDocumentTypeChanged()
        {
            if (SelectedDocumentType != null)
            {
                foreach (var approver in Approvers)
                {
                    approver.IsChecked = SelectedDocumentType.Approvers.Contains(approver.ID);
                }
            }
        }

        public class DocumentApproverViewModel : BaseViewModel
        {
            private ConsultantInfo _dispatcher;

            public DocumentApproverViewModel(ConsultantInfo dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public string Name => _dispatcher.Name;

            public long ID => _dispatcher.ID;

            #region IsChecked

            private bool _isChecked;

            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }

            #endregion
        }
    }
}