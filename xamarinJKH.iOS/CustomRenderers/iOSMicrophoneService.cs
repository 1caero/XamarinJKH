using AVFoundation;
using System.Threading.Tasks;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.iOS.CustomRenderers;

[assembly: Dependency(typeof(iOSMicrophoneService))]
namespace xamarinJKH.iOS.CustomRenderers
{
    public class iOSMicrophoneService: IMicrophoneService
    {
        TaskCompletionSource<bool> tcsPermissions;

        public Task<bool> GetPermissionAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();
            RequestMicPermission();
            return tcsPermissions.Task;
        }

        public void OnRequestPermissionResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }

        void RequestMicPermission()
        {
            var session = AVAudioSession.SharedInstance();
            session.RequestRecordPermission((granted) =>
            {
                tcsPermissions.TrySetResult(granted);
            });
        }
    }
}