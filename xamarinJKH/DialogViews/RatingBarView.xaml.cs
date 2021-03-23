using System;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.DialogViews
{
    public partial class RatingBarView : DialogView
    {
        private RestClientMP server = new RestClientMP();
        public Color HexColor { get; set; }
        public RequestInfo _Request { get; set; }
        public RatingBarView()
        {
            InitializeComponent();
            
            var close = new TapGestureRecognizer();
            close.Tapped += async (s, e) =>
            {
                DialogNotifier.Cancel();
            };
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    RatingBar.Margin = new Thickness(0, 0, 0, 0);
                    RatingBar.FillColor = (Color)Application.Current.Resources["MainColor"];
                    RatingBar.HeightRequest = 35;
                    break;
                case Device.Android:
                    break;
                default:
                    break;
            }
            IconViewClose.GestureRecognizers.Add(close);
        }
        
        
        private async void CloseApp(object sender, EventArgs e)
        {
            string text = BordlessEditor.Text;
            if (!text.Equals(""))
            {
                CommonResult result = await server.CloseApp(_Request.ID.ToString(), text, RatingBar.Rating.ToString());
                if (result.Error == null)
                {
                    await ShowToast(AppResources.AppClosed);
                }
                else
                {
                    await ShowToast(result.Error);
                }
            }
            else
            {
                await ShowToast(AppResources.ErrorFillCommant);
            }
        }
        public async Task ShowToast(string title)
        {
            Toast.Instance.Show<ToastDialog>(new{Title=title,Duration=1500, ColorB = Color.Gray,  ColorT = Color.White});
            // Optionally, view model can be passed to the toast view instance.
        }
        public override void SetUp()
        {
        }
        
        public override void TearDown()
        {
            Loading.Instance.Hide();
        }
        
        
    }
}