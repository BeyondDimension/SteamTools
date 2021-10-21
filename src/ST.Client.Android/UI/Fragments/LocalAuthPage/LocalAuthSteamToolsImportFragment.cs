using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

// ReSharper disable once CheckNamespace
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
                binding.tvImportV2ByQRCode.Text = ImportByQRCode;
                binding.tvFilePicker.Text = ImportByFilePicker;
            }).AddTo(this);

            SetOnClickListener(binding!.btnImportAutoByFilePicker, binding.btnImportV2ByQRCode);
        }

        protected abstract void OnBtnImportV2ByQRCodeClick();

        protected abstract void Analyze(string filePath);

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnImportAutoByFilePicker)
            {
                OnBtnImportByFilePickerClick();
                return true;
            }
            else if (view.Id == Resource.Id.btnImportV2ByQRCode)
            {
                OnBtnImportV2ByQRCodeClick();
                return true;
            }
            return base.OnClick(view);
        }

        async void OnBtnImportByFilePickerClick() => await FilePicker2.PickAsync(filePath => ViewModel!.ImportAuto(filePath, extension =>
        {
            if (string.Equals(extension, FileEx.JPG, StringComparison.OrdinalIgnoreCase) || string.Equals(extension, FileEx.JPEG, StringComparison.OrdinalIgnoreCase) || string.Equals(extension, FileEx.PNG, StringComparison.OrdinalIgnoreCase) || string.Equals(extension, FileEx.WEBP, StringComparison.OrdinalIgnoreCase) || string.Equals(extension, FileEx.HEIC, StringComparison.OrdinalIgnoreCase) || string.Equals(extension, FileEx.HEIF, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Analyze(filePath);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }));
    }
}