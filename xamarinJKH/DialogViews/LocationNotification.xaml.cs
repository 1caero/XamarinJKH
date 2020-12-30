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
        bool openSettings;
        public bool OpenSettingsVisible
        {
            get => openSettings;
            set
            {
                openSettings = value;
                OnPropertyChanged("OpenSettingsVisible");
            }
        }
        public LocationNotification(bool settings = false)
        {
            InitializeComponent();
            OpenSettingsVisible = settings;
            BindingContext = this;
        }

        public async void AskPermission(object sender, EventArgs args)
        {
            try
            {
                var result = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                if (result[Permission.LocationWhenInUse] == PermissionStatus.Granted)
                {
                    MessagingCenter.Send<Object>(this, "LocationRequest");
                    await PopupNavigation.PopAllAsync();
                }
                else
                {
                    throw new Exception("AskPerm");
                }
            }
            catch (Exception e)
            {
                MessagingCenter.Send<Object>(this, "ShowAskPermission");
                await PopupNavigation.PopAllAsync();
            }
            
        }

        public void OpenSettings(object sender, EventArgs args)
        {
            CrossPermissions.Current.OpenAppSettings();
        }
    }
}