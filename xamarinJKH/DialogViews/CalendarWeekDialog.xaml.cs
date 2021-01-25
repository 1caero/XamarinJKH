using AiForms.Dialogs.Abstractions;
using Syncfusion.SfCalendar.XForms;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarWeekDialog :  DialogView
    {
        public Color HexColor { get; set; }

        public CalendarWeekDialog()
        {
            InitializeComponent();

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
            close.Tapped += async (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);
            calendar.MonthViewSettings.DateSelectionColor = (Color)Application.Current.Resources["MainColor"]; 

            calendar.SelectionChanged += Calendar_SelectionChanged;
            calendar.Locale = new System.Globalization.CultureInfo(Application.Current.Properties["Culture"].ToString());
        }

        private void Calendar_SelectionChanged(object sender, Syncfusion.SfCalendar.XForms.SelectionChangedEventArgs e)
        {
            if (e.DateAdded.Count == 0)
            {
                calendar.SelectedRange = GetTotalWeekDays(e.DateAdded[0]);
            }
            else
            {
                if (GetWeekOfYear(e.DateAdded[0]) != GetWeekOfYear(e.DateAdded[e.DateAdded.Count - 1]))
                {
                    calendar.SelectedRange = GetTotalWeekDays(e.DateAdded[0], e.DateAdded[e.DateAdded.Count - 1]);
                }
                else
                {
                    calendar.SelectedRange = GetTotalWeekDays(e.DateAdded[0]);
                }
            }

            //start = ((SelectionRange)calendar.SelectedRange).StartDate;
        }

        DateTime start=new DateTime();
        public static int GetWeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
        }

        public SelectionRange GetTotalWeekDays(DateTime startDateRange, DateTime? endDateRange = null)
        {
            if (endDateRange == null)
            {
                var days = DayOfWeek.Monday - startDateRange.DayOfWeek;
                var startDate = startDateRange.AddDays(days);
                ObservableCollection<DateTime> dates = new ObservableCollection<DateTime>();
                for (var i = 0; i < 7; i++)
                {
                    dates.Add(startDate.Date);
                    startDate = startDate.AddDays(1);
                }

                return new SelectionRange(dates[0], dates[dates.Count - 1]);
            }
            else
            {
                var startDayOfWeek = DayOfWeek.Monday - startDateRange.DayOfWeek;
                var startDate = startDateRange.AddDays(startDayOfWeek);

                return new SelectionRange(startDate, startDate.AddDays(6));
            }
        }

        private void BtnConf_Clicked(object sender, EventArgs e)
        {
            if (calendar.SelectedRange != null)
            {
                MessagingCenter.Send<object, SelectionRange>(this, "MonitorDateStart",(SelectionRange)calendar.SelectedRange);
            }
            
            DialogNotifier.Cancel();
        }
    }
}