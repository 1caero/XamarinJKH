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