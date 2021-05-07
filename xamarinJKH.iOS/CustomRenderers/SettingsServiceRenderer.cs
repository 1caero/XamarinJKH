using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.iOS.CustomRenderers;

[assembly: Dependency(typeof(SettingsServiceRenderer))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class SettingsServiceRenderer : ISettingsService

    {
        public bool IsEnabledNotification()
        {
            UIUserNotificationType types = UIApplication.SharedApplication.CurrentUserNotificationSettings.Types;
            if (types.HasFlag(UIUserNotificationType.Alert))
            {
                return true;
            }
            return false;
        }

        public void OpenTab(string url)
        {
            
        }
    }
}