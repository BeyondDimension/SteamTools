using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.IO;
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

        protected abstract void QRCodeAnalyze(string filePath);

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

        async void OnBtnImportByFilePickerClick() => await FilePicker2.PickAsync(async filePath =>
        {
            var extension = Path.GetExtension(filePath);
            if (ImageSouce.IsImage(extension))
            {
                try
                {
                    QRCodeAnalyze(filePath);
                }
                catch (Exception e)
                {
                    Log.Error(nameof(QRCodeAnalyze), e, nameof(OnBtnImportByFilePickerClick));
                }
            }
            else
            {
                var isOK = await ViewModel!.ImportAutoAsnyc(filePath, extension);
                if (isOK)
                {
                    // 导入成功，关闭添加令牌页
                    Activity?.Finish();
                }
            }
        });
    }
}