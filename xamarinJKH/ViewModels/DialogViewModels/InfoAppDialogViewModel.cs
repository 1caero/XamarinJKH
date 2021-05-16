using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.DialogViewModels
{
    public class InfoAppDialogViewModel
    {
        public RequestContent _Request { get; set; }
        public Color HexColor { get; set; }
        public string PassExpiration { get; set; }
        public string SourceApp { get; set; }
        public Color ColorBlack { get; set; }
        public Command<string> Calling { get; set; }
        public bool isPass { get; set; }
        public bool isManType { get; set; }
        public bool IsCons { get; set; }
        public bool ShowDispAccepted { get; set; }
    }
    
}
