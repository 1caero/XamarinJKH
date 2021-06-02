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
    public partial class PinEnableCheck : PopupPage
    {
        public PinEnableCheck()
        {
            InitializeComponent();
            BindingContext = new PinEnableCheckViewModel();
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            if (NeverShowSwitch.IsToggled)
            {
                Preferences.Set("DisplayPinAdd", false);
            }

            await PopupNavigation.Instance.PopAsync();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            //Preferences.Set("PinAddNeed", true);
            
            await PopupNavigation.Instance.PopAsync();
            await PopupNavigation.Instance.PushAsync(new EnterPin());
        }
    }
    public class PinEnableCheckViewModel : BaseViewModel
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