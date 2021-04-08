using AiForms.Dialogs.Abstractions;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarDayDialog :  DialogView
    {
        private readonly Page _page;
        public Color HexColor { get; set; }
        private bool _isMonitor;
        private Command _setDate;

        public CalendarDayDialog()
        {
            InitializeComponent();
            Init(true, null);
        }

        public CalendarDayDialog(bool isMonitor = true, Command setDate = null, Page page = null)
        {
            _page = page;
            InitializeComponent();
            Init(isMonitor, setDate);
            HexColor = (Color)Application.Current.Resources["MainColor"];
            LabelDate.Text = AppResources.SelectDateTime;
            timePicker.IsVisible = true;
            BindingContext = this;
        }

        private void Init(bool isMonitor, Command setDate)
        {
            _isMonitor = isMonitor;
            _setDate = setDate;
            DialogView.WidthRequest = App.ScreenWidth;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    if (DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Width > 2)
                        Frame.Margin = new Thickness(15, statusBarHeight * 2, 15, statusBarHeight * 2);
                    else
                        Frame.Margin = new Thickness(15, statusBarHeight, 15, statusBarHeight);
                    break;
                default:
                    break;
            }
            var close = new TapGestureRecognizer();
            timePicker.Time = DateTime.Now.TimeOfDay;
            close.Tapped += async (s, e) => { DialogNotifier.Cancel(); };
            IconViewClose.GestureRecognizers.Add(close);
            calendar.MonthViewSettings.DateSelectionColor = (Color) Application.Current.Resources["MainColor"];
            calendar.MonthViewSettings.TodaySelectionBackgroundColor = (Color) Application.Current.Resources["MainColor"];
            calendar.MinDate = DateTime.Now;
            calendar.Locale = new System.Globalization.CultureInfo(Application.Current.Properties["Culture"].ToString());
        }

        private bool _checkDate = false;

        private async void BtnConf_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("проверка !!!!! !!!" );
            
            try
            {
                var d0 = 1 - 1;
                //var c = 1 / d0;
                if (calendar.SelectedDate != null)
                {
                    if (_isMonitor)
                    {
                        MessagingCenter.Send<object, DateTime>(this, "MonitorDay", (DateTime)calendar.SelectedDate);
                        DialogNotifier.Cancel();
                    }
                    else
                    {
                        // if (!_checkDate)
                        // {
                        //     Device.BeginInvokeOnMainThread(() =>
                        //     {
                        //         calendar.IsVisible = false;
                        //         timePicker.IsVisible = true;
                        //     });
                        //
                        //     _checkDate = true;
                        // }
                        // else
                        // {
                            TimeSpan timePickerTime = timePicker.Time;
                            DateTime calendarSelectedDate = calendar.SelectedDate.Value;
                            DateTime select = new DateTime(
                                calendarSelectedDate.Year,
                                calendarSelectedDate.Month,
                                calendarSelectedDate.Day,
                                timePickerTime.Hours,
                                timePickerTime.Minutes,
                                timePickerTime.Seconds,
                                timePickerTime.Seconds,
                                calendarSelectedDate.Kind);

                            DateTime universalTime = @select.ToUniversalTime();

                            Tuple<string, string> dateStr =
                                new Tuple<string, string>($"{universalTime:yyyy-MM-dd HH:mm:ss}",
                                    $"{select:dd.MM.yyyy HH:mm}");

                            //if (_setDate != null)
                            //    _setDate.Execute(dateStr);
                            //else
                            DateTime current = DateTime.Now;
                            if (current > select)
                            {
                                if (_page != null)
                                    await _page.DisplayAlert(AppResources.ErrorTitle,
                                        "Время должно быть больше текущего", "OK");
                                return;
                            }
                            {
                                MessagingCenter.Send<object, Tuple<string, string>>(this, "SetDateTimePass",
                                dateStr);
                            }

                            //_setDate.Execute(dateStr);

                            DialogNotifier.Cancel();

                        // }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ОШИБКА: " + ex.ToString());
                if (Settings.Person.Phone.Contains("79788262609"))
                {
                    foreach (var t in mainStack.Children)
                        t.IsVisible = false;
                    errorView.IsVisible = true;
                    errorText.Text = ex.ToString();
                }
                else
                {
                    throw; 
                }
            }
           
            
        }
    }
}