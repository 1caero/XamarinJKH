
using System.Net.Mime;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Forms;
using xamarinJKH.Droid.CustomReader;
using xamarinJKH.InterfacesIntegration;
using ZXing.Mobile;

[assembly: Dependency(typeof(QrScanningService))]  
namespace xamarinJKH.Droid.CustomReader
{
    public class QrScanningService : IQrScanningService  
    {  
        public async Task<string> ScanAsync()  
        {  
            var optionsCustom = new MobileBarcodeScanningOptions();

            var context = Android.App.Application.Context;
            var scanner = new MobileBarcodeScanner()
            {  
                TopText = "Сканируйте QR-код",  
                BottomText = "",
                CancelButtonText = "Отмена",
                
            };  
  
            var scanResult = await scanner.Scan(context,optionsCustom);  
            return scanResult?.Text;  
        }  
    }  
}