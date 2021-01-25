using AiForms.Dialogs.Abstractions;
using Syncfusion.SfCalendar.XForms;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarMonthDialog :  DialogView
    {
        public Color HexColor { get; set; }

        public CalendarMonthDialog()
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