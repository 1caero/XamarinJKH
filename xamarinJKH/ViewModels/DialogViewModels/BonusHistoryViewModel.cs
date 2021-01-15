using System.Collections.ObjectModel;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.ViewModels.DialogViewModels
{
    public class BonusHistoryViewModel:BaseViewModel
    {
        public ObservableCollection<BonusCashFlow> History { get; set; }
        public Command LoadHistory { get; set; }

        public BonusHistoryViewModel()
        {
            History = new ObservableCollection<BonusCashFlow>();
            LoadHistory = new Command<string>(async (ident) =>
            {
                var response = await Server.GetBonusHistory(ident);
                if (response.Error == null)
                {
                    if (response.Data != null)
                        foreach (var instance in response.Data)
                        {
                            Device.BeginInvokeOnMainThread(() => History.Add(instance));
                        }
                }
            });
        }
    }
}
