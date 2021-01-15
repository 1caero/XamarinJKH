using System;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;
using xamarinJKH.ViewModels.DialogViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BonusHistoryDialogView : DialogView
    {
        BonusHistoryViewModel viewModel { get; set; }
        public BonusHistoryDialogView(string ident)
        {
            InitializeComponent();
            Analytics.TrackEvent("Диалог истории бонусов");

            BindingContext = viewModel = new BonusHistoryViewModel();
            viewModel.LoadHistory.Execute(ident);
        }

        void Close(object sender, EventArgs args)
        {
            this.DialogNotifier.Cancel();
        }
    }
}