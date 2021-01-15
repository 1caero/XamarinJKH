using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageSaldoPage : ContentPage
    {
        public bool hex { get; set; }
        RestClientMP server = new RestClientMP();
        public string Period { get; set; }
        BillInfo _billInfo = new BillInfo();
        private string _filename = "";
        private byte[] _file = null;

        public ImageSaldoPage(BillInfo bill)
        {
            Period = bill.Period;
            _billInfo = bill;
            _filename = _billInfo.Period + "_" + _billInfo.Ident.Replace("/", "")
                .Replace("\\", "") + ".pdf";
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                Pancake2.HeightRequest = statusBarHeight; // new Thickness(0, statusBarHeight, 0, 0);
            }


            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            LoadPdf();
            NavigationPage.SetHasNavigationBar(this, false);
            BindingContext = this;
        }


        async void LoadPdf()
        {
            ViewPrint.IsEnabled = false;
            ViewHare.IsEnabled = false;
            new Task(async () =>
            {
                byte[] stream;
                stream = await server.DownloadFileAsync(_billInfo.ID.ToString(), 1);
                if (stream != null)
                {
                    Stream streamM = new MemoryStream(stream);
                    Device.BeginInvokeOnMainThread(async () =>
                        Image.Source = ImageSource.FromStream(() => { return streamM; }));
                    _file = await server.DownloadFileAsync(_billInfo.ID.ToString());
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
                    ViewPrint.IsEnabled = true;
                    ViewHare.IsEnabled = true;
                    progress.IsVisible = false;
                });
            }).Start();
        }

        async void ShareBill(object sender, EventArgs args)
        {
            ViewHare.IsEnabled = false;
            try
            {
                if (_file != null)
                {
                    await DependencyService.Get<IFileWorker>().SaveTextAsync(_filename, _file);
                    FileBase fileBase = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(_filename));
                    await Share.RequestAsync(new ShareFileRequest(AppResources.ShareBill, fileBase));
                }
                else
                    await DisplayAlert(null, AppResources.ErrorFileLoading, "OK");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                ViewHare.IsEnabled = true;
            }
        }

        async void Print(object sender, EventArgs args)
        {
            ViewPrint.IsEnabled = false;
            try
            {
                if (_file != null)
                    DependencyService.Get<IPrintManager>().SendFileToPrint(_file);
                else
                    await DisplayAlert(null, AppResources.ErrorFileLoading, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(null, AppResources.ErrorReboot, "ОК");
            }
            finally
            {
                ViewPrint.IsEnabled = true;
            }
        }
    }
}