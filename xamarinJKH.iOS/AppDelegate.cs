using System;
using AiForms.Dialogs;
using FFImageLoading.Forms.Platform;
using Firebase.Crashlytics;
using Foundation;
using LabelHtml.Forms.Plugin.iOS;
using MediaManager;
using Messier16.Forms.iOS.Controls;
using PanCardView.iOS;
using Plugin.FirebasePushNotification;
using Rg.Plugins.Popup;
using Syncfusion.SfAutoComplete.XForms.iOS;
using Syncfusion.SfCalendar.XForms.iOS;
using Syncfusion.SfPdfViewer.XForms.iOS;
using Syncfusion.SfPicker.XForms.iOS;
using Syncfusion.SfRangeSlider.XForms.iOS;
using Syncfusion.XForms.iOS.MaskedEdit;
using UIKit;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamEffects.iOS;
using Platform = ZXing.Net.Mobile.Forms.iOS.Platform; //using MediaManager;
//using KeyboardOverlap.Forms.Plugin.iOSUnified;

namespace xamarinJKH.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.SetFlags("RadioButton_Experimental", "IndicatorView_Experimental", "AppTheme_Experimental", "Markup_Experimental", "DragAndDrop_Experimental");

            HtmlLabelRenderer.Initialize();

            FormsMaps.Init();
            Forms.Init();

            //KeyboardOverlapRenderer.Init();
            CardsViewRenderer.Preserve();
            SfPdfDocumentViewRenderer.Init();
            SfRangeSliderRenderer.Init();
            new SfAutoCompleteRenderer();

            Dialogs.Init();
            Effects.Init(); 
            App.ScreenHeight = (int)UIScreen.MainScreen.Bounds.Height;
            App.ScreenWidth = (int)UIScreen.MainScreen.Bounds.Width;
            Messier16Controls.InitAll();
            Popup.Init();
            CachedImageRenderer.Init();

            CrossMediaManager.Current.Init();
            
            Platform.Init();

            //SimpleImageButton.SimpleImageButton.Initializator.Initializator.Init();

            SfCalendarRenderer.Init();
            SfPickerRenderer.Init();
            Syncfusion.SfImageEditor.XForms.iOS.SfImageEditorRenderer.Init();
            LoadApplication(new App());
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            Firebase.Core.App.Configure();
            SfMaskedEditRenderer.Init();
            Crashlytics.Configure();

            FirebasePushNotificationManager.Initialize(options, true);
            App.ScreenWidth2 = UIScreen.MainScreen.Bounds.Width;
            App.ScreenHeight2 = UIScreen.MainScreen.Bounds.Height;

            //SfCalendarRenderer.Init();
            

            App.DeviceId = UIDevice.CurrentDevice.IdentifierForVendor.AsString();

            UIApplication.SharedApplication.BeginBackgroundTask(() => { });

            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            FirebasePushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            FirebasePushNotificationManager.RemoteNotificationRegistrationFailed(error);

        }
        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.

            // If you disable method swizzling, you'll need to call this method. 
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            FirebasePushNotificationManager.DidReceiveMessage(userInfo);
            // Do your magic to handle the notification data
            Console.WriteLine(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }
    }
}
