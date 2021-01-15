using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.DialogViewModels
{
    public class AppRecieptViewModel:BaseViewModel
    {
        public ObservableCollection<RequestsReceiptItem> ReceiptItems { get; set; }
        decimal price;
        public decimal Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
        public AppRecieptViewModel(List<RequestsReceiptItem> items)
        {
            ReceiptItems = new ObservableCollection<RequestsReceiptItem>();
            foreach (var item in items)
            {
                Device.BeginInvokeOnMainThread(() => ReceiptItems.Add(item));
            }
            var total = items.Select(x => x.Price * x.Quantity).Sum();
            Price = Convert.ToDecimal(total);
        }
    }
}
