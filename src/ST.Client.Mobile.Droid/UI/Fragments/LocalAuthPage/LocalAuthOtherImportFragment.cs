using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.AddAuthWindowViewModel;

namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthOtherImportFragment : BaseFragment<fragment_local_auth_import_other, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_other;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tvWinAuthImport.Text = LocalAuth_WinAuthImport;
                binding.tvWinAuthImportTip.Text = LocalAuth_WinAuthTip;
                binding.btnWinAuthImport.Text = SelectFileImport.Format("*.txt");
                binding.tvSDAImport.Text = LocalAuth_SDAImport;
                binding.tvSDAImportTip.Text = LocalAuth_SDATip;
                binding.btnSDAImport.Text = SelectFileImport.Format("*.maFile");
            }).AddTo(this);

            SetOnClickListener(binding!.btnWinAuthImport, binding.btnSDAImport);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnWinAuthImport)
            {
                async void OnBtnWinAuthImportClick()
                    => await FilePickAsync(ViewModel!.ImportWinAuth);
                OnBtnWinAuthImportClick();
                return true;
            }
            else if (view.Id == Resource.Id.btnSDAImport)
            {
                async void OnBtnSDAImportClick()
                    => await FilePickAsync(ViewModel!.ImportSDA);
                OnBtnSDAImportClick();
                return true;
            }
            return base.OnClick(view);
        }
    }
}