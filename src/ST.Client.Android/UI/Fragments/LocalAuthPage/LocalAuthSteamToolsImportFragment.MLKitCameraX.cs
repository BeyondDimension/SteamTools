using System.Application.UI.Activities;
using System.Linq;

namespace System.Application.UI.Fragments
{
    partial class LocalAuthSteamToolsImportFragment
    {
        internal sealed class MLKitCameraX : LocalAuthSteamToolsImportFragment
        {
            protected override void OnBtnImportV2ByQRCodeClick()
            {
                BarcodeScannerActivity.StartActivity(this,
                    onScanCompleted: bytes => ViewModel!.ImportSteamPlusPlusV2(bytes));
            }

            protected override void Analyze(string filePath)
                => BarcodeScannerActivity.Analyze(filePath, (barCodes, _, _) =>
                {
                    var bytes = barCodes.Select(x => x.GetRawBytes());
                    ViewModel!.ImportSteamPlusPlusV2(bytes);
                });
        }
    }
}