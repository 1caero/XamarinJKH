using System.Collections.Generic;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.ViewModels;

namespace xamarinJKH.AppsConst
{
    public class OptionModel : BaseViewModel
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string ID { get; set; }

        public bool HasSubTypes { get; set; }
        public List<NamedValue> SubTypes { get; set; }
        public bool IsVisible { get; set; }

        bool selected;

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

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

        private Color _selectedColor = Color.FromHex(Application.Current.RequestedTheme == OSAppTheme.Dark
            ? "#FFFFFF"
            : "#8D8D8D");

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged("SelectedColor");
            }
        }

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