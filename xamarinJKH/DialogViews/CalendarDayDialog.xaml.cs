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

        public CalendarDayDialog()
        {
            InitializeComponent();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    if (DeviceDisplay.MainDisplayInfo.Height/DeviceDisplay.MainDisplayInfo.Width >2 )
                        Frame.Margin = new Thickness(15, statusBarHeight * 2, 15, statusBarHeight * 2);
                    else
                        Frame.Margin = new Thickness(15, statusBarHeight, 15, statusBarHeight );
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
            calendar.MonthViewSettings.TodaySelectionBackgroundColor= (Color)Application.Current.Resources["MainColor"];

            calendar.Locale = new System.Globalization.CultureInfo(Application.Current.Properties["Culture"].ToString());

        }



        private void BtnConf_Clicked(object sender, EventArgs e)
        {
            if (calendar.SelectedDate != null)
            {
                MessagingCenter.Send<object, DateTime>(this, "MonitorDay", (DateTime)calendar.SelectedDate);
            }
            
            DialogNotifier.Cancel();
        }
    }
}