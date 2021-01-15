using AiForms.Dialogs.Abstractions;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToastDialog : ToastView
    {
        public ToastDialog()
        {
            InitializeComponent();
        }
        
        // define appearing animation
        public override void RunPresentationAnimation() {}

        // define disappearing animation
        public override void RunDismissalAnimation() {}

        // define clean up process.
        public override void Destroy() {}
    }
}