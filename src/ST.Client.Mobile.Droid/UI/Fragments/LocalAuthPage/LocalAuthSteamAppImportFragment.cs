using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthSteamAppImportFragment : BaseFragment<fragment_local_auth_import_steam_app, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_app;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                if (binding == null) return;
                binding.layoutName.Hint = LocalAuth_AuthName;
                binding.tvSteamUUIDTip.Text = LocalAuth_SteamuuidTip;
                binding.tvSteamGuardTip.Text = LocalAuth_SteamGuardTip;
                binding.btnImport.Text = ConfirmImport;
                binding.tvImportTip.Text = LocalAuth_SteamAppImportTip;
            }).AddTo(this);

            binding!.tbName.TextChanged += (_, _) =>
            {
                ViewModel!.AuthName = binding.tbName.Text;
            };
            binding.tbSteamUUIDKey.TextChanged += (_, _) =>
            {
                ViewModel!.UUID = binding.tbSteamUUIDKey.Text;
            };
            binding.tbGuard.TextChanged += (_, _) =>
            {
                ViewModel!.SteamGuard = binding.tbGuard.Text;
            };

            SetOnClickListener(binding!.btnImport);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnImport)
            {
                var importResult = ViewModel!.ImportSteamGuard2();
                if (importResult)
                {
                    binding!.tbName.Text = string.Empty;
                    binding.tbSteamUUIDKey.Text = string.Empty;
                    binding.tbGuard.Text = string.Empty;
                }
                return true;
            }
            return base.OnClick(view);
        }

        public override void OnResume()
        {
            base.OnResume();
            if (!GeneralSettings.CaptureScreen.Value)
            {
                Activity.SetWindowSecure(true);
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            Activity.SetWindowSecure(false);
        }
    }
}