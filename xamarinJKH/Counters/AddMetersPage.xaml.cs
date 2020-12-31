using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Counters;
using xamarinJKH.Main;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using Xamarin.Forms.Internals;
using System.Security.Cryptography;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using xamarinJKH.Tech;
using Xamarin.Essentials;
using xamarinJKH.DialogViews;

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
            //CounterEntryNews = new List<CounterEntryNew>
            //{
            //    d1,d2,d3,d4,d41,d5,d6,d7,d8
            //};
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
                    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 700)
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
                    //GetFocusCells();
                    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width <= 720)
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
                default:
                    break;
            }
            this.meter = meter;
            this.meters = meters;
            var backClick = new TapGestureRecognizer();
            //d41_.IsVisible = IntegerPoint == 6;
            var screen = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width;
            if (IntegerPoint == 6 || (screen <= 720 && Device.RuntimePlatform == "Android"))
            {
               // var view = d41_.Parent as StackLayout;
                //view.Margins<StackLayout>(-10, 0, -10, 0);
                if (Device.RuntimePlatform == "Android")
                {
                    //CounterLayout.Spacing = -5;
                    FrameMeterReading.IsClippedToBounds = true;
                    if (screen <= 720)
                    {
                     //   (view.Parent as StackLayout).Margin = new Thickness(0);
                    }
                    if (screen == 480)
                    {
                        //CounterDigitsContainer.Spacing = 3;
                        //CounterDigitsContainer.Padding = new Thickness(10, 0);
                    }
                }
                
            }

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
            techSend.Tapped += async (s, e) => {    await Navigation.PushAsync(new AppPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
         
            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);
            FrameBtnLogin.BackgroundColor = Color.FromHex(xamarinJKH.Utils.Settings.MobileSettings.color);

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

            //d2.OnBackspace += D2_OnBackspace;
            //d3.OnBackspace += D3_OnBackspace;
            //d4.OnBackspace += D4_OnBackspace;
            //d5.OnBackspace += D5_OnBackspace;
            //d6.OnBackspace += D6_OnBackspace;
            //d7.OnBackspace += D7_OnBackspace;
            //d8.OnBackspace += D8_OnBackspace;

            //d41.OnBackspace += D41_OnBackspace;


            //d1.Focused += Entry_Focused;
            //d2.Focused += Entry_Focused;
            //d3.Focused += Entry_Focused;
            //d4.Focused += Entry_Focused;
            //d5.Focused += Entry_Focused;
            //d6.Focused += Entry_Focused;
            //d7.Focused += Entry_Focused;
            //d8.Focused += Entry_Focused;
            //d41.Focused += Entry_Focused;

            Data.Focused += Entry_Focused;


            //d1.Unfocused += Entry_Unfocused;
            //d2.Unfocused += Entry_Unfocused;
            //d3.Unfocused += Entry_Unfocused;
            //d4.Unfocused += Entry_Unfocused;
            //d5.Unfocused += Entry_Unfocused;
            //d6.Unfocused += Entry_Unfocused;
            //d7.Unfocused += Entry_Unfocused;
            //d8.Unfocused += Entry_Unfocused;

            //StackLayout cds6 = (StackLayout)CounterDigitsContainer.FindByName("cds6"); ;
            //StackLayout cds7 = (StackLayout)CounterDigitsContainer.FindByName("cds7"); ;
            //StackLayout cds8 = (StackLayout)CounterDigitsContainer.FindByName("cds8"); ;

            Analytics.TrackEvent("Установка кол-ва знаков после запятой " + DecimalPoint);
            //switch (DecimalPoint)
            //{
            //    case 0: 
            //        Divider.IsVisible = false;
            //        (d6.Parent.Parent as VisualElement).IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = false;
            //        d6.IsVisible = d7.IsVisible = d8.IsVisible = false;
                   
                      
            //        CounterDigitsContainer.Children.Remove(cds6);
                      
            //        CounterDigitsContainer.Children.Remove(cds7);
                      
            //        CounterDigitsContainer.Children.Remove(cds8);
            //        pc5.CornerRadius = new CornerRadius(5, 5, 0, 5);
            //        break;
            //    case 1: Divider.IsVisible = true;
            //        (d7.Parent.Parent as VisualElement).IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = false;
            //        (d6.Parent.Parent as VisualElement).IsVisible = d6.IsVisible = true;
            //        d7.IsVisible = d8.IsVisible = false;

                    
            //        CounterDigitsContainer.Children.Remove(cds7);
            //        CounterDigitsContainer.Children.Remove(cds8);

            //        pc6.CornerRadius = new CornerRadius(5, 5, 0, 5);
            //        break;
            //    case 2: Divider.IsVisible = true;
            //        (d6.Parent.Parent as VisualElement).IsVisible = d6.IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = d7.IsVisible = true;
            //        (d8.Parent.Parent as VisualElement).IsVisible = false;
            //        d8.IsVisible = false;

            //        CounterDigitsContainer.Children.Remove(cds8);

            //        pc7.CornerRadius = new CornerRadius(5, 5, 0, 5);
            //        break;
            //    case 3: Divider.IsVisible = true;
            //        (d6.Parent.Parent as VisualElement).IsVisible = 
            //            d6.IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = 
            //            d7.IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = d8.IsVisible = true;
            //        break;
                    
            //}
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
                //Data.Behaviors.Add(new xamarinJKH.Mask.MaskedBehavior { Mask = this.Mask });
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
               // entry.Text = e.OldTextValue;
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
            
             // Device.BeginInvokeOnMainThread(() => {
             //     
             //     Data.Focus(); });
        }
        //private void Entry_Unfocused(object sender, FocusEventArgs e)
        //{
        //    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
        //    {
        //        if (Device.RuntimePlatform == Device.iOS)
        //        {
        //            Device.BeginInvokeOnMainThread(() =>
        //            {
        //                FrameMeterReading.Margin = frameCounterMargin;
        //            });
        //        }
        //    }
        //}

        //Thickness frameCounterMargin = new Thickness();
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
            //if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            //{
            //    if (Device.RuntimePlatform == Device.iOS)
            //    {
            //        Device.BeginInvokeOnMainThread(() =>
            //        {
            //            frameCounterMargin = FrameMeterReading.Margin;
            //            FrameMeterReading.Margin = new Thickness(20, -150, 20, 15);
            //        });
            //    }
            //}

            // Device.BeginInvokeOnMainThread(async () =>
            // {
            //     if (Device.RuntimePlatform == Device.Android)
            //         await Task.Delay(100);
            //     else
            //         await Task.Delay(900);
            //     var entry = (Entry)sender;
            //     if (!string.IsNullOrWhiteSpace(entry.Text))
            //     {
            //         entry.CursorPosition = 0;
            //         entry.SelectionLength = entry.Text.Length;
            //     }
            // });
        }

        private void SetCurrent(decimal counterThisMonth)
        {
            Analytics.TrackEvent("Установка предыдущих показаний" + counterThisMonth );

            var d = GetNumbers(counterThisMonth);
            //if (IntegerPoint == 5 || IntegerPoint ==0)
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    d8.Text = d[0];
            //    d7.Text = d[1];
            //    d6.Text = d[2];
            //    d5.Text = d[3];
            //    d4.Text = d[4];
            //    d3.Text = d[5];
            //    d2.Text = d[6];
            //    d1.Text = d[7];                
            //});

            //if (IntegerPoint == 6)
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        d8.Text = d[0];
            //        d7.Text = d[1];
            //        d6.Text = d[2];
            //        d41.Text = d[3];
            //        d5.Text = d[4];
            //        d4.Text = d[5];
            //        d3.Text = d[6];
            //        d2.Text = d[7];
            //        d1.Text = d[8];
            //    });
        }

        //private void D2_OnBackspace(object sender, EventArgs e)
        //{
        //    if(string.IsNullOrWhiteSpace(d2.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d1.Text = "";
        //            d1.Focus();                    
        //        });
        //    }
        //}
        //private void D3_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d3.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d2.Text = "";
        //            d2.Focus();
        //        });
        //    }
        //}
        //private void D4_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d4.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d3.Text = "";
        //            d3.Focus();
        //        });
        //    }
        //}
        //private void D5_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d5.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d4.Text = "";
        //            d4.Focus();
        //        });
        //    }
        //}
        //private void D6_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d6.Text))
        //    {
        //        if (IntegerPoint == 5|| IntegerPoint ==0)
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d5.Text = "";
        //            d5.Focus();
        //        });
        //        if (IntegerPoint == 6)
        //            Device.BeginInvokeOnMainThread(() =>
        //            {
        //                d41.Text = "";
        //                d41.Focus();
        //            });
        //    }
        //}
        //private void D7_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d7.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d6.Text = "";
        //            d6.Focus();
        //        });
        //    }
        //}
        //private void D8_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d8.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d7.Text = "";
        //            d7.Focus();
        //        });
        //    }
        //}

        //private void D41_OnBackspace(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(d41.Text))
        //    {
        //        Device.BeginInvokeOnMainThread(() =>
        //        {
        //            d5.Text = "";
        //            d5.Focus();
        //        });
        //    }
        //}

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

        void SetPrevious(decimal counterPrevMonth)
        {
            var d = GetNumbers(counterPrevMonth);
            //Prev.Text = counterPrevMonth.ToString();

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    if (IntegerPoint == 6)
            //    {
            //        d08.Text = d[0];
            //        d07.Text = d[1];
            //        d06.Text = d[2];
            //        d041.Text = d[3];
            //        d05.Text = d[4];
            //        d04.Text = d[5];
            //        d03.Text = d[6];
            //        d02.Text = d[7];
            //        d01.Text = d[8];
            //    }
            //    else
            //    {
            //        d08.Text = d[0];
            //        d07.Text = d[1];
            //        d06.Text = d[2];
            //        d05.Text = d[3];
            //        d04.Text = d[4];
            //        d03.Text = d[5];
            //        d02.Text = d[6];
            //        d01.Text = d[7];
            //    }
                
                
            //});            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    d1.Unfocus();
            //    d1.Focus();
            //});
            // Device.BeginInvokeOnMainThread(() => { Data.Focus(); });
        }

        string value1 = "";
        string value2 = "";
        string value3 = "";

        private async void ButtonClick(object sender, EventArgs e)
        {
            Analytics.TrackEvent("Попытка отправки показаний");

            try {
                string count ="";
                //var p1 = -1;
                //var p2 = -1;
                //var p3 = -1;
                //var p4 = -1;
                //var p5 = -1;
                //var p6 = -1;
                //var p7 = -1;
                //var p8 = -1;
                //var p41 = -1;

                //bool tryParse1 = int.TryParse(d1.Text,out p1);
                //bool tryParse2 = int.TryParse(d2.Text, out p2);
                //bool tryParse3 = int.TryParse(d3.Text, out p3);
                //bool tryParse4 = int.TryParse(d4.Text, out p4);
                //bool tryParse5 = int.TryParse(d5.Text, out p5);
                //bool tryParse6 = int.TryParse(string.IsNullOrWhiteSpace(d6.Text) ? "0" : d6.Text, out p6);
                //bool tryParse7 = int.TryParse(string.IsNullOrWhiteSpace(d7.Text) ? "0" : d7.Text, out p7);
                //bool tryParse8 = int.TryParse(string.IsNullOrWhiteSpace(d8.Text) ? "0" : d8.Text, out p8);
                //bool tryParse9 = int.TryParse(d41.Text == null ? "0" : d41.Text, out p41);
                //bool All = tryParse1 && tryParse2 && tryParse3 && tryParse4 && tryParse5 && tryParse6 && tryParse7 && tryParse8 && tryParse9; // Проверка на вводимые символы

                //bool isNotNull = (p1>0 || p2 >0 || p3 > 0 || p4 > 0 || p5 >0 || p6 >0 || p7 >0 || p8 > 0 || p41 >0); // Проверка что бы все клетки не были бы нулями
                //if (All || 1 == 1) //  && IntegerPoint == 6 || d41.Text == null && (IntegerPoint == 5 || IntegerPoint ==0)
                {
                    //count += d1.Text;// != "0" ? d1.Text : "";
                    //count += d2.Text;// != "0" ? d2.Text : "";
                    //count += d3.Text;// != "0" ? d3.Text : "";
                    //count += d4.Text;// != "0" ? d4.Text : "";
                    //count += d5.Text;
                    //count += IntegerPoint == 6 ? d41.Text + "," : ",";
                    //count += d6.Text;// != "0" ? d6.Text : "";
                    //count += d7.Text;// != "0" ? d7.Text : "";
                    //count += d8.Text;// != "0" ? d8.Text : "";

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
                                //d1.Text = "";
                                //d2.Text = "";
                                //d3.Text = "";
                                //d4.Text = "";
                                //d5.Text = "";
                                //d6.Text = "";
                                //d7.Text = "";
                                //d8.Text = "";
                                Data.Text = string.Empty;
                            }
                            else
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff2Name) ? AppResources.EditMetersTarif2 : AppResources.EditMetersTarif + " \"" + meter.Tariff2Name + "\""; // "изменить показания по второму тарифу";
                                if (meter.Values[0].ValueT2 != null)
                                    SetCurrentValue(Convert.ToDecimal(meter.Values[0].ValueT2));
                                //d1.Unfocus();
                                //d1.Focus();
                                // Data.Unfocus();
                                // Data.Focus();
                            }

                            //d1.Text = "";
                            //d2.Text = "";
                            //d3.Text = "";
                            //d4.Text = "";
                            //d5.Text = ""; 
                            //d6.Text = "";
                            //d7.Text = "";
                            //d8.Text = "";


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
                                //d1.Text = "";
                                //d2.Text = "";
                                //d3.Text = "";
                                //d4.Text = "";
                                //d5.Text = "";
                                //d6.Text = "";
                                //d7.Text = "";
                                //d8.Text = "";
                                Data.Text = string.Empty;
                            }
                            else
                            {
                                meterReadingName.Text = string.IsNullOrWhiteSpace(meter.Tariff3Name) ? AppResources.EditMetersTarif3 : AppResources.EditMetersTarif + " \"" + meter.Tariff3Name + "\""; //"Изменить показания по 3му тарифу";                 
                                if (meter.Values[0].ValueT3 != null)
                                    SetCurrent(Convert.ToDecimal(meter.Values[0].ValueT3));
                                //d1.Unfocus();
                                //d1.Focus();
                                // Data.Unfocus();
                                // Data.Focus();
                            }

                            //d1.Text = "";
                            //d2.Text = "";
                            //d3.Text = "";
                            //d4.Text = "";
                            //d5.Text = "";
                            //d6.Text = "";
                            //d7.Text = "";
                            //d8.Text = "";

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
                //else
                //{
                //    await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersNotNumber, "OK");
                //}
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
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            if (meter.Resource.ToLower().Contains("холодное") || meter.Resource.ToLower().Contains("хвс"))
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
                //if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
                    meter.Resource += ", м3";
            }
            else if (meter.Resource.ToLower().Contains("горячее") || meter.Resource.ToLower().Contains("гвс")|| meter.Resource.ToLower().Contains("подог")|| meter.Resource.ToLower().Contains("отопл"))
            {
                img.Source = ImageSource.FromFile("ic_heat_water");
            }
            else if (meter.Resource.ToLower().Contains("эле"))
            {
                img.Source = ImageSource.FromFile("ic_electr");

                //если это э/э и не указаны ед. измерения, в RU локали добавляем их
                if (!meter.Resource.ToLower().Contains("кВт"))
                    meter.Resource += ", кВт";
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
            //LabelseparatorFio.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
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
                //PredCount.Text = meter.Values[0].Value.ToString(CultureInfo.InvariantCulture);
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            //IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            //LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#494949"));
            FrameMeterReading.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        public async void SaveInfoAccount()
        {
            Analytics.TrackEvent("Передача показаний на сервер");
            bool rate = Preferences.Get("rate", true);

            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            if (!string.IsNullOrEmpty(value1))
            {
                var d1 = Double.Parse(value1.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

                var d2 = value2 != "" ? Double.Parse(value2.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "";
                var d3 = value3 != "" ? Double.Parse(value3.Replace(',', '.'), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "";

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

        void marginSetOnIos(CounterEntryNew d)
        {
            if(Device.RuntimePlatform==Device.iOS)
            {
                if (string.IsNullOrWhiteSpace(d.Text))
                {
                    d.Margin = new Thickness(0, 0, -15, -10 );
                }
                //else
                //{
                //    d.Margin = new Thickness(0, 0, 0, -10);
                //}
            }
            
        }

        //private void d1_Completed(object sender, EventArgs e)
        //{

        //    var entryNew = sender as CounterEntryNew;

        //    if(Xamarin.Essentials.DeviceInfo.Platform==DevicePlatform.iOS)
        //    {
        //        if(((TextChangedEventArgs)e).NewTextValue.Length>1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d1.TextChanged -= d1_Completed;
        //            d1.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d1.TextChanged += d1_Completed;
        //            });
        //        }
        //    }

        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if(string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }
                
        //        if(!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }
                
        //        d2.Unfocus();
        //        d2.Focus();                
        //    });
        //}
        //private void d2_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d2.TextChanged -= d2_Completed;
        //                d2.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d2.TextChanged += d2_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d3.Unfocus();
        //        d3.Focus();
        //    });
        //}
        //private void d3_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d3.TextChanged -= d3_Completed;
        //                d3.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d3.TextChanged += d3_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d4.Unfocus();
        //        d4.Focus();
        //    });
        //}
        //private void d4_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d4.TextChanged -= d4_Completed;
        //                d4.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d4.TextChanged += d4_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d5.Unfocus();
        //        d5.Focus();
        //    });
        //}
        //private void d5_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d5.TextChanged -= d5_Completed;
        //                d5.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d5.TextChanged += d5_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d6.Unfocus();
        //        if (DecimalPoint >= 1)
        //        {
        //            if (IntegerPoint == 5 || IntegerPoint ==0)
        //                d6.Focus();
        //            if (IntegerPoint == 6)
        //                d41.Focus();
        //        }
        //    });
        //}
        //private void d6_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d6.TextChanged -= d6_Completed;
        //                d6.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d6.TextChanged += d6_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d7.Unfocus();
        //        if (DecimalPoint >= 2)
        //            d7.Focus();
        //    });
        //}
        //private void d7_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d7.TextChanged -= d7_Completed;
        //                d7.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d7.TextChanged += d7_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d8.Unfocus();
        //        if (DecimalPoint >= 3)
        //            d8.Focus();
        //    });
        //}

        //private void d8_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
        //    {
        //        if (((TextChangedEventArgs)e).NewTextValue.Length > 1)
        //        {
        //            Device.BeginInvokeOnMainThread(() => {
        //                d8.TextChanged -= d8_Completed;
        //                d8.Text = ((TextChangedEventArgs)e).NewTextValue[1].ToString();
        //                d8.TextChanged += d8_Completed;
        //            });
        //        }
        //    }
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }
        //    });
        //}

        //private void d41_Completed(object sender, EventArgs e)
        //{
        //    var entryNew = sender as CounterEntryNew;
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (string.IsNullOrWhiteSpace(entryNew.Text))
        //        {
        //            return;
        //        }

        //        if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
        //        {
        //            entryNew.Text = "";
        //            entryNew.Focus();
        //            return;
        //        }

        //        d6.Unfocus();
        //        if (DecimalPoint >= 1)
        //            d6.Focus();
        //    });
        //}
    }
}