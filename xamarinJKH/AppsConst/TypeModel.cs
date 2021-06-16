using System.Collections.Generic;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.ViewModels;

namespace xamarinJKH.AppsConst
{
    public class TypeModel:BaseViewModel
    {
        
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Image { get; set; }
        public List<RequestType> SubTypes { get; set; }
        public int ID { get; set; }
        public bool HasSubTypes { get; set; }
        
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