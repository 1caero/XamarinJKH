using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.iOS.DependencyImplementation;
using ZXing.Mobile;

[assembly: Dependency(typeof(QrCodeScanningService))]
namespace xamarinJKH.iOS.DependencyImplementation
{
    public class QrCodeScanningService :  IQrScanningService  
    {
        public async Task<string> ScanAsync()
        {
            var optionsCustom = new MobileBarcodeScanningOptions()
            {
                //TryHarder = true,                
                //UseFrontCameraIfAvailable = false
            };
            var scanner = new MobileBarcodeScanner()
            {
                TopText = "Сканируйте QR-код",
                BottomText = "",
                CancelButtonText = "Отмена",
            };

            var scanResults = await scanner.Scan(optionsCustom);

            return (scanResults != null) ? scanResults.Text : string.Empty;
        }
    }
}