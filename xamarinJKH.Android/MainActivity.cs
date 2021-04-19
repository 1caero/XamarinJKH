using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Context = Android.Content.Context;
using Intent = Android.Content.Intent;
using Android.Content.PM;
using Android.Net;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Plugin.CurrentActivity;
using Plugin.FirebasePushNotification;
using Plugin.Media;
using Xamarin.Forms;
using xamarinJKH.Utils;

using Firebase.Iid;
using xamarinJKH.Droid.CustomReader;
using xamarinJKH.InterfacesIntegration;

using Android.Speech;
using xamarinJKH.DialogViews;
using xamarinJKH.Droid.CustomRenderers;
using Settings = Android.Provider.Settings;
using Toast = AiForms.Dialogs.Toast;
using Plugin.Fingerprint;

namespace xamarinJKH.Droid
{
    [Activity(Label = "Тихая Гавань",  Icon = "@drawable/icon_login", HardwareAccelerated = true,Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IMessageSender
    {
        IMicrophoneService micService;
        internal static MainActivity Instance { get; private set; }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
          
        
            global::Xamarin.Forms.Forms.SetFlags("RadioButton_Experimental", "AppTheme_Experimental", "Markup_Experimental", "DragAndDrop_Experimental");
            XamEffects.Droid.Effects.Init();
            AiForms.Dialogs.Dialogs.Init(this);
            App.ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density);
            App.ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);
            App.version = Build.VERSION.Sdk;
            App.model = Build.Model;
            Messier16.Forms.Android.Controls.Messier16Controls.InitAll();
            Rg.Plugins.Popup.Popup.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            await CrossMedia.Current.Initialize();
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera);
            ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage);
            //CreateNotificationChannel();
            
            CrossFingerprint.SetCurrentActivityResolver(() => Xamarin.Essentials.Platform.CurrentActivity);

            Fabric.Fabric.With(this, new Crashlytics.Crashlytics());
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            DependencyService.Register<OpenAppAndroid>();
            var width = Resources.DisplayMetrics.WidthPixels;
            var height = Resources.DisplayMetrics.HeightPixels;
            var density = Resources.DisplayMetrics.Density;

            App.ScreenWidth2 = (width - 0.5f) / density;
            App.ScreenHeight2 = (height - 0.5f) / density;
            LoadApplication(new App());
            FirebasePushNotificationManager.ProcessIntent(this, Intent);
            FirebasePushNotificationManager.DefaultNotificationChannelImportance = NotificationImportance.High;
            GetId();
            micService = DependencyService.Resolve<IMicrophoneService>();

        }

        void GetId()
        {
            if (!string.IsNullOrWhiteSpace(App.DeviceId))
                return;
            App.DeviceId = Android.OS.Build.Serial;
            if (string.IsNullOrWhiteSpace(App.DeviceId) || App.DeviceId == Build.Unknown || App.DeviceId == "0")
            {
                try
                {
                    var context = Android.App.Application.Context;
                    App.DeviceId = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Warn("DeviceInfo", "Unable to get id: " + ex.ToString());
                }
            }
            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            switch (requestCode)
            {
                case AndroidMicrophoneService.RecordAudioPermissionCode:
                    micService.OnRequestPermissionResult(grantResults?.Length > 0 && grantResults?[0] == Permission.Granted);
                    break;
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel("occ_test_notification_channel",
                                                  "OCC",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };


            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        //[Service]
        //[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
        //public class MyFirebaseIIDService : FirebaseInstanceIdService
        //{
        //    const string TAG = "MyFirebaseIIDService";
        //    public override void OnTokenRefresh()
        //    {
        //        var refreshedToken = FirebaseInstanceId.Instance.Token;
        //        SendRegistrationToServer(refreshedToken);
        //    }
        //    void SendRegistrationToServer(string token)
        //    {
        //        App.token = token;
        //    }
        //}

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            FirebasePushNotificationManager.ProcessIntent(this, intent);
        }
        
        private readonly int VOICE = 10;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            if (requestCode == VOICE)
            {
                if (resultCode == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];
                        MessagingCenter.Send<IMessageSender, string>(this, "STT", textInput);
                    }
                    else
                    {
                        MessagingCenter.Send<IMessageSender, string>(this, "STT", "No input");
                    }

                }
            }


            base.OnActivityResult(requestCode, resultCode, data);
        }
        
    }
}