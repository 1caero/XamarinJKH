using xamarinJKH.Server.RequestModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Threading;
using System.Linq;

namespace xamarinJKH.ViewModels.Main
{
    public class PaysPageViewModel:BaseViewModel
    {
        public ObservableCollection<AccountAccountingInfo> PayInfos { get; set; }
        public Command LoadPays { get; set; }
        public Command RefreshPays { get; set; }
        public Command RemoveAccount
        {
            get => new Command<string>(ident =>
            {
                var account_to_delete = PayInfos.FirstOrDefault(x => x.Ident == ident);
                if (account_to_delete != null)
                {
                    PayInfos.Remove(account_to_delete);

                }
            });
        }
        public CancellationTokenSource TokenSource;
        public CancellationToken Token;

        public PaysPageViewModel()
        {
            PayInfos = new ObservableCollection<AccountAccountingInfo>();
            LoadPays = new Command(() =>
            {
                IsBusy = true;
                PayInfos.Clear();
                Task.Run(async () =>
                {
                    var response = await Server.GetAccountingInfo();
                    if (response.Error != null)
                    {
                        this.ShowError(response.Error);
                    }
                    else
                    {
                        foreach (var payment in response.Data)
                        {
                            Device.BeginInvokeOnMainThread(() => PayInfos.Add(payment));
                        }
                    }
                    IsBusy = false;
                });
            });
        }
    }
}
