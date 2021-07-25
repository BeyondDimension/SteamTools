using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using ZXing.Mobile;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthSteamToolsImportFragment : BaseFragment<fragment_local_auth_import_steam_plus_plus, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_plus_plus;

        MobileBarcodeScanner? scanner;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.btnImportV1.Text = LocalAuth_SteamToolsV1Import;
                binding.btnImportV2.Text = LocalAuth_SteamToolsV2Import;
                binding.btnImportV2ByQRCode.Text = LocalAuth_SteamToolsV2Import;
                binding.tvQRCode.Text = ImportByQRCode;
                binding.tvFilePicker.Text = ImportByFilePicker;
            }).AddTo(this);

            SetOnClickListener(binding!.btnImportV1, binding.btnImportV2, binding.btnImportV2ByQRCode);

            //Create a new instance of our Scanner
            scanner = new();
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnImportV1)
            {
                ViewModel!.SppBtn_Click.Invoke();
                return true;
            }
            else if (view.Id == Resource.Id.btnImportV2)
            {
                ViewModel!.SppV2Btn_Click.Invoke();
                return true;
            }
            else if (view.Id == Resource.Id.btnImportV2ByQRCode)
            {
                scanner.StartScan(x =>
                {
                    if (!x.RawBytes.Any_Nullable()) return;
                    ViewModel!.ImportSteamPlusPlusV2(x.RawBytes!);
                });
                return true;
            }
            return base.OnClick(view);
        }
    }
}