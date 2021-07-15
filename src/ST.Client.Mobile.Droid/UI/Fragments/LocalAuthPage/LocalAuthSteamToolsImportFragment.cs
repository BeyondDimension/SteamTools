using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.AddAuthWindowViewModel;

namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthSteamToolsImportFragment : BaseFragment<fragment_local_auth_import_steam_plus_plus, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_plus_plus;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.btnImportV1.Text = LocalAuth_SteamToolsV1Import;
                binding.btnImportV2.Text = LocalAuth_SteamToolsV2Import;
            }).AddTo(this);

            SetOnClickListener(binding!.btnImportV1, binding.btnImportV2);
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
            return base.OnClick(view);
        }
    }
}