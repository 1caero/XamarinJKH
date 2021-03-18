using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.AppsConst;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels;
using PermissionStatus = Plugin.Permissions.Abstractions.PermissionStatus;

namespace xamarinJKH.Apps
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewAppPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        public ObservableCollection<FileData> files { get; set; }
        public List<byte[]> Byteses = new List<byte[]>();
        private AppsPage _appsPage;
        string TAKE_PHOTO = AppResources.AttachmentTakePhoto;
        string TAKE_GALRY = AppResources.AttachmentChoosePhoto;
        string TAKE_FILE = AppResources.AttachmentChooseFile;
        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";
        public int PikerLsItem = 0;
        public int PikerTypeItem = 0;
        private AddAppModel _appModel;
        private bool isPassAPP = false;
        PassApp _passApp = new PassApp();
        public NewAppPage(bool isPassApp = false)
        {
            isPassAPP = isPassApp;
            InitializeComponent();
           
            Analytics.TrackEvent("Создание заявки");
            FrameFlat.IsVisible = Settings.MobileSettings.isRequiredFloor;
            FrameEntrance.IsVisible = Settings.MobileSettings.isRequiredEntrance;
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

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
               ClosePage();
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            
            var takeDateTime = new TapGestureRecognizer();
            takeDateTime.Tapped += async (s, e) => {
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
                            Analytics.TrackEvent("Календарь выбора дня");
                            var ret = await Dialog.Instance.ShowAsync(new CalendarDayDialog(false, _appModel.SelectDate));
                        });
                    }
                );
            };
            LayoutValidity.GestureRecognizers.Add(takeDateTime);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => 
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is Tech.AppPage) == null)
                    await Navigation.PushAsync(new Tech.AppPage());
  
            };
            LabelTech.GestureRecognizers.Add(techSend);
            LabelTechOne.GestureRecognizers.Add(techSend);
            var pickType = new TapGestureRecognizer();
            pickType.Tapped += async (s, e) => {  
                Device.BeginInvokeOnMainThread(() =>
                {
                    PickerType.Focus();
                });
            };
            StackLayoutType.GestureRecognizers.Add(pickType);
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
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { AddFile(); };
            StackLayoutAddFile.GestureRecognizers.Add(addFile);
            
            SetText();
            files = new ObservableCollection<FileData>();

            List<AccountInfo> accs = new List<AccountInfo>();
            accs = isPassAPP ? Settings.Person.Accounts.Where(_ => _.AllowPassRequestCreation).ToList() : Settings.Person.Accounts;
            
#if DEBUG
            _appModel = new AddAppModel()
            {
                AllAcc = accs,
                AllType = Settings.TypeApp,
                AllKindPass = new List<string> { AppResources.PassMan, AppResources.PassMotorcycle,
                    AppResources.PassCar, AppResources.PassGazele, AppResources.PassCargo },
                AllBrand = new List<string>() {"Suzuki", "Kavasaki", "Lada", "Opel", "Volkswagen", "Запорожец" }, /*Settings.BrandCar,*/
                hex = (Color)Application.Current.Resources["MainColor"],
                SelectedAcc = accs[0],
                SelectedType = null /*Settings.TypeApp[0]*/,
                Files = files,
                LabelTakeDateTime = LabelTakeDateTime
            };
#else
_appModel = new AddAppModel()
            {
                AllAcc = accs,
                AllType = Settings.TypeApp,
                AllKindPass = new List<string>{AppResources.PassMan, AppResources.PassMotorcycle,
                    AppResources.PassCar, AppResources.PassGazele, AppResources.PassCargo},
                AllBrand = Settings.BrandCar,
                hex = (Color)Application.Current.Resources["MainColor"],
                SelectedAcc = accs[0],
                SelectedType = null /*Settings.TypeApp[0]*/,
                Files = files
            };
