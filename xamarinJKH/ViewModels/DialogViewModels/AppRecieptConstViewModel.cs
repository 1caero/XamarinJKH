using AiForms.Dialogs.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.DialogViewModels
{
    public class AppRecieptConstViewModel:BaseViewModel
    {
        public ObservableCollection<RequestsReceiptItem> ReceiptItems { get; set; }
        public Command Delete { get; set; }
        public Command Edit { get; set; }
        public Command Update { get; set; }
        public int ID { get; set; }
        decimal price { get; set; }
        public decimal Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        readonly IDialogNotifier Notifier;

        private string GetOrderStr(ObservableCollection<RequestsReceiptItem> requestsReceiptItems)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"{AppResources.OrderCorrected}\n");
            int i = 0;
            foreach (var each in requestsReceiptItems)
            {
                decimal? eachQuantity = each.Price * each.Quantity;
                stringBuilder.Append(i + 1)
                    .Append(") ").Append(each.Name).Append(AppResources.Amount)
                    .Append(each.Quantity).Append(AppResources.Amount2).Append(AppResources.PriceLowerCase)
                    .Append(eachQuantity).Append(AppResources.Currency)
                    .Append("\n");
                i++;
            }

            stringBuilder.Append($"{AppResources.TotalPrice} ").Append(ReceiptItems.Select(x => x.Price * x.Quantity).Sum())
                .Append($"{AppResources.Currency}\n");
            
            return stringBuilder.ToString();
        }
        public AppRecieptConstViewModel(List<RequestsReceiptItem> items, int id, IDialogNotifier dialog)
        {
            ReceiptItems = new ObservableCollection<RequestsReceiptItem>();
            ID = id;
            if (items != null)
                foreach (var item in items)
                {
                    ReceiptItems.Add(item);
                }

            Notifier = dialog;

            var total = ReceiptItems.Select(x => x.Price * x.Quantity).Sum();
            Price = Convert.ToDecimal(total);

            Delete = new Command<RequestsReceiptItem>(item =>
            {
                if (item != null)
                {
                    ReceiptItems.Remove(item);
                }
                var total = ReceiptItems.Select(x => x.Price * x.Quantity).Sum();
                Price = Convert.ToDecimal(total);
            });

            Edit = new Command<RequestsReceiptItem>(item => 
            {
            });

            Update = new Command(async () =>
            {
                var response = await Server.UpdateReceipt(new RequestsReceiptItemsList(this.ID, ReceiptItems));
                if(ReceiptItems.Count > 0)
                {
                    CommonResult result;
                    result = await Server.AddMessageConst(GetOrderStr(ReceiptItems), this.ID.ToString(), false);
                    MessagingCenter.Send<Object>(this, "RefreshApp");

                }
                MessagingCenter.Send<Object>(this, "RefreshAppList");
                Notifier.Cancel();
            });

            
            MessagingCenter.Subscribe<Object>(this, "UpdatePrice", sender =>
            {
                var total = ReceiptItems.Select(x => x.Price * x.Quantity).Sum();
                Price = Convert.ToDecimal(total);
            });

            MessagingCenter.Unsubscribe<Object, List<Goods>>(this, "AddItems");
            MessagingCenter.Subscribe<Object, List<Goods>>(this, "AddItems", (sender, args) =>
            {
                if (args != null)
                {
                    foreach (var item in args)
                    {
                        var receipt_item = new RequestsReceiptItem { Name = item.Name, Price = item.Price, Quantity = item.ColBusket };
                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            var existing = ReceiptItems.FirstOrDefault(x => x.Name == item.Name);
                            if (existing == null)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    ReceiptItems.Add(receipt_item);
                                    var total = ReceiptItems.Select(x => x.Price * x.Quantity).Sum();
                                    Price = Convert.ToDecimal(total);
                                });
                            }
                            else
                            {
                                existing.Quantity += item.ColBusket;
                                existing.Amount = Convert.ToDecimal(existing.Quantity * existing.Price);
                            }

                        }
                        
                    }
                }
                var total = ReceiptItems.Select(x => x.Price * x.Quantity).Sum();
                Price = Convert.ToDecimal(total);
            });
        }
    }
}
