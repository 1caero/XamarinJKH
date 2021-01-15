using System.Collections.ObjectModel;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.Shop
{
    public class BasketPageViewModel:BaseViewModel
    {
        public ObservableCollection<Goods> BasketItems { get; set; }
        public BasketPageViewModel(ObservableCollection<Goods> Items)
        {
            BasketItems = Items;
        }
    }
}