#endif
                        
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var account in accs)
                    {
                        _appModel.Accounts.Add(account);
                    }
                    _appModel.SelectedAccount = accs[0];
                });
            

            BindingContext = _appModel;
            ListViewFiles.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));

            var passTypeEntryTGR = new TapGestureRecognizer();
            passTypeEntryTGR.Tapped += async (s, e) =>
            {
                if (!PassTypesList.IsVisible)
                {
                    PassTypesList.IsVisible = true;
                }
            };
            PassType.GestureRecognizers.Add(call);
            MessagingCenter.Subscribe<Object, string>(this, "SetVisibleLayout", (sender,name) =>
            {
                // if (name.Contains("пропуск"))
                // {
                //     SetPassApp();
                //     SaveText = EntryMess.Text;
                //     isPassAPP = true;
                // }
                // else
                // {
                //     SetDefaultApp();
                //     isPassAPP = false;
                // }
            });
            if (isPassAPP)
            {
                SetPassApp();
            }
        }

        

        
        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            PassTypesList.IsVisible = true;
            PassTypesList.BeginRefresh();

            try
            {
                var dataEmpty = _appModel.AllKindPass.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
                
                SetVisibleLayout(e.NewTextValue);

                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                {
                    //PassTypesList.IsVisible = false;
                    PassTypesList.ItemsSource = _appModel.AllKindPass;
                }
                else if (dataEmpty.Max().Length == 0)
                    PassTypesList.IsVisible = false;
                else
                    PassTypesList.ItemsSource = _appModel.AllKindPass.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
                PassTypesList.IsVisible = false;
            }
            PassTypesList.EndRefresh();

        }

        private void TSBrand_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TSBrandList.IsVisible = true;
            TSBrandList.BeginRefresh();

            try
            {
                var dataEmpty = _appModel.AllBrand.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
                
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                {
                    //TSBrandList.IsVisible = false;
                    TSBrandList.ItemsSource = _appModel.AllBrand;
                }
                else if (dataEmpty.Max().Length == 0)
                    TSBrandList.IsVisible = false;
                else
                    TSBrandList.ItemsSource = _appModel.AllBrand.Where(i => i.ToLower().Contains(e.NewTextValue.ToLower()));
            }
            catch (Exception ex)
            {
                TSBrandList.IsVisible = false;
            }
            TSBrandList.EndRefresh();
        }

        void SetVisibleLayout(string listsd)
        {
            if (listsd != null)
            {
                if (!_appModel.AllKindPass.Exists(i => i.ToLower() == listsd.ToLower()))
                {
                    LayoutPeshehod.IsVisible = false;
                    LayoutAvto.IsVisible = false;
                    return;
                }
                _passApp.idType = _appModel.AllKindPass.IndexOf(listsd) + 1;
                // User selected an item from the suggestion list, take an action on it here.
                if (listsd.Equals(AppResources.PassMan))
                {
                    LayoutPeshehod.IsVisible = true;
                    LayoutAvto.IsVisible = false;

                }
                else
                {
                    LayoutPeshehod.IsVisible = false;
                    LayoutAvto.IsVisible = true;
                }
            }
            else
            {
                // User hit Enter from the search box. Use args.QueryText to determine what to do.
                _passApp.idType = 0;
            }
        }

        private void ListView_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            String listsd = e.Item as string;
            PassType.Text = listsd;
            PassTypesList.IsVisible = false;
            ((ListView)sender).SelectedItem = null;            
            SetVisibleLayout(listsd);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(TimeSpan.FromSeconds(1));           
        }

        private async void AddFile()
        {

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
                catch
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
            try
            {
                        if (action == TAKE_PHOTO)
                        {
                            await getCameraFile();
                            return;
                        }
                        if (action == TAKE_GALRY)
                        {
                            await GetGalaryFile();
                            return;
                        }
                        if (action == TAKE_FILE)
                        {
                            await PickAndShowFile(null);
                            return;
                        }
            }
            catch (Exception ex)
            {
                
            }
        }
        async void ClosePage()
        {
            try
            {
                await Navigation.PopAsync();
               
            }
            catch
            {
                await Navigation.PopModalAsync();
            }
        }
        private async void PickImage_Clicked(object sender, EventArgs args)
        {
            string[] fileTypes = null;

            if (Device.RuntimePlatform == Device.Android)
            {
                fileTypes = new string[] {"image/png", "image/jpeg"};
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                fileTypes = new string[] {"public.image"}; // same as iOS constant UTType.Image
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                fileTypes = new string[] {".jpg", ".png"};
            }

            if (Device.RuntimePlatform == Device.WPF)
            {
                fileTypes = new string[] {"JPEG files (*.jpg)|*.jpg", "PNG files (*.png)|*.png"};
            }

            await PickAndShowFile(fileTypes);
        }

        private async Task PickAndShowFile(string[] fileTypes)
        {
            try
            {
                FileData pickedFile = await CrossFilePicker.Current.PickFile(fileTypes);

                if (pickedFile != null)
                {
                    if (pickedFile.DataArray.Length > 10000000)
                    {
                        await DisplayAlert(AppResources.ErrorTitle,AppResources.FileTooBig, "OK");
                        return;
                    }

                    files.Add(pickedFile);
                    Byteses.Add(pickedFile.DataArray);
                    ListViewFiles.IsVisible = true;
                    if (ListViewFiles.HeightRequest < 120)
                        ListViewFiles.HeightRequest = _appModel.Files.Count * 42;
                    _appModel.Files = files;
                    ListViewFiles.ItemsSource = _appModel.Files;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, ex.ToString(), "OK");
            }
        }

        async Task getCameraFile()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCameraNotAvailable, "OK");

                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    CompressionQuality =  90,
                    Directory = Xamarin.Essentials.AppInfo.Name.Replace("\"", "")
                });

            if (file == null)
                return;
            FileData fileData = new FileData( file.Path,getFileName(file.Path), () => file.GetStream() );
            Byteses.Add(StreamToByteArray(file.GetStream()));
            files.Add(fileData);
            ListViewFiles.IsVisible = true;
            if (ListViewFiles.HeightRequest < 120)
                ListViewFiles.HeightRequest = _appModel.Files.Count * 42;
            _appModel.Files = files;
            ListViewFiles.ItemsSource = _appModel.Files;
        }
        
        async Task GetGalaryFile()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorGalleryNotAvailable, "OK");

                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;
            FileData fileData = new FileData( file.Path,getFileName(file.Path), () => file.GetStream() );
            Byteses.Add(StreamToByteArray(file.GetStream()));
            files.Add(fileData);
            ListViewFiles.IsVisible = true;
            if (ListViewFiles.HeightRequest < 120)
                ListViewFiles.HeightRequest = _appModel.Files.Count * 42;
            _appModel.Files = files;
            ListViewFiles.ItemsSource = _appModel.Files;
        }
        
        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream) stream).ToArray();
            }
            else
            {
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

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
           
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        

        public class AddAppModel:BaseViewModel
        {
            public List<AccountInfo> AllAcc { get; set; }
            public List<RequestType> AllType { get; set; }
            public List<string> AllBrand { get; set; }
            public List<string> AllKindPass { get; set; }
            public AccountInfo SelectedAcc { get; set; }
            public NamedValue SelectedType { get; set; }
            public ObservableCollection<AccountInfo> Accounts { get; set; }
            AccountInfo selectedAccount;
            bool isVisible;
            public bool IsVisible
            {
                get => isVisible;
                set
                {
                    isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
            public ObservableCollection<OptionModel> Types { get; set; }
            public ObservableCollection<TypeModel> PodTypes { get; set; }  
            OptionModel selectedTyp;
            public OptionModel SelectedTyp
            {
                get => selectedTyp;
                set
                {
                    selectedTyp = value;
                    OnPropertyChanged("SelectedTyp");
                }
            }
            TypeModel _podTypSelected;
            private Label _labelTakeDateTime;

            public TypeModel PodTypSelected
            {
                get => _podTypSelected;
                set
                {
                    _podTypSelected = value;
                    OnPropertyChanged("PodTypSelected");
                }
            }
            public AccountInfo SelectedAccount
            {
                get => selectedAccount;
                set
                {
                    selectedAccount = value;
                    OnPropertyChanged("SelectedAccount");
                }
            }

            public ObservableCollection<FileData> Files { get; set; }
            public Color hex { get; set; }
            public Command SelectTyp { get; set; }
            public Command SelectDate { get; set; }
            public Command PodTypeSelect { get; set; }
            public Command SelectAccount { get; set; }
            public string DateValidity { get; set; }
            public Label LabelTakeDateTime
            {
                get => _labelTakeDateTime;
                set => _labelTakeDateTime = value;
            }

            public AddAppModel()
            {
                Accounts = new ObservableCollection<AccountInfo>();
                //AllAcc = Settings.Person.Accounts;
                Types = new ObservableCollection<OptionModel>();
                PodTypes = new ObservableCollection<TypeModel>();
                //foreach (var account in AllAcc)
                //{
                //    Device.BeginInvokeOnMainThread(() =>
                //    {
                //        Accounts.Add(account);
                //        SelectedAccount = Accounts[0];
                //    });
                //}

                foreach (var type in Settings.TypeApp)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        OptionModel type_ = new OptionModel();
                        type_.Name = type.Name;
                        type_.HasSubTypes = type.HasSubTypes;
                        type_.SubTypes = type.SubTypes;
                        type_.ID = type.ID.ToString();
                        String image = "";
                        type_.ReplaceMap = SetIconType(type.Name, ref image);
                        type_.Image = image;
                        Types.Add(type_);
                        SelectedTyp = null /*Types[0]*/;
                    });
                }
                
               
                
                PodTypeSelect = new Command<object>(name =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var selected = new TypeModel();
                        selected = PodTypes.FirstOrDefault(x => x.Name == PodTypSelected?.Name);
                        if (selected != null)
                        {
                            foreach (var typ in PodTypes)
                            {
                                typ.Selected = false;
                                string replaceColor = Application.Current.RequestedTheme == OSAppTheme.Dark ? "#FFFFFF" : "#8D8D8D";
                                typ.ReplaceMap = new Dictionary<string, string> { { "#000000", replaceColor } };
                            }
                            selected.Selected = true;
                            selected.ReplaceMap = new Dictionary<string, string> { { "#000000", "#" + Settings.MobileSettings.color } };
                        }
                    });
                });
                
                SelectTyp = new Command<object>(name =>
                {

                    Device.BeginInvokeOnMainThread(() =>
                    {
                       
                        if (SelectedTyp != null)
                        {
                            IsVisible = SelectedTyp.HasSubTypes;
                            if (!IsVisible)
                            {
                                PodTypSelected = null;
                            }
                            Device.BeginInvokeOnMainThread(() => { PodTypes.Clear(); });
                            
                            foreach (var type in SelectedTyp.SubTypes)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    TypeModel type_ = new TypeModel();
                                    type_.Name = type.Name;
                                    String image = "";
                                    type_.ID = type.ID;
                                    PodTypes.Add(type_);
                                    PodTypSelected = null;
                                });
                            }
                            
                        }
                        
                    });
                    
                });

                SelectAccount = new Command<string>((name) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (SelectedAccount != null)
                        {
                            foreach (var account in Accounts)
                            {
                                account.Selected = false;
                            }
                            SelectedAccount.Selected = true;
                        }
                    });
                });
                
                SelectDate = new Command<Tuple<string,string>>((date) =>
                {
                    DateValidity = date.Item1;
                    LabelTakeDateTime.Text = date.Item2;
                    LabelTakeDateTime.TextColor = Color.Gray;
                });
            }

            private static Dictionary<string, string> SetIconType(String Name, ref string Image)
            {
                switch (Name.ToLower())
                {
                    case "бухгалтерия":
                        Image = "resource://xamarinJKH.Resources.app_accountaint.svg";
                        break;
                    case "паспортный стол":
                        Image = "resource://xamarinJKH.Resources.app_passport.svg"; //vector 3
                        break;
                    case "сантехник":
                        Image = "resource://xamarinJKH.Resources.app_plumber.svg"; //vector 1
                        break;
                    case "электрик":
                        Image = "resource://xamarinJKH.Resources.app_electritian.svg"; //vector 2
                        break;
                    case "другие вопросы":
                        Image = "resource://xamarinJKH.Resources.app_other.svg"; //vector 5
                        break;
                    case "домофон":
                        Image = "resource://xamarinJKH.Resources.app_domophone.svg";
                        break;
                    case "заявка на пропуск":
                        Image = "resource://xamarinJKH.Resources.app_pass.svg"; //vector 4
                        break;
                }

                string replaceColor = Application.Current.RequestedTheme == OSAppTheme.Dark ? "#FFFFFF" : "#8D8D8D";
                return new Dictionary<string, string> {{"#000000", replaceColor}};
            }
        }

        private bool PassIsConstant = true;
        private async void addApp(object sender, EventArgs e)
        {
            string text = EntryMess.Text;
            FrameBtnAdd.IsVisible = false;
            progress.IsVisible = true;
          
            if (GetEnabledAdd(text))
            {
                try
                {
                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                        return;
                    }
                    

                    var vm = (BindingContext as AddAppModel);

                    if (vm.SelectedTyp == null && !isPassAPP)
                    {
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.AppTypeNotSelected, "OK"));
                        return;
                    }

                    var index = vm.Accounts.IndexOf(vm.SelectedAccount);
                    var type_index = vm.Types.IndexOf(vm.SelectedTyp);
                    string ident = Settings.Person.Accounts[index].Ident;
                    string typeId = isPassAPP ? Settings.MobileSettings.requestTypeForPassRequest.ToString() : Settings.TypeApp[type_index].ID.ToString();
                    int? SubTypeID = _appModel.PodTypSelected?.ID;
                    string floor = Settings.MobileSettings.isRequiredFloor ? EntryFloor.Text.Replace(AppResources.Floor + " № ", "") :null;
                    string entrance = Settings.MobileSettings.isRequiredEntrance ? EntryEntrance.Text.Replace(AppResources.Entrance + " № ", "") :null;
                    text = isPassAPP ? AppResources.NamePassApp : text;
                    if (Settings.MobileSettings.isRequiredFloor && !isPassAPP)
                    {
                        if (string.IsNullOrWhiteSpace(floor))
                        {
                            await DisplayAlert(AppResources.ErrorTitle, AppResources.EnterFloorNumber, "OK");
                            return;
                        }
                    }

                    if (Settings.MobileSettings.isRequiredEntrance && !isPassAPP)
                    {
                        if (string.IsNullOrWhiteSpace(entrance))
                        {
                            await DisplayAlert(AppResources.ErrorTitle, AppResources.EnterEntranceNumber, "OK");
                            return;
                        }
                    }
                    IDResult result = new IDResult();
                    if (isPassAPP)
                    {
                        result = await _server.newAppPass(ident, typeId, text,_passApp.idType,PassIsConstant, _appModel.DateValidity, _passApp.Fio,
                            _passApp.SeriaNumber, _passApp.CarBrand, _passApp.CarNumber);
                    }
                    else
                    {
                        
                        result = await _server.newApp(ident, typeId, text,SubTypeID, floor, entrance);

                    }
                    var update = await _server.GetRequestsUpdates(Settings.UpdateKey, result.ID.ToString());
                    Settings.UpdateKey = update.NewUpdateKey;
                    
                    
                    if (result.Error == null)
                    {
                        sendFiles(result.ID.ToString());
                        await DisplayAlert(AppResources.AlertSuccess, AppResources.AppCreated, "OK");
                       ClosePage();
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    MessagingCenter.Send<Object>(this, "UpdateIdent");
                    MessagingCenter.Send<Object>(this, "UpdateEvents");
                    MessagingCenter.Send<Object>(this, "AutoUpdate");
                }
                catch (Exception ex)
                {
                    // ignored
                }
                finally
                {
                    FrameBtnAdd.IsVisible = true;
                    progress.IsVisible = false;
                }
            }

            FrameBtnAdd.IsVisible = true;
            progress.IsVisible = false;
        }

        bool GetEnabledAdd(string text)
        {
            if (isPassAPP)
            {

                if (_passApp.idType != 0)
                {
                    
                    if (_passApp.idType == 1)
                    {
                        _passApp.Fio = EntryFIO.Text;
                        _passApp.SeriaNumber = EntryPassport.Text;
                        _passApp.CarBrand = null;
                        _passApp.CarNumber = null;
                        if (string.IsNullOrWhiteSpace(_passApp.Fio))
                        {
                            DisplayAlert(AppResources.ErrorTitle, AppResources.EnterFIO, "OK");
                            return false;
                        }


                        return true;

                    }
                    else
                    {
                        _passApp.Fio = null;
                        _passApp.SeriaNumber = null;
                        _passApp.CarNumber = EntryNumber.Text;
                        if (string.IsNullOrWhiteSpace(_passApp.CarBrand))
                        {
                            DisplayAlert(AppResources.ErrorTitle, AppResources.EnterCarBrand, "OK");
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(_passApp.CarNumber))
                        {
                            DisplayAlert(AppResources.ErrorTitle, AppResources.EnterStateNumber, "OK");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                        // else
                        // {
                        //     if (!CheckBoxInNumber.IsChecked)
                        //     {
                        //         Regex regexNumberAvto = new Regex(@"^[А-Я]{1}[0-9]{3}[А-Я]{2}[0-9]{2,3}$");
                        //         Regex regexNumberAvto2 = new Regex(@"[А-Я]{2}[0-9]{3}[0-9]{2,3}$");
                        //
                        //         if (regexNumberAvto.IsMatch(_passApp.CarNumber.Replace(" ","")) ||
                        //             regexNumberAvto2.IsMatch(_passApp.CarNumber.Replace(" ","")))
                        //         {
                        //             return true;
                        //         }
                        //         else
                        //         {
                        //             DisplayAlert(AppResources.ErrorTitle,
                        //                 AppResources.EnterStateNumber + " " + AppResources.MaskNumberCar + ":\n" +
                        //                 "А 234 АА 12, А 234 АА 123, АА 234 22, АА 234 123", "OK");
                        //         }
                        //     }
                        //     else
                        //     {
                        //         return true;
                        //     }
                        // }
                    }
                }
                else
                {
                    DisplayAlert(AppResources.ErrorTitle, AppResources.EnterTypePass, "OK");
                }
                
                return false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    DisplayAlert(AppResources.ErrorTitle, AppResources.AppErrorFill, "OK");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        
        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            FileData select = e.Item as FileData;
            bool answer = await DisplayAlert(AppResources.Delete, AppResources.DeleteFile, AppResources.Yes, AppResources.No);
            if (answer)
            {
                int indexOf = files.IndexOf(@select);
                Byteses.RemoveAt(indexOf);
                files.RemoveAt(indexOf);
                _appModel.Files = files;
                ListViewFiles.ItemsSource = _appModel.Files;
                ListViewFiles.HeightRequest = _appModel.Files.Count * 42;
                if (files.Count == 0)
                {
                    ListViewFiles.IsVisible = false;
                }
            }
        }


        async void sendFiles(string id)
        {
            int i = 0; 
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            foreach (var each in files)
            {
                CommonResult commonResult = await _server.AddFileApps(id, each.FileName, Byteses[i],
                    each.FilePath);
                i++;
            }
        }

        void SetPassApp()
        {
            FrameEntryMess.IsVisible = false;
            LayoutPassApp.IsVisible = true;
            LayoutFloor.IsVisible = false;
            LayoutSetType.IsVisible = false;
        } 
        void SetPassApp2()
        {
            FrameEntryMess.IsVisible = false;
            LayoutPassApp.IsVisible = true;
            LayoutFloor.IsVisible = false;
        }
        
        void SetDefaultApp()
        {
            FrameEntryMess.IsVisible = true;
            LayoutPassApp.IsVisible = false;
            LayoutSetType.IsVisible = true;
            LayoutFloor.IsVisible = true;
        }

        string SaveText { get; set; }
        
        private void pickerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _appModel.SelectTyp.Execute(null);
            if (_appModel.SelectedTyp.ID.Equals(Settings.MobileSettings.requestTypeForPassRequest.ToString()))
            {
               
                isPassAPP = true;
                SetPassApp2();
            }
            else
            {
                SetDefaultApp();
                // EntryMess.Text = "";
                isPassAPP = false;
            }
        }


        private async void EntryNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // string entryNumberText = EntryNumber.Text;
            // Regex regexNumberAvto = new Regex(@"^[А-Я]{1}[0-9]{3}[А-Я]{2}[0-9]{2,3}$");
            // Regex regexNumberAvto2 = new Regex(@"[А-Я]{2}[0-9]{3}[0-9]{2,3}$");
            // string result = entryNumberText;
            // if (!CheckBoxInNumber.IsChecked)
            // {
            //     if (regexNumberAvto.IsMatch(entryNumberText.Replace(" ","")))
            //     {
            //         result = entryNumberText.Insert(1, " ").Insert(5, " ").Insert(8, " ");
            //         EntryNumber.Text = result;
            //         EntryNumber.MaxLength = 12;
            //         LabelError.BackgroundColor = Color.Transparent;
            //
            //     }
            //     else if (regexNumberAvto2.IsMatch(entryNumberText.Replace(" ","")))
            //     {
            //         result = entryNumberText.Insert(2, " ").Insert(6, " ");
            //         EntryNumber.Text = result;
            //         EntryNumber.MaxLength = 10;
            //         LabelError.BackgroundColor = Color.Transparent;
            //     }
            //     else
            //     {
            //         LabelError.BackgroundColor = Color.Red;
            //     }
            // }
        }
        


        class PassApp
        {
            public int idType { get; set; } = 0;
            public string CarBrand { get; set;}
            public string CarNumber { get; set;}
            public string Fio { get; set; }
            public string SeriaNumber { get; set; }
        }

        private void TSBrandList_OnItemTapped(Object sender, ItemTappedEventArgs e)
        {
            String listsd = e.Item as string;
            TSBrand.Text = listsd;
            TSBrandList.IsVisible = false;
            ((ListView)sender).SelectedItem = null;
            _passApp.CarBrand = listsd;
        }

        private void EntryPassport_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { 
            if (EntryPassport.Text.Contains(","))
            {
                EntryPassport.Text = EntryPassport.Text.Replace(",", "");
            }
            });
    }

        private void PassType_Unfocused(object sender, FocusEventArgs e)
        {
            PassTypesList.IsVisible = false;
        }

        private void PassType_Focused(object sender, FocusEventArgs e)
        {
            PassTypesList.IsVisible = true;
        }
                
        private void TSBrand_Unfocused(object sender, FocusEventArgs e)
        {
            TSBrandList.IsVisible = false;
        }

        private void TSBrand_Focused(object sender, FocusEventArgs e)
        {
            TSBrandList.IsVisible = true;
        }

        StackLayout lastElementSelected;
        

        private void FrameIdentGR_Tapped_1(object sender, EventArgs e)
        {
            if (lastElementSelected != null)
            {
                VisualStateManager.GoToState(lastElementSelected.Children[0], "Normal");
            }

            var el = sender as StackLayout;

            VisualStateManager.GoToState(el.Children[0], "Selected");          
            
            var acc = el.BindingContext as AccountInfo;
            foreach (var account in (BindingContext as AddAppModel).Accounts)
            {
                account.Selected = false;
            }
            acc.Selected = true;
            var vm = (BindingContext as AddAppModel);
            vm.SelectedAccount = acc;
            lastElementSelected = (StackLayout)sender;
        }


        StackLayout lastElementSelected2;
        private void FrameIdentGR_Tapped(object sender, EventArgs e)
        {
            if (lastElementSelected2 != null)
            {
                VisualStateManager.GoToState(lastElementSelected2.Children[0], "Normal");
            }

            var el = sender as StackLayout;

            VisualStateManager.GoToState(el.Children[0], "Selected");

            var om = el.BindingContext as OptionModel;
            foreach (var option in (BindingContext as AddAppModel).Types)
            {
                option.Selected = false;
            }
            om.Selected = true;
            var vm = (BindingContext as AddAppModel);
            vm.SelectedTyp = om;
            lastElementSelected2 = (StackLayout)sender;
        }
        
        private void FrameIdentGR2_Tapped(object sender, EventArgs e)
        {
            if (lastElementSelected2 != null)
            {
                VisualStateManager.GoToState(lastElementSelected2.Children[0], "Normal");
            }

            var el = sender as StackLayout;

            VisualStateManager.GoToState(el.Children[0], "Selected");

            var om = el.BindingContext as TypeModel;
            foreach (var option in (BindingContext as AddAppModel).PodTypes)
            {
                option.Selected = false;
            }
            om.Selected = true;
            var vm = (BindingContext as AddAppModel);
            vm.PodTypSelected = om;
            lastElementSelected2 = (StackLayout)sender;
        }

        private void EntryMess_Focused(object sender, FocusEventArgs e)
        {
            //Scroll.IsEnabled = false;
        }

        private void EntryMess_Unfocused(object sender, FocusEventArgs e)
        {
            //Scroll.IsEnabled = true;
        }
        

        private void PickerPodType_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void CheckBoxInNumber_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (CheckBoxInNumber.IsChecked)
            {
                MaskAvtoNumber.ColorError = Color.Black;
            }
            else
            {
                Regex regexNumberAvto = new Regex(@"^[А-Я]{1}[0-9]{3}[А-Я]{2}[0-9]{2,3}$");
                Regex regexNumberAvto2 = new Regex(@"[А-Я]{2}[0-9]{3}[0-9]{2,3}$");
                var entryNumberText = EntryNumber.Text.Replace(" ","");
                if (regexNumberAvto.IsMatch(entryNumberText) || regexNumberAvto2.IsMatch(entryNumberText))
                {
                    MaskAvtoNumber.ColorError = Color.Black;
                }
                else
                {
                    MaskAvtoNumber.ColorError = Color.Red;
                }
            }
        }

        private void ButtonConstantPass_OnClicked(object sender, EventArgs e)
        {
            Color currentResource = (Color)Application.Current.Resources["MainColor"];
            FrameConstantPass.BorderColor =  currentResource;
            ButtonConstantPass.TextColor =  currentResource;
            PassIsConstant = true;
            FrameOneOffPass.BorderColor = Color.Gray;
            ButtonOneOffPass.TextColor = Color.Gray;
        }

        private void ButtonOneOffPass_OnClicked(object sender, EventArgs e)
        {
            Color currentResource = (Color)Application.Current.Resources["MainColor"];
            FrameConstantPass.BorderColor =  Color.Gray;
            ButtonConstantPass.TextColor =  Color.Gray;
            PassIsConstant = false;
            FrameOneOffPass.BorderColor = currentResource;
            ButtonOneOffPass.TextColor = currentResource;
        }
    }
}