using AiForms.Dialogs.Abstractions;
using Syncfusion.SfCalendar.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarDayDialog :  DialogView
    {
        public Color HexColor { get; set; }

        public CalendarDayDialog()
        {
            InitializeComponent();

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