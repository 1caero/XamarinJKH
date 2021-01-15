using System;
using System.Linq;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.ViewModels.Shop;

namespace xamarinJKH.Shop
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopPageNew : ContentPage
    {
        ShopViewModel viewModel { get; set; }
        public ShopPageNew(AdditionalService select)
        {
            InitializeComponent();
            Analytics.TrackEvent("Магазин " + select.Name);

            if(Device.RuntimePlatform==Device.iOS)
            {
                int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
            }

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await Navigation.PushAsync(new AppPage());};
            LabelTech.GestureRecognizers.Add(techSend);

            BindingContext = viewModel = new ShopViewModel(select, this.Navigation);
            viewModel.LoadGoods.Execute(null);

        }

        async void Back(object sender, EventArgs args)
        {
            try
            {
                _ = await Navigation.PopAsync();
            }
            catch { }
        }

        private void CollectionView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            try
            {
                var index = e.FirstVisibleItemIndex;
                var delta = e.HorizontalDelta;
                if (index >= 0)
                {
                    Analytics.TrackEvent($"index={index}, viewModel.Categories.Count={viewModel.Categories.Count}, e.HorizontalOffset={e.HorizontalOffset}  ");
                    if (index + 1 <= viewModel.Categories.Count - 1 && e.HorizontalOffset < 160)
                    {
                        viewModel.SelectedCategory = viewModel.Categories[index];
                    }
                    else
                    {
                        viewModel.SelectedCategory = viewModel.Categories[e.LastVisibleItemIndex];
                    }
                }
                if (viewModel.SelectedCategory == " ")
                {                    
                    var id = viewModel.Categories.ToList().IndexOf(" ");
                    Analytics.TrackEvent($"Выбрана ' '(пустая) категория, id={id}");
                    if (id > 0)
                        (sender as CollectionView).ScrollTo(id - 1);
                }
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
            }
            
        }
    }
}