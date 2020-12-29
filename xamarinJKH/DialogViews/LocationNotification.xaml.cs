using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Pages;
using Xamarin.Essentials;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;
using Rg.Plugins.Popup.Services;

namespace xamarinJKH.DialogViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationNotification : PopupPage
    {
        public LocationNotification()
        {
            InitializeComponent();
        }

        public async void AskPermission(object sender, EventArgs args)
        {
            await PopupNavigation.PopAllAsync();
            await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
            
        }
    }
}