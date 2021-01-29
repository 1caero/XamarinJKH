using System.Threading.Tasks;

namespace xamarinJKH.InterfacesIntegration
{
    public interface IMicrophoneService
    {
        Task<bool> GetPermissionAsync();
        void OnRequestPermissionResult(bool isGranted);
    }
}