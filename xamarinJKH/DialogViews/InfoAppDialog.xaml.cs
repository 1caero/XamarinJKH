using System;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.DialogViews
{
    public partial class InfoAppDialog : DialogView
    {
        public double width { get; set; }
        public Color HexColor { get; set; }
        public string SourceApp { get; set; }
        public RequestContent _Request { get; set; }
        public Command Call { get; set; }
        public bool isPass { get; set; } = false;
        public bool isManType { get; set; } = false;


        public bool IsCons { get; set; } = false;


        public bool ShowDispAccepted { get; set; } = false;
        //{
        //    get
        //    {
        //        if (_Request != null)
        //            return !string.IsNullOrEmpty(_Request.AcceptedDispatcher);
        //        return false;
        //    }
        //    set { value = false; }
        //}

        public InfoAppDialog()
        {
            InitializeComponent();
            var close = new TapGestureRecognizer();
            close.Tapped += (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            IconViewClose.GestureRecognizers.Add(close);
            View.WidthRequest = App.ScreenWidth;
            Analytics.TrackEvent("Инфо о заявке ");
            //IconViewPhone.IsVisible = Settings.Person.IsDispatcher;
            Frame.SetAppThemeColor(Frame.BorderColorProperty, (Color)Application.Current.Resources["MainColor"], Color.White);
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        Frame.Margin = new Thickness(15,113,15,15); 
                    }else if (Math.Abs(or - 0.55) < 0.02)
                    {
                        Frame.Margin = new Thickness(15,100,15,15); 
                    }
                    break;
            }
                        
            BindingContext = this;
        }

        

        public override void SetUp()
        {
            // called each opening dialog
        }

        public override void TearDown()
        {
        }

        public override void RunPresentationAnimation()
        {
            // define opening animation
        }

        public override void RunDismissalAnimation()
        {
            // define closing animation
        }

        public override void Destroy()
        {
            // define clean up process.
        }

        void Handle_OK_Clicked(object sender, EventArgs e)
        {
            // send complete notification to the dialog.
            DialogNotifier.Complete();
        }

        void Handle_Cancel_Clicked(object sender, EventArgs e)
        {
            // send cancel notification to the dialog.
            DialogNotifier.Cancel();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var copyToClipboardInfo = $"{AppResources.Theme}: {_Request.Text}\r\n" +
                $"{_Request.Added}\r\n" +
                $"{AppResources.Type}: {_Request.TypeName} {_Request._MalfunctionType}\r\n";
            if (_Request.HasPass)
                copyToClipboardInfo += $"{AppResources.TypePass}: {_Request.TextPassIsConstant}\r\n"+
                    $"{AppResources.Validity}: {_Request.PassExpiration}\r\n";

            if (!string.IsNullOrWhiteSpace(_Request.PerofmerName))
                copyToClipboardInfo += $"{AppResources.Executor}: {_Request.PerofmerName}\r\n";
            if (!string.IsNullOrWhiteSpace(_Request.SourceType))
                copyToClipboardInfo += $"{AppResources.ASource}: {_Request.SourceType}\r\n";
            if (!string.IsNullOrWhiteSpace(_Request.RequestTerm))
                copyToClipboardInfo += $"{AppResources.PeriodExecution}: {_Request.RequestTerm}\r\n";
            if (!string.IsNullOrWhiteSpace(_Request.PriorityName))
                copyToClipboardInfo += $"{AppResources.Priority}: {_Request.PriorityName}\r\n";



            copyToClipboardInfo += $"{AppResources.Debt}: {_Request.Debt}\r\n" +
                $"{AppResources.Adress}: {_Request.Address}\r\n";
            if(IsCons)
            {
                copyToClipboardInfo += $"{AppResources.FIO}: {_Request.AuthorName}\r\n";
            }
            if (ShowDispAccepted)
            {
                copyToClipboardInfo += $"{AppResources.FIOConsAcceptOrder}: {_Request.AcceptedDispatcher}\r\n";
            }

            if (IsCons)
            {
                copyToClipboardInfo += $"{AppResources.Phone}: {_Request.Phone}\r\n";
            }

            if (_Request.IsPaid)
            {
                copyToClipboardInfo += $"{AppResources.StatusOrder}: {_Request.PaidRequestStatus}\r\n"+
                    $"{AppResources.CheckCode}: {_Request.PaidRequestCompleteCode}\r\n"+
                    $"{_Request.Status}\r\n";
            }

            if (isPass)
            {
                if (isManType)
                {
                    copyToClipboardInfo += $"{_Request.PassInfo.CategoryName}: {_Request.PassInfo.FIO}\r\n";
                }
                else
                {
                    copyToClipboardInfo += $"{_Request.PassInfo.CategoryName}: {_Request.PassInfo.VehicleMark}\r\n"+
                        $"{_Request.PassInfo.VehicleNumber}\r\n";
                }

            }

            await Xamarin.Essentials.Clipboard.SetTextAsync(copyToClipboardInfo);
        }
    }
}