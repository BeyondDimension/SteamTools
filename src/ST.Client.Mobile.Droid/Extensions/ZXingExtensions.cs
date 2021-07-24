using ZXing.Mobile;
using ZXingResult = ZXing.Result;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ZXingExtensions
    {
        // https://github.com/Redth/ZXing.Net.Mobile/blob/master/Samples/Sample.Android/MainActivity.cs

        public static async void StartScan(this MobileBarcodeScanner? scanner, Action<ZXingResult> handleScanResult)
        {
            if (scanner == null) return;

            // Tell our scanner to use the default overlay
            scanner.UseCustomOverlay = false;

            // We can customize the top and bottom text of the default overlay
            scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
            scanner.BottomText = "Wait for the barcode to automatically scan!";

            //Start scanning
            var result = await scanner.Scan();
            if (result == null) return;

            handleScanResult(result);
        }
    }
}