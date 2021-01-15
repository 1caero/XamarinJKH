using AiForms.Dialogs.Abstractions;
using Xamarin.Forms.Xaml;
using xamarinJKH.ViewModels.DialogViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopListDialogView : DialogView
    {
        ShopListViewModel viewModel;
        public ShopListDialogView(int shop)
        {
            InitializeComponent();
            BindingContext = viewModel = new ShopListViewModel(DialogNotifier, shop);

            viewModel.LoadItems.Execute(null);
        }
    }
}