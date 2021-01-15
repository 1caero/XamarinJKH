using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderView : ContentView
    {
        public static BindableProperty HeaderPicture = BindableProperty.Create("HeaderPicture", typeof(string), typeof(string));
        public static BindableProperty ShowBackButton = BindableProperty.Create("ShowBackButton", typeof(bool), typeof(bool));
        public string Picture
        {
            get => (string)GetValue(HeaderPicture);
            set
            {
                SetValue(HeaderPicture, value);
                OnPropertyChanged("Picture");
            }
        }

        public bool ShowBack
        {
            get => (bool)GetValue(ShowBackButton);
            set
            {
                SetValue(ShowBackButton, value);
                OnPropertyChanged("ShowBack");
            }
        }

        public string Phone
        {
            get => Settings.Person.companyPhone.Replace("+", "");
        }

        public string Company
        {
            get => Settings.MobileSettings.main_name;
        }

        public HeaderView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected async void GoBack(object sender, EventArgs args)
        {
            var page = this.Parent;
        }
    }
}