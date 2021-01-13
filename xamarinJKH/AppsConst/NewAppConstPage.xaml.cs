using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.MainConst;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace xamarinJKH.AppsConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewAppConstPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        public Command SetLs { get; set; }
        public List<FileData> files { get; set; }
        public List<byte[]> Byteses = new List<byte[]>();
        private AppsConstPage _appsPage;
        string TAKE_PHOTO = AppResources.AttachmentTakePhoto;
        string TAKE_GALRY = AppResources.AttachmentChoosePhoto;
        string TAKE_FILE = AppResources.AttachmentChooseFile;
        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";
        public int PikerLsItem = 0;
        public int PikerTypeItem = 0;

        public int CreateType { get; set; }

        int? District;
        string Street;
        int? House;
        int? Flat;

        public bool ShowArea { get => Settings.MobileSettings.districtsExists; }
        public bool ShowStreets { get => Settings.MobileSettings.streetsExists; }
        public bool ShowHouses { get => Settings.MobileSettings.housesExists; }
        public bool ShowPremises { get => Settings.MobileSettings.premisesExists; }

        public NamedValue selectedDistrict;
        public NamedValue selectedHouse;
        public NamedValue selectedFlat;

        private async void TechSend(object sender, EventArgs e)
        {

            // await PopupNavigation.Instance.PushAsync(new TechDialog(false));
            if (Settings.Person != null && !string.IsNullOrWhiteSpace(Settings.Person.Phone))
            {
                await Navigation.PushModalAsync(new Tech.AppPage());
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
            }
        }

        public NewAppConstPage(AppsConstPage appsPage)
        {
            _appsPage = appsPage;
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //BackgroundColor = Color.White;
                    // ImageTop.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    // if (Application.Current.MainPage.Height > 800)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -180);
                    //     BackStackLayout.Margin = new Thickness(-5, 35, 0, 0);
                    // }
                    break;
                case Device.Android:
                    // ScrollViewContainer.Margin = new Thickness(0, 0, 0, -162);
                    // double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -115);
                    //     BackStackLayout.Margin = new Thickness(-5, 15, 0, 0);
                    // }

                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfileConstPage) == null)
                    await Navigation.PushAsync(new ProfileConstPage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { AddFile(); };
            StackLayoutAddFile.GestureRecognizers.Add(addFile);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += TechSend; //async(s, e) => { await PopupNavigation.Instance.PushAsync(new TechDialog()); };
            LabelTech.GestureRecognizers.Add(techSend);

            var delLS = new TapGestureRecognizer();
            delLS.Tapped += async (s, e) =>
            {
                EntryLS.IsVisible = false;
                LabelLs.IsVisible = true;
                EntryLS.Text = "";
                LabelLs.Text = "Нажмите для выбора";
                IconViewClose.IsVisible = false;
            };

            var setLss = new TapGestureRecognizer();
            if (Settings.MobileSettings.chooseIdentByHouse)
            {
                setLss.Tapped += async (s, e) =>
                {
                    await PopupNavigation.Instance.PushAsync(
                        new SetLsConstDialog());
                };
                LabelLs.GestureRecognizers.Add(setLss);
                EntryLS.IsVisible = false;
                LabelLs.IsVisible = true;
                (IconViewClose.Parent as View).GestureRecognizers.Add(delLS);
            }
            else
            {
                StackLayoutHouses.IsVisible = false;
            }

            SetText();
            files = new List<FileData>();
            if (Settings.TypeApp == null)
            {
                Console.WriteLine("Отсутсвуют типы заявок");
            }
            else
            {
                BindingContext = new AddAppConstModel()
                {
                    AllType = Settings.TypeApp,
                    hex = (Color)Application.Current.Resources["MainColor"],
                    SelectedType = Settings.TypeApp[0],
                    Files = files
                };
            }


            MessagingCenter.Subscribe<Object, string>(this, "SetLs", async (sender, args) =>
            {
                LabelLs.Text = args;
                EntryLS.Text = args;
                IconViewClose.IsVisible = true;
            });

            MessagingCenter.Subscribe<Object, Tuple<int?, int?, int?, string>>(this, "SetTypes", (sender, data) =>
            {
                this.District = data.Item1;
                this.House = data.Item2;
                this.Flat = data.Item3;
                this.Street = data.Item4;
            });

            MessagingCenter.Subscribe<Object, Tuple<NamedValue, NamedValue, NamedValue>>(this, "SetNames", (sender, data) =>
            {
                selectedDistrict = data.Item1;
                selectedHouse = data.Item2;
                selectedFlat = data.Item3;
            });

            ((TypeStack.Children[0] as StackLayout).Children[0] as RadioButton).IsChecked = true;

            foreach (StackLayout option in TypeStack.Children)
            {
                var Tapped = new TapGestureRecognizer();
                Tapped.Tapped += (s, e) =>
                {
                    try
                    {
                        (((s as View).Parent as StackLayout).Children[0] as RadioButton).IsChecked = true;
                        var index = Convert.ToInt32((((s as View).Parent as StackLayout).Children[0] as RadioButton).ClassId);

                        (BindingContext as AddAppConstModel).Ident = index == 0;
                        this.CreateType = index;

                    }
                    catch { }
                };
                (option.Children[1] as Label).GestureRecognizers.Add(Tapped);
            }
        }

        private async void AddFile()
        {
            if (Device.RuntimePlatform == "Android")
            {
                try
                {
                    var camera_perm =
                        await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                    var storage_perm =
                        await Plugin.Permissions.CrossPermissions.Current
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
                            Plugin.Permissions.CrossPermissions.Current.OpenAppSettings();
                    });
                    return;
                }
            }

            var action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
                TAKE_PHOTO,
                TAKE_GALRY, TAKE_FILE);
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
            }
        }

        private async void PickImage_Clicked(object sender, EventArgs args)
        {
            string[] fileTypes = null;

            if (Device.RuntimePlatform == Device.Android)
            {
                fileTypes = new string[] { "image/png", "image/jpeg" };
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                fileTypes = new string[] { "public.image" }; // same as iOS constant UTType.Image
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                fileTypes = new string[] { ".jpg", ".png" };
            }

            if (Device.RuntimePlatform == Device.WPF)
            {
                fileTypes = new string[] { "JPEG files (*.jpg)|*.jpg", "PNG files (*.png)|*.png" };
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
                    // UkName.Text = pickedFile.FileName;
                    // LabelPhone.Text = pickedFile.FilePath;
                    if (pickedFile.DataArray.Length > 10000000)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, AppResources.FileTooBig, "OK");
                        return;
                    }

                    files.Add(pickedFile);
                    Byteses.Add(pickedFile.DataArray);
                    ListViewFiles.IsVisible = true;
                    if (ListViewFiles.HeightRequest < 120)
                        ListViewFiles.HeightRequest += 30;
                    setBinding();
                    // if (pickedFile.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase)
                    //     || pickedFile.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    // {
                    //     IconViewNameUk.Source = ImageSource.FromStream(() => pickedFile.GetStream());
                    //     IconViewNameUk.IsVisible = true;
                    // }
                    // else
                    // {
                    //     IconViewNameUk.IsVisible = false;
                    // }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, ex.ToString(), "OK");
            }
        }

        async Task getCameraFile()
        {
            try
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
                        Directory = "Demo"
                    });

                if (file == null)
                    return;
                FileData fileData = new FileData(file.Path, getFileName(file.Path), () => file.GetStream());
                Byteses.Add(StreamToByteArray(file.GetStream()));
                files.Add(fileData);
                ListViewFiles.IsVisible = true;
                if (ListViewFiles.HeightRequest < 120)
                    ListViewFiles.HeightRequest += 30;
                setBinding();
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, $"{ex.Message}\n{ex.StackTrace}", "ОК");
            }
        }

        async Task GetGalaryFile()
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCameraNotAvailable, "OK");

                    return;
                }

                var file = await CrossMedia.Current.PickPhotoAsync();
                if (file == null)
                    return;
                FileData fileData = new FileData(file.Path, getFileName(file.Path), () => file.GetStream());
                Byteses.Add(StreamToByteArray(file.GetStream()));
                files.Add(fileData);
                ListViewFiles.IsVisible = true;
                if (ListViewFiles.HeightRequest < 120)
                    ListViewFiles.HeightRequest += 30;
                setBinding();
                //PickerLs.SelectedIndex = PikerLsItem;
                PickerType.SelectedIndex = PikerTypeItem;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, $"{ex.Message}\n{ex.StackTrace}", "ОК");
            }
        }

        public async Task startLoadFile(string metod)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.LoadingFile,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                switch (metod)
                {
                    case CAMERA:
                        await getCameraFile();
                        break;
                    case GALERY:
                        await GetGalaryFile();
                        break;
                    case FILE:
                        await PickAndShowFile(null);
                        break;
                }
            });
        }

        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
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
            FormattedString formattedName = new FormattedString();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                currentTheme = OSAppTheme.Dark;
            formattedName.Spans.Add(new Span
            {
                Text = Settings.Person.FIO,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            });
            formattedName.Spans.Add(new Span
            {
                Text = AppResources.GoodDay,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.None,
                FontSize = 16
            });
            //LabelName.FormattedText = formattedName;
            Color hexColor = (Color)Application.Current.Resources["MainColor"];
            //IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            //Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent); if (Device.RuntimePlatform == Device.iOS) { if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo") { PancakeViewIcon.Padding = new Thickness(0); } }
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        void setBinding()
        {
            //PikerLsItem = PickerLs.SelectedIndex;
            PikerTypeItem = PickerType.SelectedIndex;
            try
            {
                BindingContext = null;
                BindingContext = new AddAppConstModel()
                {
                    AllType = Settings.TypeApp,
                    hex = (Color)Application.Current.Resources["MainColor"],
                    SelectedType = Settings.TypeApp[PikerTypeItem],
                    Files = files
                };
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // try
            // {
            //     var identLength = Settings.Person.Accounts[PickerLs.SelectedIndex].Ident.Length;
            //     if (identLength < 6)
            //     {
            //         PickerLs.WidthRequest = identLength * 9;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     // ignored
            // }
        }

        public class AddAppConstModel : xamarinJKH.ViewModels.BaseViewModel
        {
            public List<RequestType> AllType { get; set; }
            public NamedValue SelectedType { get; set; }

            public List<FileData> Files { get; set; }
            public Color hex { get; set; }
            public ObservableCollection<NamedValue> CreateTypes { get; set; }
            public AddAppConstModel()
            {
                Ident = true;
                CreateTypes = new ObservableCollection<NamedValue>();
                CreateTypes.Add(new NamedValue { Name = "Лицевому счету", ID = 1 });
                CreateTypes.Add(new NamedValue { Name = "Району" });
                CreateTypes.Add(new NamedValue { Name = "Улице" });
                CreateTypes.Add(new NamedValue { Name = "Дому" });
                CreateTypes.Add(new NamedValue { Name = "Квартире" });
            }

            bool _ident;
            public bool Ident
            {
                get => _ident;
                set
                {
                    _ident = value;
                    OnPropertyChanged("Ident");
                }
            }
        }

        private async void addApp(object sender, EventArgs e)
        {
            string text = EntryMess.Text;
            FrameBtnAdd.IsVisible = false;
            progress.IsVisible = true;
            string ident = EntryLS.Text;

            
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            if (!(BindingContext as AddAppConstModel).Ident)
            {
                if ((this.District == null && this.CreateType == 1 || this.House == null && this.CreateType == 2 || this.Flat == null && (this.CreateType == 3 || this.CreateType == 4)) || string.IsNullOrEmpty(text))
                {
                    await DisplayAlert(AppResources.Error, AppResources.ErrorFills.Replace(':',' '), "OK");
                    progress.IsVisible = false;
                    FrameBtnAdd.IsVisible = true;
                    return;
                }
                try
                {
                    string typeId = Convert.ToInt32(Settings.TypeApp[PickerType.SelectedIndex].ID).ToString();
                    switch (CreateType)
                    {
                        case 1: House = null;
                            Flat = null;
                            break;
                        case 2: Flat = null;
                            District = null;
                            break;
                        case 3: District = null;
                            House = null;
                            break;
                    }
                    IDResult result = await _server.newAppConst(null, typeId, text, "", this.District, this.House, this.Flat, this.Street);
                    await _server.SetReadedFlag(result.ID, true);

                    if (result.Error == null)
                    {
                        sendFiles(result.ID.ToString());
                        await DisplayAlert(AppResources.AlertSuccess, AppResources.AppCreated, "OK");
                        try
                        {
                            _ = await Navigation.PopAsync();
                        }
                        catch { }
                    }
                    else
                    {
                        if (result.Error.Contains("Not"))
                        {
                            await DisplayAlert(AppResources.ErrorTitle, AppResources.IdentNotFound, "OK");
                        }
                        else
                            await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (ident.Equals("") && (BindingContext as AddAppConstModel).Ident)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorFillIdent, "OK");
                FrameBtnAdd.IsVisible = true;
                progress.IsVisible = false;
                return;
            }


            if (!text.Equals(""))
            {
                try
                {
                    string typeId = Convert.ToInt32(Settings.TypeApp[PickerType.SelectedIndex].ID).ToString();
                    IDResult result = await _server.newAppConst(ident, typeId, text);
                    await _server.SetReadedFlag(result.ID, true);

                    if (result.Error == null)
                    {
                        sendFiles(result.ID.ToString());
                        await DisplayAlert(AppResources.AlertSuccess, AppResources.AppCreated, "OK");
                        try
                        {
                            _ = await Navigation.PopAsync();
                        }
                        catch { }
                    }
                    else
                    {
                        if (result.Error.Contains("Not"))
                        {
                            await DisplayAlert(AppResources.ErrorTitle, AppResources.IdentNotFound, "OK");
                        }
                        else
                            await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.AppErrorFill, "OK");
            }

            FrameBtnAdd.IsVisible = true;
            progress.IsVisible = false;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            FileData select = e.Item as FileData;
            bool answer = await DisplayAlert(AppResources.Delete, AppResources.DeleteFile, AppResources.Yes,
                AppResources.No);
            if (answer)
            {
                int indexOf = files.IndexOf(@select);
                Byteses.RemoveAt(indexOf);
                files.RemoveAt(indexOf);
                ListViewFiles.HeightRequest -= 30;
                if (files.Count == 0)
                {
                    ListViewFiles.IsVisible = false;
                }

                setBinding();
            }
        }


        async void sendFiles(string id)
        {
            int i = 0;
            foreach (var each in files)
            {
                CommonResult commonResult = await _server.AddFileAppsConst(id, each.FileName, Byteses[i],
                    each.FilePath);
                i++;
            }
        }

        private void pickerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // try
            // {
            //     var identLength = Settings.TypeApp[PickerType.SelectedIndex].Name.Length;
            //     if (identLength < 6)
            //     {
            //         PickerType.WidthRequest = identLength * 10;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     // ignored
            // }
        }

        private void RadioButton_Focused(object sender, FocusEventArgs e)
        {

        }

        private void RadioButton_Pressed(object sender, EventArgs e)
        {
            var parent = (sender as View).Parent.Parent as StackLayout;
            var index = (parent as StackLayout).Children.IndexOf((sender as RadioButton).Parent as StackLayout);

            (BindingContext as AddAppConstModel).Ident = index == 0;
            this.CreateType = index;
        }

        private async void AddressApp(object sender, EventArgs e)
        {

            var select = TypeStack.Children.First(x => ((x as StackLayout).Children[0] as RadioButton).IsChecked);
            if (select != null)
            {
                var index = TypeStack.Children.IndexOf(select);
                await Navigation.PushAsync(new AddressSearch(index, new Tuple<NamedValue, NamedValue, NamedValue>(selectedDistrict, selectedHouse, selectedFlat)));

            }
        }

    }
}