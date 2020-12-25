using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Plugin.FilePicker.Abstractions;
using Rg.Plugins.Popup.Services;
using Syncfusion.SfAutoComplete.XForms;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using SelectionChangedEventArgs = Syncfusion.SfAutoComplete.XForms.SelectionChangedEventArgs;

namespace xamarinJKH.PushNotification
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendPushPage : ContentPage
    {
        public bool isRefresh { get; set; }
        public ObservableCollection<NamedValue> groups { get; set; }
        public ObservableCollection<HouseProfile> HouseProfiles { get; set; }

        private List<HouseProfile> DefaultHouses = new List<HouseProfile>();
        private List<HouseProfile> SelecttHouses = new List<HouseProfile>();

        public SendPushPage()
        {
            InitializeComponent();
            DatePicker2.MinimumDate = DatePicker.Date;
            Frame.SetAppThemeColor(PancakeView.BorderColorProperty,(Color)Application.Current.Resources["MainColor"],Color.Transparent );
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    //if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    //    mainStack.Padding = new Thickness(0, statusBarHeight * 2, 0, 0);
                    //else
                        mainStack.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    break;
                case Device.Android:
                    double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        //ScrollViewContainer.Margin = new Thickness(0, 0, 0, -150);
                    }

                    break;
                default:
                    break;
            }

            getGroups();
            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            UkName.Text = Settings.MobileSettings.main_name;
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) =>
            {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch
                {
                }
            };
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Settings.Person != null && !string.IsNullOrWhiteSpace(Settings.Person.Phone))
                {
                    await Navigation.PushModalAsync(new AppPage());
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync(new EnterPhoneDialog());
                }
            };
            LabelTech.GestureRecognizers.Add(techSend);

            Color hexColor = (Color)Application.Current.Resources["MainColor"];

            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);

            // var takeOS = new TapGestureRecognizer();
            //
            // takeOS.Tapped += async (s, e) =>
            // {
            //     var action = await DisplayActionSheet(AppResources.OsTake, AppResources.Cancel, null,
            //         "Android", "iOS", AppResources.All);
            //     if (action != null && !action.Equals(AppResources.Cancel))
            //     {
            //         _os = action;
            //         LabelKindOs.Text = action;
            //     }
            // };
            // StackLayoutOs.GestureRecognizers.Add(takeOS); 
            var takeDate = new TapGestureRecognizer();
            takeDate.Tapped += async (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => DatePicker.Focus());
            };
            StackLayoutDate.GestureRecognizers.Add(takeDate); 
            var takeDuty = new TapGestureRecognizer();
            takeDuty.Tapped += async (s, e) => ButtonDuty_OnClicked(FrameDuty, null);
            FrameDuty.GestureRecognizers.Add(takeDuty); 
            var takeRyon = new TapGestureRecognizer();
            takeRyon.Tapped += async (s, e) => ButtonRyon_OnClicked(FrameRyon, null);
            FrameRyon.GestureRecognizers.Add(takeRyon); 
            var takeIdent = new TapGestureRecognizer();
            takeIdent.Tapped += async (s, e) => ButtonIdent_OnClicked(FrameIdent, null);
            FrameIdent.GestureRecognizers.Add(takeIdent); 
            var takeHouse = new TapGestureRecognizer();
            takeHouse.Tapped += async (s, e) => ButtonHouse_OnClicked(FrameHouse, null);
            FrameHouse.GestureRecognizers.Add(takeHouse); 
            var takeDate2 = new TapGestureRecognizer();
            takeDate2.Tapped += async (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() => DatePicker2.Focus());
            };
            StackLayoutDate2.GestureRecognizers.Add(takeDate2);

            var kind = new TapGestureRecognizer();
            kind.Tapped += async (s, e) =>
            {
                var action = await DisplayActionSheet(AppResources.Takes, AppResources.Cancel, null,
                    AppResources.ToRayon, AppResources.ByHome, AppResources.ByLS, AppResources.ByDuty);
                if (action != null && !action.Equals(AppResources.Cancel))
                {
                    LabelKind.Text = action;

                    SetKind(action);
                }
            };
            StackLayoutKind.GestureRecognizers.Add(kind);
            BackStackLayout.GestureRecognizers.Add(backClick);
        }

        private void SetKind(string action)
        {
            if (action.Contains(AppResources.ToRayon))
            {
                LayoutRayon.IsVisible = true;
                LayoutHouses.IsVisible = false;
                LayoutLs.IsVisible = false;
                LayoutDebt.IsVisible = false;
                autoCompleteHouses.Clear();
                SelecttHouses.Clear();
                EntryLS.Text = "";
                EntryDebt.Text = "";
                autoCompleteHouses.EnableAutoSize = false;
                Device.BeginInvokeOnMainThread(() => autoComplete.Focus());
            }
            else if (action.Contains(AppResources.ByHome))
            {
                LayoutRayon.IsVisible = false;
                LayoutHouses.IsVisible = true;
                LayoutLs.IsVisible = false;
                LayoutDebt.IsVisible = false;
                autoComplete.Clear();
                _selectedGroupId = -1;
                EntryLS.Text = "";
                EntryDebt.Text = "";
                autoCompleteHouses.EnableAutoSize = true;
                Device.BeginInvokeOnMainThread(() => autoCompleteHouses.Focus());
            }
            else if (action.Contains(AppResources.ByLS))
            {
                LayoutLs.IsVisible = true;
                LayoutRayon.IsVisible = false;
                LayoutHouses.IsVisible = false;
                LayoutDebt.IsVisible = false;
                EntryLS.Focus();
                autoComplete.Clear();
                autoCompleteHouses.Clear();
                SelecttHouses.Clear();
                _selectedGroupId = -1;
                autoCompleteHouses.EnableAutoSize = false;
                EntryDebt.Text = "";
            }
            else if (action.Contains(AppResources.ByDuty))
            {
                LayoutDebt.IsVisible = true;
                LayoutLs.IsVisible = false;
                LayoutRayon.IsVisible = false;
                LayoutHouses.IsVisible = false;
                autoComplete.Clear();
                autoCompleteHouses.Clear();
                EntryDebt.Focus();
                SelecttHouses.Clear();
                autoCompleteHouses.EnableAutoSize = false;
                _selectedGroupId = -1;
                EntryLS.Text = "";
            }
        }

        private string _os = "";
        Thickness frameMargin = new Thickness();

        private void BordlessEditor_Focused(object sender, FocusEventArgs e)
        {
            if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                frameMargin = Frame.Margin;
                Device.BeginInvokeOnMainThread(() => { Frame.Margin = new Thickness(15, 0, 15, 15); });
            }
        }

        private RestClientMP _server = new RestClientMP();

        async void getGroups()
        {
            ItemsList<NamedValue> result = await _server.GetHouseGroups();
            ItemsList<HouseProfile> resultHouse = await _server.GetHouse();
            groups = new ObservableCollection<NamedValue>(result.Data);
            HouseProfiles = new ObservableCollection<HouseProfile>(resultHouse.Data.Where(x => x.Address != null));
            DefaultHouses = resultHouse.Data;
            BindingContext = this;
        }

        List<HouseProfile> DellNull(List<HouseProfile> profiles)
        {
            
            
            return profiles;
        }
        
        
        private void BordlessEditor_Unfocused(object sender, FocusEventArgs e)
        {
            if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            {
                Device.BeginInvokeOnMainThread(() => { Frame.Margin = frameMargin; });
            }
        }

        //private void CheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        //{
        //}

        private int _selectedGroupId = -1;

        private void AutoComplete_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NamedValue select = e.Value as NamedValue;
            if (@select != null)
            {
                _selectedGroupId = @select.ID;
            }
            else
            {
                _selectedGroupId = -1;
            }
        }

        private void AutoCompleteHouses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Value != null && e.RemovedItems == null)
            {
                SelecttHouses.Clear();
               
                foreach (HouseProfile each in (IEnumerable) e.Value)
                {
                    SelecttHouses.Add(each);
                }
            }
            else
            {
                HouseProfile rem = e.RemovedItems as HouseProfile;
                if (rem != null)
                {
                    SelecttHouses.Remove(rem);
                }
                else
                {
                    foreach (HouseProfile each in (IEnumerable) e.RemovedItems)
                    {
                        SelecttHouses.Remove(each);
                    }
                }

            }
        }

        private void AutoCompleteHouses_OnFocusChanged(object sender, FocusChangedEventArgs e)
        {
            // autoCompleteHouses.HeightRequest =  GetHeight(e.HasFocus);
        }

        private async void AddPush(object sender, EventArgs e)
        {
            string title = EntryTitle.Text;
            string text = BordlessEditor.Text;
            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.EnterName, "OK");
            }else if (string.IsNullOrWhiteSpace(text))
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.SfPdfViewerPlaceHolderText, "OK");
            }
            else
            {
                AnnouncementArguments announcementArguments = new AnnouncementArguments()
                {
                    Header = title,
                    Text = text,
                    ShowOnMainPage = CheckBox.IsToggled,
                    ActiveFrom = DatePicker.Date.ToString("dd.MM.yyyy"),
                    ActiveTo = DatePicker2.Date.ToString("dd.MM.yyyy")
                };
                if (!string.IsNullOrWhiteSpace(EntryLS.Text))
                {
                    announcementArguments.Ident = EntryLS.Text;
                }

                if (!string.IsNullOrWhiteSpace(EntryDebt.Text))
                {
                    try
                    {
                        decimal debt = Decimal.Parse(EntryDebt.Text);
                        announcementArguments.ForAccountsWithDebtOver = debt;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }

                if (SelecttHouses != null && SelecttHouses.Count > 0)
                {
                    announcementArguments.Houses = SelecttHouses.Select(each => each.ID).ToList();
                }

                if (_selectedGroupId != -1)
                {
                    announcementArguments.id_homegroup = _selectedGroupId;
                }

                if (announcementArguments.id_homegroup == null
                    && announcementArguments.Houses == null
                    && string.IsNullOrWhiteSpace(announcementArguments.Ident)
                    && announcementArguments.ForAccountsWithDebtOver == null)
                {
                    announcementArguments.Houses = DefaultHouses.Select(each => each.ID).ToList();
                }

                if (!string.IsNullOrWhiteSpace(_os) && !_os.Equals(AppResources.All))
                {
                    announcementArguments.OS = _os;
                }
               

                
                new Task(() =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Configurations.LoadingConfig = new LoadingConfig
                        {
                            IndicatorColor = (Color)Application.Current.Resources["MainColor"],
                            OverlayColor = Color.Black,
                            Opacity = 0.8,
                            DefaultMessage = AppResources.Loading,
                        };

                        await Loading.Instance.StartAsync(async progress =>
                        {
                            IDResult newAnnouncement = await _server.NewAnnouncement(announcementArguments);
                            if (newAnnouncement.Error == null)
                            {
                                CommonResult sendAnnouncement = await _server.SendAnnouncement(newAnnouncement.ID);
                                if (sendAnnouncement.Error != null)
                                {
                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        await DisplayAlert(AppResources.ErrorTitle, sendAnnouncement.Error, "OK");
                                    });
                                }
                                else
                                {
                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        await DisplayAlert("", AppResources.SendingPush, "OK");
                                    });
                                }
                            }
                            else
                            {
                                Device.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert(AppResources.ErrorTitle, newAnnouncement.Error, "OK");
                                });
                            }
                        });
                    });
                }).Start();
                
            }
        }
        
        
        private bool _isDateTo = false;
        private void DatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            DatePicker2.MinimumDate = DatePicker.Date;
        }

        private bool _isDateFrom = false;

        private void DatePicker2_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            DatePicker.MaximumDate = DatePicker2.Date;
        }

        Color hex = (Color) Application.Current.Resources["MainColor"];

        private void Button_All_OS_Click(object sender, EventArgs e)
        {
            _os = ButtonAll.Text;
            FrameAll.HasShadow = true;
            FrameAll.BorderColor = hex;
            ButtonAll.TextColor = hex;

            FrameAnd.HasShadow = false;
            FrameAnd.BorderColor = Color.Gray;
            ButtonAnd.TextColor = Color.Black;
            
            FrameIos.HasShadow = false;
            FrameIos.BorderColor = Color.Gray;
            ButtonIos.TextColor = Color.Black;
            
        }

        private void Button_Android_OS_Click(object sender, EventArgs e)
        {
            _os = ButtonAnd.Text;
            FrameAnd.HasShadow = true;
            FrameAnd.BorderColor = hex;
            ButtonAnd.TextColor = hex;

            FrameAll.HasShadow = false;
            FrameAll.BorderColor = Color.Gray;
            ButtonAll.TextColor = Color.Black;
            
            FrameIos.HasShadow = false;
            FrameIos.BorderColor = Color.Gray;
            ButtonIos.TextColor = Color.Black;
        }

        private void Button_IOS_Clcik(object sender, EventArgs e)
        {
            _os = ButtonIos.Text;
            FrameIos.HasShadow = true;
            FrameIos.BorderColor = hex;
            ButtonIos.TextColor = hex;

            FrameAll.HasShadow = false;
            FrameAll.BorderColor = Color.Gray;
            ButtonAll.TextColor = Color.Black;
            
            FrameAnd.HasShadow = false;
            FrameAnd.BorderColor = Color.Gray;
            ButtonAnd.TextColor = Color.Black;
        }

        private void ButtonDuty_OnClicked(object sender, EventArgs e)
        {
            SetKind(ButtonDuty.Text);
            
            FrameDuty.HasShadow = true;
            FrameDuty.BorderColor = hex;
            ButtonDuty.TextColor = hex;
            
            ImageDuty.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", hex.ToHex() } };
            
            FrameHouse.HasShadow = false;
            FrameHouse.BorderColor = Color.Gray;
            ButtonHouse.TextColor = Color.Gray;
            
            ImageHouse.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
           
            FrameRyon.HasShadow = false;
            FrameRyon.BorderColor = Color.Gray;
            ButtonRyon.TextColor = Color.Gray;
            
            ImageRyon.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
            
            FrameIdent.HasShadow = false;
            FrameIdent.BorderColor = Color.Gray;
            ButtonIdent.TextColor = Color.Gray;
            
            ImageIdent.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
            
        }

        private void ButtonIdent_OnClicked(object sender, EventArgs e)
        {
            SetKind(ButtonIdent.Text);
            
            FrameIdent.HasShadow = true;
            FrameIdent.BorderColor = hex;
            ButtonIdent.TextColor = hex;
            
            ImageIdent.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", hex.ToHex() } };
            
            FrameHouse.HasShadow = false;
            FrameHouse.BorderColor = Color.Gray;
            ButtonHouse.TextColor = Color.Gray;
            
            ImageHouse.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
           
            FrameRyon.HasShadow = false;
            FrameRyon.BorderColor = Color.Gray;
            ButtonRyon.TextColor = Color.Gray;
            
            ImageRyon.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
            
            FrameDuty.HasShadow = false;
            FrameDuty.BorderColor = Color.Gray;
            ButtonDuty.TextColor = Color.Gray;
            
            ImageDuty.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
        }

        private void ButtonRyon_OnClicked(object sender, EventArgs e)
        {
            SetKind(ButtonRyon.Text);
            
            FrameRyon.HasShadow = true;
            FrameRyon.BorderColor = hex;
            ButtonRyon.TextColor = hex;
            
            ImageRyon.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", hex.ToHex() } };
            
            FrameHouse.HasShadow = false;
            FrameHouse.BorderColor = Color.Gray;
            ButtonHouse.TextColor = Color.Gray;
            
            ImageHouse.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
           
            FrameIdent.HasShadow = false;
            FrameIdent.BorderColor = Color.Gray;
            ButtonIdent.TextColor = Color.Gray;
            
            ImageIdent.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
            
            FrameDuty.HasShadow = false;
            FrameDuty.BorderColor = Color.Gray;
            ButtonDuty.TextColor = Color.Gray;
            
            ImageDuty.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
        }

        private void ButtonHouse_OnClicked(object sender, EventArgs e)
        {
            SetKind(ButtonHouse.Text);
            
            FrameHouse.HasShadow = true;
            FrameHouse.BorderColor = hex;
            ButtonHouse.TextColor = hex;
            
            ImageHouse.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", hex.ToHex() } };
            
            FrameRyon.HasShadow = false;
            FrameRyon.BorderColor = Color.Gray;
            ButtonRyon.TextColor = Color.Gray;
            
            ImageRyon.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
           
            FrameIdent.HasShadow = false;
            FrameIdent.BorderColor = Color.Gray;
            ButtonIdent.TextColor = Color.Gray;
            
            ImageIdent.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
            
            FrameDuty.HasShadow = false;
            FrameDuty.BorderColor = Color.Gray;
            ButtonDuty.TextColor = Color.Gray;
            
            ImageDuty.ReplaceStringMap = new System.Collections.Generic.Dictionary<string, string> { { "#000000", "#777777" } };
        }
    }
}