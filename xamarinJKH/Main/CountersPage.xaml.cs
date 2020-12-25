using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.Counters;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CountersPage : ContentPage
    {
        public List<MeterInfo> _meterInfo { get; set; }
        public List<MeterInfo> _meterInfoAll { get; set; }
        private string account = "";
        public ObservableCollection<AccountInfo> Accounts { get; set; }
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;

        public Color hex { get; set; }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public Command ChangeTheme { get; set; }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;
                    await RefreshCountersData();
                    IsRefreshing = false;
                });
            }
        }


        public async Task RefreshCountersData()
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            Device.BeginInvokeOnMainThread( () =>
            {
                if(!IsRefreshing)
                aIndicator.IsVisible = true;
            });

            try
            {
                ItemsList<MeterInfo> info = await _server.GetThreeMeters();
                if (info.Error == null && info.Data != null)
                {
                    if (string.IsNullOrEmpty(account) || string.IsNullOrWhiteSpace(account))
                    {
                        account = "Все";
                    }
                    _meterInfoAll = info.Data;
                    if (account == "Все")
                    {
                        _meterInfo = _meterInfoAll;
                    }
                    else
                    {
                        List<MeterInfo> meters = new List<MeterInfo>();
                        foreach (var meterInfo in _meterInfoAll)
                        {
                            if (meterInfo.Ident == account)
                            {
                                meters.Add(meterInfo);
                            }
                        }

                        _meterInfo = meters;
                    }

                    if (info.Data.Count == 0)
                    {
                        baseForCounters.Children.Clear();
                        if  (Accounts.Count > 1)
                        {
                            
                            baseForCounters.Children.Add(new Label
                            {
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontSize = 18,
                                Text = AppResources.NoMetersIdent,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                HeightRequest = 400
                            });
                        }
                        
                    }
                    else
                    {
                        
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(100));
                            //var idents = _meterInfo.Select(x => x.Ident);
                            //foreach (var account in Settings.Person.Accounts)
                            //{
                                
                            //    if (!Accounts.Contains(Accounts.FirstOrDefault(x => x.Ident == account.Ident)))
                            //    {
                            //        Accounts.Add(account);
                            //    }
                            //}
                            //var all = Accounts.Where(x => x.Ident == AppResources.All).ToList();
                            //if (all.Count > 1)
                            //{
                            //    foreach (var account in all)
                            //    {
                            //        Accounts.Remove(account);
                            //    }
                            //    //Accounts.Insert(0, new AccountInfo() { Ident = AppResources.All, Selected = true });
                            //}
                            
                            if (SelectedAccount == null)
                            {
                                //Accounts.Insert(0, new AccountInfo() { Ident = AppResources.All, Selected = true });
                                if (Accounts.Count > 0)
                                SelectedAccount = Accounts[0];

                            }
                            else if (SelectedAccount.Ident == AppResources.All)
                                SelectedAccount = Accounts[0];
                            else if (!Accounts.Contains(SelectedAccount))
                            {
                                SelectedAccount = Accounts[0];
                                foreach (var account in Accounts)
                                {
                                    account.Selected = false;
                                }
                                SelectedAccount.Selected = true;
                            }
                            //addIdentLbl.IsVisible = false;

                            List<MeterInfo> meters_ = new List<MeterInfo>();
                            if (SelectedAccount != null && SelectedAccount.Ident != AppResources.All)
                                meters_ = _meterInfo.Where(x => x.Ident == SelectedAccount.Ident).ToList();
                            else
                                meters_ = _meterInfo;
                            baseForCounters.Children.Clear();

                            if (meters_.Count == 0 && Accounts.Count > 0)
                            {
                                baseForCounters.Children.Add(new Label
                                {
                                    VerticalTextAlignment = TextAlignment.Center,
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    FontSize = 18,
                                    Text = AppResources.NoMetersIdent,
                                    VerticalOptions = LayoutOptions.FillAndExpand,
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    HeightRequest = 400
                                });
                            }

                            foreach (var mi in meters_)
                            {
                                var mtc = new MetersThreeCell(mi);
                                TapGestureRecognizer tap = new TapGestureRecognizer();
                                tap.Tapped += Tap_Tapped;
                                mtc.GestureRecognizers.Add(tap);

                                baseForCounters.Children.Add(mtc);
                            }
                        });
                    }

                    
                }
                OSAppTheme currentTheme = Application.Current.RequestedTheme;
                SetHeader(currentTheme);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    aIndicator.IsVisible = false;
                    IsRefreshing = false;
                });

        }
            catch (Exception e)
            {

                Microsoft.AppCenter.Crashes.Crashes.TrackError(e);
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    aIndicator.IsVisible = false;
                    IsRefreshing = false;
                });
        }



        //countersList.ItemsSource = null;
        //countersList.ItemsSource = _meterInfo;
    }

        private async void Tap_Tapped(object sender, EventArgs e)
        {
            int currDay = DateTime.Now.Day;

            MeterInfo select = ((MetersThreeCell) sender).meterInfo; // as MeterInfo;
            if (select != null)
            {
                if (select.IsDisabled || select.AutoValueGettingOnly)
                {
                    return;
                }

                if (Settings.Person != null)
                    if (Settings.Person.Accounts != null)
                        if (Settings.Person.Accounts.Count > 0)
                            if (select.ValuesCanAdd)
                            {
                                if (select.Values!=null && select.Values.Count >= 1 )
                                {
                                    // int monthCounter;
                                    // var parceMonthOk = int.TryParse(select.Values[0].Period.Split('.')[1], out monthCounter) ;
                                    // if(parceMonthOk)
                                    // {
                                        if(select.Values[0].IsCurrentPeriod)
                                        {
                                            var counterThisMonth = select.Values[0].Value;
                                            var counterThisMonth2 = select.Values.Count >= 2 ? select.Values[1].Value : 0;
                                            if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                                await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this,
                                                counterThisMonth,
                                                counterThisMonth2));
                                        }
                                        else
                                        {
                                            var counterThisMonth =  select.Values[0].Value;
                                            if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                                await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, 0,
                                                counterThisMonth));
                                        }                                        
                                    // }
                                    // else
                                    // {
                                    //     var counterThisMonth = select.Values[0].Value;
                                    //     if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                    //         await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, 0,
                                    //         counterThisMonth));
                                    // }
                                }
                                else
                                {
                                    //var counterThisMonth =  0;
                                    if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                        await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, 0,
                                        0));
                                }
                                //if (select.Values.Count >= 1 && int.Parse(select.Values[0].Period.Split('.')[1]) ==
                                //    DateTime.Now.Month)
                                //{
                                //    var counterThisMonth = (select.Values.Count >= 1) ? select.Values[0].Value : 0;
                                //    var counterThisMonth2 = (select.Values.Count >= 2) ? select.Values[1].Value : 0;
                                //    await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this,
                                //        counterThisMonth,
                                //        counterThisMonth2));
                                //}
                                //else
                                //{
                                //    var counterThisMonth = (select.Values.Count >= 1) ? select.Values[0].Value : 0;
                                //    await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, 0,
                                //        counterThisMonth));
                                //}
                            }
            }
        }

        public CountersPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Показания");
            Accounts = new ObservableCollection<AccountInfo>();
            Settings.mainPage = this;
            NavigationPage.SetHasNavigationBar(this, false);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);

                    break;
                default:
                    break;
            }

            hex = (Color)Application.Current.Resources["MainColor"];

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new AppPage());};
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone))
                        phoneDialer.MakePhoneCall(
                            System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }
            };
            var pickLs = new TapGestureRecognizer();
            pickLs.Tapped += async (s, e) => {  
                Device.BeginInvokeOnMainThread(() =>
                {
                    Picker.Focus();
                });
            };
            //StackLayoutLs.GestureRecognizers.Add(pickLs);
            SetTextAndColor();
            getInfo();
            SetTitle();

            //countersList.BackgroundColor = Color.Transparent;
            baseForCounters.BackgroundColor = Color.Transparent;
            //if (Device.RuntimePlatform != Device.iOS)
            //    countersList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
            var goAddIdent = new TapGestureRecognizer();
            goAddIdent.Tapped += async (s, e) =>
            {
                /*await Dialog.Instance.ShowAsync<AddAccountDialogView>();*/
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddIdent) == null)
                    await Navigation.PushAsync(new AddIdent((PaysPage)Settings.mainPage));
            };
            StackLayoutAddIdent.GestureRecognizers.Add(goAddIdent);
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
          

                //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.FromHex("#494949"));
            ChangeTheme = new Command(async () => { SetTitle(); });
            MessagingCenter.Subscribe<Object>(this, "ChangeThemeCounter", (sender) => ChangeTheme.Execute(null));
            //MessagingCenter.Subscribe<Object>(this, "UpdateCounters", (sender) => RefreshCommand.Execute(null));
            MessagingCenter.Subscribe<Object>(this, "UpdateCounters", async (sender) => await RefreshCountersData());
            MessagingCenter.Subscribe<Object, AccountInfo>(this, "AddIdent", async (sender, ident) =>
            {
                //await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (ident != null)
                {
                    var contain = Accounts.FirstOrDefault(x => x.Ident == ident.Ident);
                    if (contain == null)
                        Device.BeginInvokeOnMainThread(() => Accounts.Add(ident));

                    var all = Accounts.FirstOrDefault(x => x.Ident == AppResources.All);
                    if (all == null)
                    {
                        Device.BeginInvokeOnMainThread(() => Accounts.Insert(0, new AccountInfo { Ident = AppResources.All, Selected = true }));
                    }

                }
                Device.BeginInvokeOnMainThread(async () =>
                {
                    //
                    
                    //OnPropertyChanged("Accounts");
                    //var all = Accounts.FirstOrDefault(x => x.Ident == AppResources.All);
                    //if (all == null)
                    //{
                    //    Accounts.Insert(0, new AccountInfo { Ident = AppResources.All, Selected = true });
                    //}
                    //Accounts.Clear();
                    //Accounts.Add(new AccountInfo { Ident = AppResources.All, Selected = true });
                    //foreach (var account in Settings.Person.Accounts)
                    //{
                    //    Accounts.Add(account);
                    //}
                    //await RefreshCountersData();
                });
                
            });
            MessagingCenter.Subscribe<Object, AccountInfo>(this, "RemoveIdent", async (sender, ident) =>
            {
                //await Task.Delay(TimeSpan.FromMilliseconds(500));
                Device.BeginInvokeOnMainThread(() => 
                {
                    if (SelectedAccount != null && ident != null)
                    if (SelectedAccount.Ident == ident.Ident)
                        SelectedAccount = null;
                    if (ident != null)
                    Accounts.Remove(Accounts.First(x => x.Ident == ident.Ident));
                    if (Accounts.Count == 1)
                    {
                        Accounts.Clear();
                    }
                });
            });
            Device.BeginInvokeOnMainThread(() =>
            {
                var all = Accounts.FirstOrDefault(x => x.Ident == AppResources.All);
                if (all == null && Settings.Person.Accounts.Count > 0)
                {
                    Accounts.Insert(0, new AccountInfo { Ident = AppResources.All, Selected = true });
                }
                
                foreach (var account in Settings.Person.Accounts)
                {
                    Accounts.Add(account);
                }
            });
            
            Picker.ItemsSource = Accounts;
        }

        private void SetTitle()
        {
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            var colors = new Dictionary<string, string>();
            var arrowcolor = new Dictionary<string, string>();
            if (currentTheme == OSAppTheme.Light || currentTheme == OSAppTheme.Unspecified)
            {
                colors.Add("#000000", ((Color) Application.Current.Resources["MainColor"]).ToHex());
                arrowcolor.Add("#000000", "#494949");
            }
            else
            {
                colors.Add("#000000", "#FFFFFF");
                arrowcolor.Add("#000000", "#FFFFFF");
            }

            //IconViewTech.ReplaceStringMap = colors;
            //Arrow.ReplaceStringMap = arrowcolor;

            
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            SetHeader(currentTheme);
        }

        private void SetHeader(OSAppTheme currentTheme)
        {
            var day = DateTime.Now.Day;
            if (Settings.Person != null)
                if (Settings.Person.Accounts != null)
                    if (Settings.Person.Accounts.Count > 0)
                    {
                        FormattedString formattedResource = new FormattedString();
                        formattedResource.Spans.Add(new Span
                        {
                            Text = AppResources.CountersInfo1,
                            TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                            FontAttributes = FontAttributes.None,
                            FontSize = 12
                        });
                        if (Settings.Person.Accounts[0].MetersStartDay != null &&
                            Settings.Person.Accounts[0].MetersEndDay != null)
                        {
                            if (Settings.Person.Accounts[0].MetersStartDay != 0 &&
                                Settings.Person.Accounts[0].MetersEndDay != 0)
                            {
                                if (Settings.Person.Accounts[0].MetersStartDay >
                                    Settings.Person.Accounts[0].MetersEndDay)
                                {
                                    if (day <= Settings.Person.Accounts[0].MetersEndDay)
                                    {
                                        formattedResource.Spans.Add(new Span
                                        {
                                            Text =
                                                $" {AppResources.From} {Settings.Person.Accounts[0].MetersStartDay} " +
                                                $"{AppResources.PreviousMonth} {AppResources.To} {Settings.Person.Accounts[0].MetersEndDay} ",
                                            TextColor = currentTheme.Equals(OSAppTheme.Light)
                                                ? Color.Black
                                                : Color.White,
                                            FontAttributes = FontAttributes.Bold,
                                            FontSize = 12
                                        });
                                        formattedResource.Spans.Add(new Span
                                        {
                                            Text = AppResources.CountersCurrentMonth,
                                            TextColor = currentTheme.Equals(OSAppTheme.Light)
                                                ? Color.Black
                                                : Color.White,
                                            FontAttributes = FontAttributes.None,
                                            FontSize = 12
                                        });
                                    }
                                    else
                                    {
                                        formattedResource.Spans.Add(new Span
                                        {
                                            Text =
                                                $" {AppResources.From} {Settings.Person.Accounts[0].MetersStartDay} " +
                                                $"{AppResources.CountersCurrentMonth} {AppResources.To} {Settings.Person.Accounts[0].MetersEndDay} ",
                                            TextColor = currentTheme.Equals(OSAppTheme.Light)
                                                ? Color.Black
                                                : Color.White,
                                            FontAttributes = FontAttributes.Bold,
                                            FontSize = 12
                                        });
                                        formattedResource.Spans.Add(new Span
                                        {
                                            Text = AppResources.NextMonth,
                                            TextColor = currentTheme.Equals(OSAppTheme.Light)
                                                ? Color.Black
                                                : Color.White,
                                            FontAttributes = FontAttributes.None,
                                            FontSize = 12
                                        });
                                    }
                                }
                                else
                                {
                                    formattedResource.Spans.Add(new Span
                                    {
                                        Text = AppResources.From + Settings.Person.Accounts[0].MetersStartDay +
                                               AppResources.To +
                                               Settings.Person.Accounts[0].MetersEndDay + AppResources.DayOfMounth,
                                        TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                                        FontAttributes = FontAttributes.Bold,
                                        FontSize = 12
                                    });
                                    formattedResource.Spans.Add(new Span
                                    {
                                        Text = AppResources.CountersCurrentMonth,
                                        TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                                        FontAttributes = FontAttributes.None,
                                        FontSize = 12
                                    });
                                }
                            }
                            else
                            {
                                formattedResource.Spans.Add(new Span
                                {
                                    Text = AppResources.CountersThisMonth,
                                    TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 12
                                });
                            }
                        }
                        else
                        {
                            formattedResource.Spans.Add(new Span
                            {
                                Text = AppResources.CountersThisMonth,
                                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 15
                            });
                        }
                        PeriodSendLbl.IsVisible = false;
                        PeriodSendLbl.FormattedText = formattedResource;
                        StackLayoutPicker.IsVisible = true;
                        StackLayoutAddIdent.IsVisible = false;
                        addIdentLbl.IsVisible = false;
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PeriodSendLbl.IsVisible = false;
                            //PeriodSendLbl.FontSize = 15;
                            //PeriodSendLbl.FontAttributes = FontAttributes.Bold;
                            //PeriodSendLbl.Text = AppResources.NoAccounts;
                            addIdentLbl.IsVisible = true;
                            StackLayoutPicker.IsVisible = false;
                            StackLayoutAddIdent.IsVisible = true;
                        });
                        //PeriodSendLbl.IsVisible = true;
                        //PeriodSendLbl.FontSize = 15;
                        ////PeriodSendLbl.FontAttributes = FontAttributes.Bold;
                        //PeriodSendLbl.Text = AppResources.NoAccounts;
                        //StackLayoutPicker.IsVisible = false;
                        //StackLayoutAddIdent.IsVisible = true;
                    }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            new Task(SyncSetup).Start(); // This could be an await'd task if need be
            
            
        }

        async void SyncSetup()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Assuming this function needs to use Main/UI thread to move to your "Main Menu" Page
                RefreshCountersData();
            });
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        { 
        //{
        //    try
        //    {
        //        var identLength = Settings.Person.Accounts[Picker.SelectedIndex - 1].Ident.Length;
        //        if (identLength < 6)
        //        {
        //            Device.BeginInvokeOnMainThread(() =>
        //            {
        //                Picker.WidthRequest = identLength * 10;
        //                //Picker.MinimumWidthRequest = identLength * 9;
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (Picker.SelectedIndex != -1 && Settings.Person.Accounts.Count != 0)
        //        {
        //            var account = Settings.Person.Accounts[Picker.SelectedIndex];
        //            if (account != null)
        //            {
        //                if (!string.IsNullOrEmpty(account.Ident))
        //                {
        //                    var identLength = account.Ident.Length;
        //                    if (identLength < 6)
        //                    {
        //                        Device.BeginInvokeOnMainThread(() =>
        //                        {
        //                            Picker.WidthRequest = identLength * 9;
        //                            // Picker.MinimumWidthRequest = identLength * 9;
        //                        });
        //                    }
        //                }
                        
        //            }
                    
        //        }
            //}

            //if (Accounts != null)
            //{
            //    if (Accounts.Count > 0)
            //    {
            //        account = Accounts[Picker.SelectedIndex];
            //        SetIdents();
            //    }
            //}
        }

        protected async void OnIdentChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                var selected = args.CurrentSelection[0] as AccountInfo;
                //(sender as CollectionView).UpdateSelectedItems(new List<Object> { selected });
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        foreach (var acc in Accounts)
                        {
                            acc.Selected = false;
                        }
                        selected.Selected = true;
                        var newmeters = _meterInfoAll.Where(x => x.Ident == selected.Ident).ToList();

                        if (selected.Ident == AppResources.All)
                        {
                            newmeters = _meterInfoAll;
                        }

                        baseForCounters.Children.Clear();

                        if (newmeters.Count == 0 && Accounts.Count > 0)
                        {
                            baseForCounters.Children.Add(new Label
                            {
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontSize = 18,
                                Text = AppResources.NoMetersIdent,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                HeightRequest = 400
                            });
                        }

                        foreach (var mi in newmeters)
                        {
                            var mtc = new MetersThreeCell(mi);
                            TapGestureRecognizer tap = new TapGestureRecognizer();
                            tap.Tapped += Tap_Tapped;
                            mtc.GestureRecognizers.Add(tap);

                            baseForCounters.Children.Add(mtc);
                        }
                    }
                    catch { }
                });
            }
            catch { }
            
            
        }

        void SetIdents()
        {
            try
            {

                Picker.SetAppThemeColor(Xamarin.Forms.Picker.TextColorProperty, Color.Black, Color.White);
                Picker.SetAppThemeColor(Xamarin.Forms.Picker.TitleColorProperty, Color.Black, Color.White);
                //Picker.Title = account;
                _meterInfo = null;
                if (account == "Все")
                {
                    _meterInfo = _meterInfoAll;
                }
                else
                {
                    List<MeterInfo> meters = new List<MeterInfo>();
                    foreach (var meterInfo in _meterInfoAll)
                    {
                        if (meterInfo.Ident == account)
                        {
                            meters.Add(meterInfo);
                        }
                    }

                    _meterInfo = meters;
                }

                baseForCounters.Children.Clear();

                foreach (var mi in _meterInfo)
                {
                    baseForCounters.Children.Add(new MetersThreeCell(mi));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //countersList.ItemsSource = null;
            //countersList.ItemsSource = _meterInfo;
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
        }

        void SetTextAndColor()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            
        }
        AccountInfo selectedAccount;
        public AccountInfo SelectedAccount
        {
            get => selectedAccount;
            set
            {
                selectedAccount = value;
                OnPropertyChanged("SelectedAccount");
            }
        }

        async void getInfo()
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }

            ItemsList<MeterInfo> info = await _server.GetThreeMeters();
            if (info.Error == null)
            {
                if (info.Data.Count == 0)
                {
                    if (Accounts.Count > 0)
                    {
                        baseForCounters.Children.Clear();

                        baseForCounters.Children.Add(new Label
                        {
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            Text = AppResources.NoMetersIdent,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            HeightRequest = 400
                        });
                    }
                    
                }
                if (Accounts.Count == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        addIdentLbl.IsVisible = true;
                    });
                }
                if (info.Data.Count > 0)
                {
                    _meterInfo = info.Data;
                    _meterInfoAll = info.Data;
                    this.BindingContext = this;
                    var idents = _meterInfoAll.Select(x => x.Ident);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //foreach (var account in Accounts)
                        //{
                        //    if (!idents.Contains(account.Ident) && account.Ident != AppResources.All)
                        //    {
                        //        Device.BeginInvokeOnMainThread(() =>
                        //        {
                        //            Accounts.Remove(account);
                        //        });
                        //    }
                        //}
                        var all = Accounts.FirstOrDefault(x => x.Ident == AppResources.All);
                        if (all == null)
                        Accounts.Insert(0, new AccountInfo { Ident = AppResources.All });
                    });
                    

                    if (_meterInfo != null && _meterInfo.Count > 0)
                    {
                        account = "Все";
                        //Accounts.Add("Все");
                        //foreach (var meterInfo in _meterInfo)
                        //{
                        //    Boolean k = false;
                        //    foreach (var s in Accounts)
                        //    {
                        //        if (s == meterInfo.Ident)
                        //        {
                        //            k = true;
                        //        }
                        //    }

                        //    if (k == false)
                        //    {
                        //        Accounts.Add(meterInfo.Ident);
                        //    }
                        //}

                        Picker.ItemsSource = Accounts;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            SelectedAccount = Accounts[0];
                            Accounts[0].IsFirst = true;
                        });
                        //Picker.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCountersNoData, "OK");
            }

        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            int currDay = DateTime.Now.Day;
            MeterInfo select = e.Item as MeterInfo;
            if (select.IsDisabled || select.AutoValueGettingOnly)
            {
                return;
            }

            if (Settings.Person != null)
                if (Settings.Person.Accounts != null)
                    if (Settings.Person.Accounts.Count > 0)
                        if (select.ValuesCanAdd)
                        {
                            if (select.Values.Count >= 1 &&
                                select.Values[0].IsCurrentPeriod)
                            {
                                var counterThisMonth = (select.Values.Count >= 1) ? select.Values[0].Value : 0;
                                var counterThisMonth2 = (select.Values.Count >= 2) ? select.Values[1].Value : 0;
                                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                    await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, counterThisMonth,
                                    counterThisMonth2));
                            }
                            else
                            {
                                var counterThisMonth = (select.Values.Count >= 1) ? select.Values[0].Value : 0;
                                if (Navigation.NavigationStack.FirstOrDefault(x => x is AddMetersPage) == null)
                                    await Navigation.PushAsync(new AddMetersPage(select, _meterInfo, this, 0,
                                    counterThisMonth));
                            }
                        }
        }
        private static bool CheckPeriod(int currDay, MeterInfo meterInfo)
        {
//#if DEBUG
//            return true;
//#endif
            if (meterInfo.ValuesEndDay < meterInfo.ValuesStartDay)
            {
                return MetersThreeCell.GetPeriodEnabled() || (meterInfo.ValuesStartDay == 0 &&
                                                              meterInfo.ValuesEndDay == 0);

            }
            
            return (meterInfo.ValuesStartDay <= currDay &&
                    meterInfo.ValuesEndDay >= currDay) ||
                   (meterInfo.ValuesStartDay == 0 &&
                    meterInfo.ValuesEndDay == 0);
        }
        private async void RefreshView_RefreshingAsync(object sender, EventArgs e)
        {
            IsRefreshing = true;
            await RefreshCountersData();
            IsRefreshing = false;
        }

        StackLayout lastElementSelected2;
        private void FrameIdentGR_Tapped(object sender, EventArgs e)
        {
            
            //var vm = (BindingContext as CountersPage);

            try
            {
                if (lastElementSelected2 != null)
                {
                    VisualStateManager.GoToState(lastElementSelected2.Children[0], "Normal");
                }

                var el = sender as StackLayout;

                VisualStateManager.GoToState(el.Children[0], "Selected");

                foreach (var account in Accounts)
                {
                    account.Selected = false;
                }

                var om = el.BindingContext as AccountInfo;
                SelectedAccount = om;
                om.Selected = true;
                var newmeters = _meterInfoAll.Where(x => x.Ident == om.Ident).ToList();
                if (om.Ident == AppResources.All)
                {
                    newmeters = _meterInfoAll;
                }
                baseForCounters.Children.Clear();

                if (newmeters.Count == 0 && Accounts.Count > 0)
                {
                    baseForCounters.Children.Add(new Label
                    {
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 18,
                        Text = AppResources.NoMetersIdent,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HeightRequest = 400
                    });
                }
                else
                {
                    foreach (var mi in newmeters)
                    {
                        var mtc = new MetersThreeCell(mi);
                        TapGestureRecognizer tap = new TapGestureRecognizer();
                        tap.Tapped += Tap_Tapped;
                        mtc.GestureRecognizers.Add(tap);

                        baseForCounters.Children.Add(mtc);
                    }
                }

                
            }
            catch { }

            //vm.SelectedTyp = om;
            lastElementSelected2 = (StackLayout)sender;
        }
    }
}