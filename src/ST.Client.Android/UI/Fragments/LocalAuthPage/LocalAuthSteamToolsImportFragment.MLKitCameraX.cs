using System.Application.UI.Activities;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    partial class LocalAuthSteamToolsImportFragment
    {
        internal sealed class MLKitCameraX : LocalAuthSteamToolsImportFragment
        {
            protected override void OnBtnImportV2ByQRCodeClick()
            {
                BarcodeScannerActivity.StartActivity(this,
                    onScanCompleted: bytes =>
                    {
                        if (ViewModel!.ImportSteamPlusPlusV2B(bytes))
                        {
                            // 导入成功，关闭添加令牌页
                            Activity?.Finish();
                        }
                    });
            }

            protected override void QRCodeAnalyze(string filePath)
                => BarcodeScannerActivity.Analyze(filePath, (barCodes, _, _) =>
                {
                    var bytes = barCodes.Select(x => x.GetRawBytes());
                    if (ViewModel!.ImportSteamPlusPlusV2B(bytes))
                    {
                        // 导入成功，关闭添加令牌页
                        Activity?.Finish();
                    }
                });
        }
    }
}