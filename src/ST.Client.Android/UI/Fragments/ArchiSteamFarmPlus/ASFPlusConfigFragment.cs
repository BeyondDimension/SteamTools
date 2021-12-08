using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    internal sealed class ASFPlusConfigFragment : ASFPlusFragment<fragment_asf_plus_config>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_asf_plus_config;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.tvASFVersion.Text = ASF_VersionNum + IArchiSteamFarmService.Instance.CurrentVersion;
                binding.btnGoToWebUIGlobalSettings.Text = ASF_OpenWebUIGlobalConfig;
                binding.btnImportASFBot.Text = ASF_ImportBotFile;
            }).AddTo(this);

            SetOnClickListener(binding!.btnGoToWebUIGlobalSettings, binding.btnImportASFBot);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnGoToWebUIGlobalSettings)
            {
                ViewModel?.OpenBrowser("WebConfig");
                return true;
            }
            if (view.Id == Resource.Id.btnImportASFBot)
            {
                ViewModel?.SelectBotFiles?.Invoke();
                return true;
            }
            return base.OnClick(view);
        }
    }
}
