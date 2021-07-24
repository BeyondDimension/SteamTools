using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using ZXing;
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
            var result = await scanner.Scan(new()
            {
                // https://github.com/Redth/ZXing.Net.Mobile/issues/808#issuecomment-835089415
                PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE },
                CameraResolutionSelector = new((List<CameraResolution> availableResolutions) =>
                {
                    CameraResolution? result = null;

                    double aspectTolerance = 0.1;
                    var displayOrientationHeight = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Height : DeviceDisplay.MainDisplayInfo.Width;
                    var displayOrientationWidth = DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ? DeviceDisplay.MainDisplayInfo.Width : DeviceDisplay.MainDisplayInfo.Height;

                    var targetRatio = displayOrientationHeight / displayOrientationWidth;
                    var targetHeight = displayOrientationHeight;
                    var minDiff = double.MaxValue;

                    foreach (var r in availableResolutions.Where(r => Math.Abs(((double)r.Width / r.Height) - targetRatio) < aspectTolerance))
                    {
                        if (Math.Abs(r.Height - targetHeight) < minDiff)
                            minDiff = Math.Abs(r.Height - targetHeight);
                        result = r;
                    }
                    return result;
                })
            });
            if (result == null) return;

            handleScanResult(result);
        }
    }
}