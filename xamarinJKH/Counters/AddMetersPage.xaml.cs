using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.Counters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMetersPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        private MeterInfo meter = new MeterInfo();
        private List<MeterInfo> meters = new List<MeterInfo>();
        private CountersPage _countersPage;
        private List<CounterEntryNew> CounterEntryNews = new List<CounterEntryNew>();
        private decimal _counterThisMonth = 0;

        public Color CellColor { get; set; } 
        public decimal PrevCounter { get; set; }
        decimal PrevValue;
        bool SetPrev;
        int DecimalPoint { get; set; }
        int IntegerPoint { get; set; }

        string mask;
        public string Mask
        {
            get => mask;
            set
            {
                mask = value;
                OnPropertyChanged("Mask");
            }
        }

        string prev;
        public string Previous
        {
            get => prev;
            set
            {
                prev = value;
                OnPropertyChanged("Previous");
            }
        }

        private Entry Data = new Entry();
        public AddMetersPage(MeterInfo meter, List<MeterInfo> meters, CountersPage countersPage, decimal counterThisMonth = 0, decimal counterPrevMonth = 0)
        {            
            InitializeComponent();
            
            IntegerPoint = meter.NumberOfIntegerPart;
            DecimalPoint = meter.NumberOfDecimalPlaces;
            BindingContext = this;
            GetFocusCells();
            Analytics.TrackEvent("Передача показаний по счетчику №" + meter.UniqueNum);
            NavigationPage.SetHasNavigationBar(this, false);
            _countersPage = countersPage;
            _counterThisMonth = counterThisMonth;
            
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    Data = new Entry
                    {
                        Keyboard = Keyboard.Numeric,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalTextAlignment = TextAlignment.Center
                    };
                    FrameEntry.Content = Data;
                    break;
                default:
                    Data = new EntryWithCustomKeyboard
                    {
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalTextAlignment = TextAlignment.Center,
                        IntegerPoint = IntegerPoint,
                        DecimalPoint = DecimalPoint
                    };
                    FrameEntry.Content = Data;
                    break;
            }
            SetMask();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //для iphone5,5s,se,5c
                    if (DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        //CounterLayout.Margin = new Thickness(5, 0);
                        meterRootStack.Margin = new Thickness(5);

                        NameLbl.FontSize = 15;
                        UniqNumLbl.FontSize = 13;
                        CheckupLbl.FontSize = 13;
                        RecheckLbl.FontSize = 13;
                    }

                    UniqNumLbl.Margin = new Thickness(0, 5, 0, -5);
                    CheckupLbl.Margin = new Thickness(0, -5, 0, -5);
                    RecheckLbl.Margin = new Thickness(0, -5, 0, -5);

                    break;
                case Device.Android:
                    if (DeviceDisplay.MainDisplayInfo.Width <= 720)
                    {
                        meterRootStack.Margin = new Thickness(5);

                        NameLbl.FontSize = 15;
                        UniqNumLbl.FontSize = 13;
                        CheckupLbl.FontSize = 13;
                        RecheckLbl.FontSize = 13;
                    }

                    UniqNumLbl.Margin = new Thickness(0, 5, 0, -5);
                    CheckupLbl.Margin = new Thickness(0, -5, 0, -5);
                    RecheckLbl.Margin = new Thickness(0, -5, 0, -5);

                    break;
                default:
                    break;
            }
            this.meter = meter;
            this.meters = meters;
            var backClick = new TapGestureRecognizer();

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);
         
            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);
            FrameBtnLogin.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);

            if (counterPrevMonth > 0)
            {
                SetPrev = true;
                SetPreviousValue(counterPrevMonth);
            }

            if(counterThisMonth>0)
            {
                meterReadingName.Text = AppResources.ChangePenance;
                SetCurrent(counterThisMonth);
                SetCurrentValue(counterThisMonth);
            }
            else
            {
                meterReadingName.Text = AppResources.NewData;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                CellColor = (Color)Application.Current.Resources["MainColor"];
            });            


            SetTextAndColor();
            Data.Focused += Entry_Focused;
            Analytics.TrackEvent("Установка кол-ва знаков после запятой " + DecimalPoint);
        }

        private async void SetMask()
        {
                string result = string.Empty;
            var integer = IntegerPoint;
            var decimal_ = DecimalPoint;
                while (integer > 0)
                {
                    result += "X";
                    integer--;
                }
                result += ".";
                while (decimal_ > 0)
                {
                    result += "X";
                    decimal_--;
                }
                Mask = result;
                if(Device.RuntimePlatform == Device.iOS)
                  Data.TextChanged += Data_TextChanged;
            
        }

        private async void Data_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            if (e.NewTextValue == null)
                return;

            if (e.NewTextValue == "," || e.NewTextValue == ".")
            {
                entry.TextChanged -= Data_TextChanged;

                if (e.OldTextValue != null)
                    entry.Text = e.OldTextValue;
                else
                    entry.Text = "";
                entry.TextChanged += Data_TextChanged;
                return;
            }            

            if (e.NewTextValue.Count(_=>_==',')>1 || e.NewTextValue.Count(_ => _ == '.') > 1)
            {
                
                entry.Text = e.NewTextValue.Remove(e.NewTextValue.Length - 1); 
                return;
            }

            if ((entry.Text.Contains(",") || entry.Text.Contains(".")) && entry.Text.Length > 1)
            {
                var numbers = entry.Text.Split(',', '.');
                if (numbers[1].Length > DecimalPoint)
                {
                    entry.Text = entry.Text.Remove(entry.Text.Length - 1);
                }
            }
            else
            {
                if(e.OldTextValue!=null)
                    if (e.OldTextValue.Length == IntegerPoint && e.NewTextValue.Length > e.OldTextValue.Length)
                    {
                        entry.Text = e.OldTextValue;
                    }
            }
            if (e.NewTextValue.Equals("-"))
            {
                if (e.OldTextValue != null)
                    entry.Text = e.OldTextValue;
                else
                    entry.Text = "";
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            entry.Text = entry.Text.Replace(".", ",");
        }

        async void SetPreviousValue(decimal prevCount)
        {
            var format = "{0:" + Mask.Replace("X","0").Replace(",",".") + "}";
            Prev.Text = String.Format(format, prevCount);
        }

        async void SetCurrentValue(decimal currentCount)
        {
            var format = "{0:" + Mask.Replace("X", "0").Replace(",", ".") + "}";
            Data.Text = String.Format(format, currentCount);
            await Task.Delay(TimeSpan.FromSeconds(2)); 
            
           
        }
      
        void GetFocusCells()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(10), OnTimerTick);
        }

        private bool OnTimerTick()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool flag = false;
                foreach (var each in CounterEntryNews)
                {
                    flag = each.IsFocused;
                }

                if (!flag)
                {
                    foreach (var each in CounterEntryNews)
                    {
                        if (string.IsNullOrWhiteSpace(each.Text))
                        {
                            each.Text = "0";
                        }
                    }
                }
            });
            return true;
        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
           
        }

        private void SetCurrent(decimal counterThisMonth)
        {
            Analytics.TrackEvent("Установка предыдущих показаний" + counterThisMonth );

            var d = GetNumbers(counterThisMonth);
        }

        List<string> GetNumbers(decimal counter)
        {
            var retList = new List<string>();
            try
            {
                var counter8 = Convert.ToInt64(counter * 1000);
                for (int i = 0; i < ((IntegerPoint == 5 || IntegerPoint ==0) ? 8 : 9); i++)
                {
                    var d = counter8 % 10;
                    retList.Add(d.ToString());
                    counter8 = (counter8 - d) / 10;
                }

                return retList;
            }
            catch
            {
            }
            return retList;

        }

        string value1 = "";
        string value2 = "";
        string value3 = "";

        private async void ButtonClick(object sender, EventArgs e)
        {
            Analytics.TrackEvent("Попытка отправки показаний");

            try {
                string count ="";
              
                {
                    

                    count = Data.Text;
                    decimal prevPenencies;

                    switch (tarif)
                    {
                        case 1:
                            value1 = count;
                            SaveInfoAccount();
                            break;
                        case 2:
                            if (meter.TariffNumberInt == 3)
                            {
                                tarif = 3;
                                BtnSave.Text = AppResources.NextTarif;// "Следующий тариф";

                            }
                            else
                            {
                                tarif = 0;//идем в default
                                BtnSave.Text = AppResources.PassPenance;// "Передать показания"
                                IconViewSave.IsVisible = true;
                                IconArrowForward.IsVisible = false;
                            }
                            value1 = count;

                            if (_counterThisMonth == 0)
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff2Name) ? AppResources.Tarif2Meters : AppResources.EnterTarifMeters + " \"" + meter.Tariff2Name + "\""; //AppResources.Tarif2Meters;// "Показания по второму тарифу";                 
                                Data.Text = string.Empty;
                            }
                            else
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff2Name) ? AppResources.EditMetersTarif2 : AppResources.EditMetersTarif + " \"" + meter.Tariff2Name + "\""; // "изменить показания по второму тарифу";
                                if (meter.Values[0].ValueT2 != null)
                                    SetCurrentValue(Convert.ToDecimal(meter.Values[0].ValueT2));
                            }

                            if (meter.Values != null && meter.Values.Count >= 1)
                            {
                                int monthCounter;
                                var parceMonthOk = int.TryParse(meter.Values[0].Period.Split('.')[1], out monthCounter);
                                if (parceMonthOk)
                                {
                                    if (monthCounter == DateTime.Now.Month)
                                    {
                                        if (meter.Values.Count >= 2)
                                        {
                                            if (meter.Values[1].ValueT2 != null)
                                                prevPenencies = Convert.ToDecimal(meter.Values[1].ValueT2);
                                            else
                                                prevPenencies = 0;
                                        }
                                        else
                                        {
                                            prevPenencies = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (meter.Values[0].ValueT2 != null)
                                            prevPenencies = Convert.ToDecimal(meter.Values[0].ValueT2);
                                        else
                                            prevPenencies = 0;
                                    }
                                }
                                else
                                {
                                    if (meter.Values[0].ValueT2 != null)
                                        prevPenencies = Convert.ToDecimal(meter.Values[0].ValueT2);
                                    else
                                        prevPenencies = 0;
                                }
                            }
                            else
                            {
                                prevPenencies = 0;
                            }

                            SetPreviousValue(prevPenencies);

                            break;
                        case 3:

                            tarif = 0;//идем в default
                            BtnSave.Text = AppResources.PassPenance;// "Передать показания"
                            IconViewSave.IsVisible = true;
                            IconArrowForward.IsVisible = false;

                            value2 = count;

                            if (_counterThisMonth == 0)
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff3Name) ? AppResources.Tarif3Meters : AppResources.EnterTarifMeters + " \"" + meter.Tariff3Name + "\""; //AppResources.Tarif3Meters;// "Показания по 3му тарифу";                 
                                Data.Text = string.Empty;
                            }
                            else
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff3Name) ? AppResources.EditMetersTarif3 : AppResources.EditMetersTarif + " \"" + meter.Tariff3Name + "\""; //"Изменить показания по 3му тарифу";                 
                                if (meter.Values[0].ValueT3 != null)
                                    SetCurrent(Convert.ToDecimal(meter.Values[0].ValueT3));
                            }

                            if (meter.Values != null && meter.Values.Count >= 1)
                            {
                                int monthCounter;
                                var parceMonthOk = int.TryParse(meter.Values[0].Period.Split('.')[1], out monthCounter);
                                if (parceMonthOk)
                                {
                                    if (monthCounter == DateTime.Now.Month)
                                    {
                                        if (meter.Values.Count >= 2)
                                        {
                                            if (meter.Values[1].ValueT3 != null)
                                                prevPenencies = Convert.ToDecimal(meter.Values[1].ValueT3);
                                            else
                                                prevPenencies = 0;
                                        }
                                        else
                                        {
                                            prevPenencies = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (meter.Values[0].ValueT3 != null)
                                            prevPenencies = Convert.ToDecimal(meter.Values[0].ValueT3);
                                        else
                                            prevPenencies = 0;
                                    }
                                }
                                else
                                {
                                    if (meter.Values[0].ValueT3 != null)
                                        prevPenencies = Convert.ToDecimal(meter.Values[0].ValueT3);
                                    else
                                        prevPenencies = 0;
                                }
                            }
                            else
                            {
                                prevPenencies = 0;
                            }

                            SetPreviousValue(prevPenencies);

                            break;
                        default:
                            if (value2 != "")
                                value3 = count;
                            else
                                value2 = count;
                            SaveInfoAccount();
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersError, "OK");
            }
            
        }

        int tarif = 1;
        private Keyboard _dataKeyboard;


        void SetTextAndColor()
        {
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            if ((meter.Resource.ToLower().Contains("холод") && !meter.Resource.ToLower().Contains("гвс")) || (meter.Resource.ToLower().Contains("хвс") && !meter.Resource.ToLower().Contains("гвс") ))
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
                    meter.Resource += ", м3";
            }
            else if (meter.Resource.ToLower().Contains("горяч") || meter.Resource.ToLower().Contains("гвс"))
            {
                img.Source = ImageSource.FromFile("ic_heat_water");
                
            }else if (meter.Resource.ToLower().Contains("подог") || meter.Resource.ToLower().Contains("отопл")|| meter.Resource.ToLower().Contains("тепл"))
            {
                img.Source = ImageSource.FromFile("ic_heat_energ");
            }
            else if (meter.Resource.ToLower().Contains("эле"))
            {
                img.Source = ImageSource.FromFile("ic_electr");

                //если это э/э и не указаны ед. измерения, в RU локали добавляем их
                if (!meter.Resource.ToLower().Contains("кВт"))
                    meter.Resource += ", кВт";
            }else if (meter.Resource.ToLower().Contains("газ"))
            {
                img.Source = ImageSource.FromFile("ic_gas");
            }
            else
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
            }

            //если это вода и не указаны ед. измерения, в RU локали добавляем их
            if ((meter.Resource.ToLower().Contains("горячее") || meter.Resource.ToLower().Contains("гвс")
                || meter.Resource.ToLower().Contains("холодное") || meter.Resource.ToLower().Contains("хвс"))
                && !meter.Resource.ToLower().Contains("м3"))
                meter.Resource += ", м3";

            //для двухтарифного/трехтарифного счетчика
            if (meter.TariffNumberInt>1)
            {
                tarif = 2;
                if (_counterThisMonth == 0)
                    meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff1Name) ? AppResources.Tarif1Meters : AppResources.EnterTarifMeters + " \"" + meter.Tariff1Name + "\""; //AppResources.Tarif1Meters;// "Показания по первому тарифу";                 
                else
                    meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff1Name) ? AppResources.EditMetersTarif1 : AppResources.EditMetersTarif + " \"" + meter.Tariff1Name + "\""; //AppResources.Tarif1Meters;// "Показания по первому тарифу";                 
                meterReadingName.FontSize = 16;
                FrameBtnLogin.Margin = new Thickness(0, 0, 0, 10);
                BtnSave.Text = AppResources.NextTarif;// "Следующий тариф";
                IconArrowForward.IsVisible = true;
                IconViewSave.IsVisible = false;
            }

            UkName.Text = Settings.MobileSettings.main_name;
         
            NameLbl.Text = meter.CustomName != null && !meter.CustomName.Equals("") ? meter.CustomName : meter.Resource;
            progress.Color = (Color)Application.Current.Resources["MainColor"];
            FrameBtnLogin.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            FormattedString formattedUniq = new FormattedString();
            formattedUniq.Spans.Add(new Span
            {
                Text = AppResources.FacNum,
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedUniq.Spans.Add(new Span
            {
                Text = meter.FactoryNumber,
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            UniqNumLbl.FormattedText = formattedUniq;

            FormattedString formattedCheckup = new FormattedString();
            formattedCheckup.Spans.Add(new Span
            {
                Text = AppResources.LastCheck + " ",
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedCheckup.Spans.Add(new Span
            {
                Text = meter.NextCheckupDate,
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            CheckupLbl.FormattedText = formattedCheckup;

            FormattedString formattedRecheckup = new FormattedString();
            formattedRecheckup.Spans.Add(new Span
            {
                Text = AppResources.CheckInterval + " ",
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedRecheckup.Spans.Add(new Span
            {
                Text = meter.RecheckInterval.ToString() + " лет",
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            RecheckLbl.FormattedText = formattedRecheckup;
            if (meter.Values.Count != 0)
            {
                BindingContext = new AddMetersPageViewModel(SetPrev ? PrevValue : meter.Values[0].Value);
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#494949"));
            FrameMeterReading.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        string checkDouble(string val)
        {
            string ds="";
            double d;
            var db = Double.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
            if (db)
            {
                ds = d.ToString(CultureInfo.InvariantCulture);
            }
            return ds;
        }

        public async void SaveInfoAccount()
        {
            Analytics.TrackEvent("Передача показаний на сервер");
            bool rate = Preferences.Get("rate", true);

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            if (!string.IsNullOrEmpty(value1))
            {
                //string d1 = Double.Parse(value1.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                //var d2 = value2 != "" ? Double.Parse(value2.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "";
                //var d3 = value3 != "" ? Double.Parse(value3.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "";

                var d1 = checkDouble(value1);
                if (string.IsNullOrWhiteSpace(d1))
                {
                    Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersNotNumber, "OK"));
                    return;
                }

                var d2 = "";
                if (value2 != "")
                {
                    d2 = checkDouble(value2);
                    if (string.IsNullOrWhiteSpace(d2))
                    {
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersNotNumber, "OK"));
                        return;
                    } 
                }

                var d3 = "";
                if (value3 != "")
                {
                    d3 = checkDouble(value3);
                    if (string.IsNullOrWhiteSpace(d3))
                    {
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersNotNumber, "OK"));
                        return;
                    }
                }



                progress.IsVisible = true;
                FrameBtnLogin.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.SaveMeterValue(meter.ID.ToString(), d1, 
                   d2, d3);
                if (result.Error == null)
                {
                    Analytics.TrackEvent("Показания Т1:" + d1 + " Т2:" + d2 + " Т3:" + d3 + " переданы");
                    
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", AppResources.AddMetersSuccess, "OK");
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (rate)
                    {
                        await PopupNavigation.Instance.PushAsync(new RatingAppMarketDialog());
                    }
                    try
                    {
                        _ = await Navigation.PopAsync();
                    }
                    catch { }
                    _countersPage.RefreshCountersData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
            }
            else
            {
                await DisplayAlert(AppResources.AddMetersNoData, "", "OK");
            }
        }
    }
}