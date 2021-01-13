using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.AppsConst
{
    public class OptionModel:xamarinJKH.ViewModels.BaseViewModel
    {
        public string Name { get; set; }
        public string Image { get; set; }
        
        public bool HasSubTypes { get; set; }
        public List<NamedValue> SubTypes { get; set; }
        public bool IsVisible { get; set; }
        bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }
        public Command Command { get; set; }
        Dictionary<string, string> replace;
        public Dictionary<string, string> ReplaceMap
        {
            get => replace;
            set
            {
                replace = value;
                OnPropertyChanged("ReplaceMap");
            }
        }
    }
}
