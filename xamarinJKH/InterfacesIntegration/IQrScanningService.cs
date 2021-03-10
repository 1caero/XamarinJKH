using System.Threading.Tasks;

namespace xamarinJKH.InterfacesIntegration
{
    public interface IQrScanningService
    {
        Task<string> ScanAsync();
    }
}