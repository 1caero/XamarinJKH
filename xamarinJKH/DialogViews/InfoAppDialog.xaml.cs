using System;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.DialogViewModels;

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
                case Device.iOS:

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
            var bk = (InfoAppDialogViewModel) BindingContext;

            var copyToClipboardInfo = $"{AppResources.Theme}: {bk._Request.Text}\r\n" +
                $"{bk._Request.Added}\r\n" +
                $"{AppResources.Type}: {bk._Request.TypeName} {bk._Request._MalfunctionType}\r\n";
            if (bk._Request.HasPass)
                copyToClipboardInfo += $"{AppResources.TypePass}: {bk._Request.TextPassIsConstant}\r\n"+
                    $"{AppResources.Validity}: {bk._Request.PassExpiration}\r\n";

            if (!string.IsNullOrWhiteSpace(bk._Request.PerofmerName))
                copyToClipboardInfo += $"{AppResources.Executor}: {bk._Request.PerofmerName}\r\n";
            if (!string.IsNullOrWhiteSpace(bk._Request.SourceType))
                copyToClipboardInfo += $"{AppResources.ASource}: {bk._Request.SourceType}\r\n";
            if (!string.IsNullOrWhiteSpace(bk._Request.RequestTerm))
                copyToClipboardInfo += $"{AppResources.PeriodExecution}: {bk._Request.RequestTerm}\r\n";
            if (!string.IsNullOrWhiteSpace(bk._Request.PriorityName))
                copyToClipboardInfo += $"{AppResources.Priority}: {bk._Request.PriorityName}\r\n";



            copyToClipboardInfo += $"{AppResources.Debt}: {bk._Request.Debt}\r\n" +
                $"{AppResources.Adress}: {bk._Request.Address}\r\n";
            if(bk.IsCons)
            {
                copyToClipboardInfo += $"{AppResources.FIO}: {bk._Request.AuthorName}\r\n";
            }
            if (bk.ShowDispAccepted)
            {
                copyToClipboardInfo += $"{AppResources.FIOConsAcceptOrder}: {bk._Request.AcceptedDispatcher}\r\n";
            }

            if (bk.IsCons)
            {
                copyToClipboardInfo += $"{AppResources.Phone}: {bk._Request.Phone}\r\n";
            }

            if (bk._Request.IsPaid)
            {
                copyToClipboardInfo += $"{AppResources.StatusOrder}: {bk._Request.PaidRequestStatus}\r\n"+
                    $"{AppResources.CheckCode}: {bk._Request.PaidRequestCompleteCode}\r\n"+
                    $"{bk._Request.Status}\r\n";
            }

            if (bk.isPass)
            {
                if (bk.isManType)
                {
                    copyToClipboardInfo += $"{bk._Request.PassInfo.CategoryName}: {bk._Request.PassInfo.FIO}\r\n";
                }
                else
                {
                    copyToClipboardInfo += $"{bk._Request.PassInfo.CategoryName}: {bk._Request.PassInfo.VehicleMark}\r\n"+
                        $"{bk._Request.PassInfo.VehicleNumber}\r\n";
                }

            }

            await Xamarin.Essentials.Clipboard.SetTextAsync(copyToClipboardInfo);
            Toast.Instance.Show<ToastDialog>(new { Title = AppResources.InfoCopied, Duration = 1900, ColorB = Color.Gray, ColorT = Color.White });
        }

    }
}