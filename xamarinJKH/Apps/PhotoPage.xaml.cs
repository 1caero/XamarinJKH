using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;

namespace xamarinJKH.Apps
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoPage : ContentPage
    {
        private RestClientMP server = new RestClientMP();
        public PhotoPage(string idFile, bool isConst)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            LoadPhoto(idFile, isConst);
        }

        async void LoadPhoto(string id, bool isConst)
        {
            new Task(async () =>
            {
                byte[] stream;
                stream = isConst ? await server.GetFileAPPConst(id) : await server.GetFileAPP(id);
                if (stream != null)
                {
                    Stream streamM = new MemoryStream(stream);
                    Device.BeginInvokeOnMainThread(async () =>
                        ZoomImage.Source = ImageSource.FromStream(() => { return streamM; }));
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                    });
                }
                Device.BeginInvokeOnMainThread(async () =>
                {
                    progress.IsVisible = false;
                });
            }).Start();
        }
    }
}