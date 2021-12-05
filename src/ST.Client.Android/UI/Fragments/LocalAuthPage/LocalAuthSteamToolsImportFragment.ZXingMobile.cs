//using Android.Views;
//using ZXing.Mobile;

//// ReSharper disable once CheckNamespace
//namespace System.Application.UI.Fragments
//{
//    partial class LocalAuthSteamToolsImportFragment
//    {
//        internal sealed class ZXing : LocalAuthSteamToolsImportFragment
//        {
//            MobileBarcodeScanner? scanner;

//            public override void OnCreateView(View view)
//            {
//                base.OnCreateView(view);

//                //Create a new instance of our Scanner
//                scanner = new();
//            }

//            protected override async void OnBtnImportV2ByQRCodeClick()
//            {
//                var result = await scanner.StartScanAsync();
//                if (result == null) return;
//                if (!result.RawBytes.Any_Nullable()) return;
//                ViewModel!.ImportSteamPlusPlusV2(result.RawBytes!);
//            }
//        }
//    }
//}