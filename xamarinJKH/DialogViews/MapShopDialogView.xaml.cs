using System;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapShopDialogView : DialogView
    {
        public AdditionalService Service { get; set; }
        string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        string _image;
        public string Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public Command Open { get; set; }
        public MapShopDialogView(AdditionalService service)
        {
            InitializeComponent();
            DialogView.WidthRequest = App.ScreenWidth;
            Analytics.TrackEvent("Диалог выбора товара из магазина");
            Service = service;
            if (Service != null)
            {
                Name = Service.Name;
                Image = RestClientMP.SERVER_ADDR + "/AdditionalServices/Logo/" + Service.ID.ToString();
            }

            Open = new Command(async () =>
            {
                DialogNotifier.Cancel();
                AdditionalService select = Service;
                MessagingCenter.Send<Object, AdditionalService>(this, "OpenService", select);
            });
            BindingContext = this;
        }
    }
}