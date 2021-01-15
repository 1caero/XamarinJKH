using System;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.News
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewFile : DialogView
    {
        string image;
        public string Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }
        public NewFile(string img)
        {
            InitializeComponent();
            Analytics.TrackEvent("Файл новостей");

            Image = img;
            BindingContext = this;
        }

        void Close(object sender, EventArgs args)
        {
            DialogNotifier.Cancel();
        }
    }
}