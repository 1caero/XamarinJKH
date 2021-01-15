using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;
using xamarinJKH.ViewModels.DialogViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppReceiptDialogWindow : DialogView
    {
        public AppReceiptDialogWindow(AppRecieptViewModel vm)
        {
            InitializeComponent();
            Analytics.TrackEvent("Чек по заказу");

            BindingContext = vm;
        }
    }
}