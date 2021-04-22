using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.ViewModels.VideoStreamingViewModels;

namespace xamarinJKH.VideoStreaming
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CameraListPage : ContentPage
    {
        CameraListViewModel viewModel;
        public CameraListPage()
        {
            InitializeComponent();
            Analytics.TrackEvent("Список камер");
            BindingContext = viewModel = new CameraListViewModel();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    //var p = cameraStack.Padding;
                    //cameraStack.Padding= new Thickness(p.Left,p.Top+ statusBarHeight,p.Right,p.Bottom);
                    
                    break;
                default:
                    break;
            }
            MessagingCenter.Subscribe<HeaderViewStack>(this, "GoBack", async sender =>
            {
                if (Application.Current.MainPage.Navigation.ModalStack.Count > 1)
                {

                    await Navigation.PopModalAsync();
                }
                else
                {

                    await Navigation.PopAsync();
                }
            });
            viewModel.LoadCameras.Execute(null);
        }

        async void CameraSelect(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                var camera = args.CurrentSelection[0] as CameraModel;

                if (Device.RuntimePlatform == Device.Android)
                {
                    if (Navigation.ModalStack.FirstOrDefault(x => x is CameraPage) == null)
                        if (camera != null)
                            await Navigation.PushModalAsync(new CameraPage(camera.Url, camera.Address));
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => await Launcher.OpenAsync(camera.Url));

                }
                (sender as CollectionView).SelectedItem = null;
            }
            catch
            {

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            headerStack.IsAppTheme = !headerStack.IsAppTheme;
        }
    }
}