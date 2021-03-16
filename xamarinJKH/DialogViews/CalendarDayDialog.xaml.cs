using AiForms.Dialogs.Abstractions;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarDayDialog :  DialogView
    {
        public Color HexColor { get; set; }
        private bool _isMonitor;
        private Command _setDate;

        public CalendarDayDialog()
        {
            InitializeComponent();
            Init(true, null);
        }

        public CalendarDayDialog(bool isMonitor = true, Command setDate = null)
        {
            InitializeComponent();
            Init(isMonitor, setDate);
            HexColor = (Color)Application.Current.Resources["MainColor"];
            LabelDate.Text = "Выберите дату и время";
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
            close.Tapped += async (s, e) => { DialogNotifier.Cancel(); };
            IconViewClose.GestureRecognizers.Add(close);
            calendar.MonthViewSettings.DateSelectionColor = (Color) Application.Current.Resources["MainColor"];
            calendar.MonthViewSettings.TodaySelectionBackgroundColor = (Color) Application.Current.Resources["MainColor"];
            calendar.MinDate = DateTime.Now;
            calendar.Locale = new System.Globalization.CultureInfo(Application.Current.Properties["Culture"].ToString());
        }

        private bool _checkDate = false;

        private void BtnConf_Clicked(object sender, EventArgs e)
        {
            if (calendar.SelectedDate != null)
            {
                if (_isMonitor)
                {
                    MessagingCenter.Send<object, DateTime>(this, "MonitorDay", (DateTime) calendar.SelectedDate);
                    DialogNotifier.Cancel();
                }
                else
                {
                    if (!_checkDate)
                    {
                        calendar.IsVisible = false;
                        timePicker.IsVisible = true;
                        _checkDate = true;
                    }
                    else
                    {
                        DateTime calendarSelectedDate = calendar.SelectedDate.Value;
                        TimeSpan timePickerTime = timePicker.Time;
                        _setDate.Execute($"{calendarSelectedDate.Date:dd.MM.yyyy} {timePickerTime.ToString("g")}");
                        DialogNotifier.Cancel();

                    }
                    
                }
            }
            
        }
    }
}