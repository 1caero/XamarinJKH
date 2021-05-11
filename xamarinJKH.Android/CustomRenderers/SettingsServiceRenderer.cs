using System.Linq;
using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using xamarinJKH.Droid.CustomRenderers;
using xamarinJKH.InterfacesIntegration;

[assembly:Dependency(typeof(SettingsServiceRenderer))]
namespace xamarinJKH.Droid.CustomRenderers
{
    public class SettingsServiceRenderer:ISettingsService
    {
        Activity activity { get; set; }
        public SettingsServiceRenderer()
        {
            activity = CrossCurrentActivity.Current.Activity;
        }
        public void OpenTab(string uri)
        {
            //var uiBuilder = new HostedUIBuilder();
            
            //var manager = (CrossCurrentActivity.Current.Activity as MainActivity).hostedManager;
            //manager.BindService();
            //manager.LoadUrl("https://google.com/", uiBuilder);

            //var url = "https://www.xamarin.com";
            //var customTabsIntent = new CustomTabsIntent.Builder().Build();
            //var helper = new CustomTabActivityHelper();
            //helper.LaunchUrlWithCustomTabsOrFallback(activity, customTabsIntent, Xamarin.Essentials.AppInfo.PackageName, Uri.Parse(url), null);
            //Device.OpenUri(new System.Uri(url));
        }

        public bool IsEnabledNotification()
        {
            bool areNotificationsEnabled = NotificationManagerCompat.From(Android.App.Application.Context).AreNotificationsEnabled();
            if (areNotificationsEnabled)
            {
                var notificationChannels = NotificationManagerCompat.From(Android.App.Application.Context).NotificationChannels;
                if(notificationChannels.Any())
                {
                    return notificationChannels.First()
                        .Importance != NotificationImportance.None;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}
