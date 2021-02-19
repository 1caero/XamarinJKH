using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;

namespace xamarinJKH.Apps
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoPage : ContentPage
    {
        private RestClientMP server = new RestClientMP();
        private byte[] _file = null;
        private string _fileName = "";
        public PhotoPage(string idFile, string fileName, bool isConst)
        {
            InitializeComponent();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();

                    iosBarSeparator.IsVisible = true;
                    iosBarSeparator.IsEnabled = true;
                    iosBarSeparator.HeightRequest = statusBarHeight;

                    break;
                default:
                    break;
            }

            _fileName = fileName;
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
             
                _file = isConst ? await server.GetFileAPPConst(id) : await server.GetFileAPP(id);
                if (_file != null)
                {
                    Stream streamM = new MemoryStream(_file);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            ZoomImage.Source = ImageSource.FromStream(() => { return streamM; });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                        }
                    });

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
                    ViewHare.IsEnabled = true;
                });
            }).Start();
        }
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        private async void SharePhoto(object sender, EventArgs e)
        {
            if (_file != null)
            {
                ViewHare.IsEnabled = false;
                try
                {
                    if (_file != null)
                    {
                        await DependencyService.Get<IFileWorker>().SaveTextAsync(_fileName, _file);
                        FileBase fileBase = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(_fileName));
                        await Share.RequestAsync(new ShareFileRequest(AppResources.ShareBill, fileBase));
                    }
                    else
                        await DisplayAlert(null, AppResources.ErrorFileLoading, "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    ViewHare.IsEnabled = true;
                }
            }
        }
    }
}