using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.ViewModels;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PushEnableCheck : PopupPage
    {
        public PushEnableCheck()
        {
            InitializeComponent();
            BindingContext = new PushEnableCheckViewModel();
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            if (NeverShowSwitch.IsToggled)
            {
                Preferences.Set("DisplayNotification", false);
            }

            await PopupNavigation.Instance.PopAsync();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            Xamarin.Essentials.AppInfo.ShowSettingsUI();
            await PopupNavigation.Instance.PopAsync();
        }
    }
    public class PushEnableCheckViewModel : BaseViewModel
    {
        public Color HexColor
        {
            get
            {
                try
                {
                    return (Color)Application.Current.Resources["MainColor"];
                }
                catch
                {
                    return Color.Blue;
                }
            }

        }
    }
}