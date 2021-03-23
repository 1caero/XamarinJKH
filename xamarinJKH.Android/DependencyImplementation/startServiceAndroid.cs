using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Xamarin.Forms;
using xamarinJKH.Droid.DependencyImplementation;
using xamarinJKH.InterfacesIntegration;
using Object = System.Object;

[assembly: Dependency(typeof(StartServiceAndroid))]
namespace xamarinJKH.Droid.DependencyImplementation
{
    public class StartServiceAndroid: IStartService
    {
        public void StartForegroundServiceCompat()
        {
            var intent = new Intent(MainActivity.Instance, typeof(MyBackgroundService));


            // if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            // {
            //     MainActivity.Instance.StartForegroundService(intent);
            // }
            // else
            // {
                MainActivity.Instance.StartService(intent);
            // }

        }
    }
    
    [Service]
    public class MyBackgroundService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();
       
        }

        private int i = 1;
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Code not directly related to publishing the notification has been omitted for clarity.
            // Normally, this method would hold the code to be run when the service is started.
            // Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            // {
            //     Console.WriteLine($@"Сервис ожил {i++} раз");
            //     return true; // runs again, or false to stop
            // });
            new Task(action: () =>
            {
                while (true)
                {
                    Console.WriteLine($@"Сервис ожил {i++} раз");
                    MessagingCenter.Send<object>(this, "SetToken");
                    Thread.Sleep(1000);
                }
            }).Start();
            //Write want you want to do here
            return StartCommandResult.Sticky;
        }
    }
}