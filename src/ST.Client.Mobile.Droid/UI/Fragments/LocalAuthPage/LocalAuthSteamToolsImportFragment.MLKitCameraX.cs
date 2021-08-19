using System.Application.UI.Activities;

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
        }
    }
}