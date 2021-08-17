using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Fragments
{
    internal abstract partial class LocalAuthSteamToolsImportFragment : BaseFragment<fragment_local_auth_import_steam_plus_plus, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_plus_plus;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                if (binding == null) return;
                binding.btnImportV1.Text = LocalAuth_SteamToolsV1Import;
                binding.btnImportV2.Text = LocalAuth_SteamToolsV2Import;
                binding.btnImportV2ByQRCode.Text = LocalAuth_SteamToolsV2Import;
                binding.tvQRCode.Text = ImportByQRCode;
                binding.tvFilePicker.Text = ImportByFilePicker;
            }).AddTo(this);

            SetOnClickListener(binding!.btnImportV1, binding.btnImportV2, binding.btnImportV2ByQRCode);
        }

        protected abstract void OnBtnImportV2ByQRCodeClick();

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
                OnBtnImportV2ByQRCodeClick();
                return true;
            }
            return base.OnClick(view);
        }
    }
}