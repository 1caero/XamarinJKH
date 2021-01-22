using AiForms.Dialogs.Abstractions;
using Syncfusion.SfCalendar.XForms;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarMonthDialog :  DialogView
    {
        public Color HexColor { get; set; }

        public CalendarMonthDialog()
        {
            InitializeComponent();

            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);

            calendarYear.MaxDate = DateTime.Now;
            calendarYear.Locale = new System.Globalization.CultureInfo(Application.Current.Properties["Culture"].ToString());

        }


        private void calendarYear_MonthChanged(object sender, MonthChangedEventArgs e)
        {
            if (calendarYear.DisplayDate != null)
            {
                MessagingCenter.Send<object, DateTime>(this, "MonitorMonth", (DateTime)calendarYear.DisplayDate);
            }

            DialogNotifier.Cancel();
        }
    }
}