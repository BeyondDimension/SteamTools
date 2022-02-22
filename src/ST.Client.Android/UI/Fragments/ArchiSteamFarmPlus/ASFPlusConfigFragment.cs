using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Activities;
using ArchiSteamFarm;
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
                binding.tvGoToWebUIGlobalSettings.Text = ASF_OpenWebUIGlobalConfig;
                binding.tvImportASFBot.Text = ASF_ImportBotFile;
                binding.tvASFSettingsAutoRunArchiSteamFarm.Text = ASF_AutoRunASF;
                binding.tvSetCryptKey.Text = ASF_SetCryptKey;
                binding.tvConsoleSettings.Text = ASF_ConsoleSettings;
                binding.tvASFSettingsConsoleFontSize.Text = ASF_ConsoleFontSize;
                binding.tvASFSettingsConsoleMaxLine.Text = ASF_ConsoleMaxLine;
                binding.tvStorageSpace.Text = $"ASF {Settings_General_StorageSpace}";
                binding.tvOpenDirASFConfig.Text = Settings_General_BrowseCustomFolder.Format("ASF Config");
                binding.tvOpenDirASFPlugin.Text = Settings_General_BrowseCustomFolder.Format("ASF Plugin");
                binding.tvOpenDirASFLog.Text = Settings_General_BrowseCustomFolder.Format("ASF Log");
                binding.tvOpenDirASFWebUI.Text = Settings_General_BrowseCustomFolder.Format("ASF WebUI");
            }).AddTo(this);

            binding!.tbASFSettingsConsoleFontSize.Text = ASFSettings.ConsoleFontSize.Value.ToString();
            binding.tbASFSettingsConsoleFontSize.Hint = ASFSettings.DefaultConsoleFontSize.ToString();
            binding.tbASFSettingsConsoleFontSize.TextChanged += (_, _) =>
            {
                var value = binding.tbASFSettingsConsoleFontSize.Text;
                if (int.TryParse(value, out var value2))
                {
                    if (value2 < ASFSettings.MinRangeConsoleFontSize) value2 = ASFSettings.MinRangeConsoleFontSize;
                    if (value2 > ASFSettings.MaxRangeConsoleFontSize) value2 = ASFSettings.MaxRangeConsoleFontSize;
                    ASFSettings.ConsoleFontSize.Value = value2;
                }
            };

            binding.tbASFSettingsConsoleMaxLine.Text = ASFSettings.ConsoleMaxLine.Value.ToString();
            binding.tbASFSettingsConsoleMaxLine.Hint = ASFSettings.DefaultConsoleMaxLine.ToString();
            binding.tbASFSettingsConsoleMaxLine.TextChanged += (_, _) =>
            {
                var value = binding.tbASFSettingsConsoleMaxLine.Text;
                if (int.TryParse(value, out var value2))
                {
                    if (value2 < ASFSettings.MinRangeConsoleMaxLine) value2 = ASFSettings.MinRangeConsoleMaxLine;
                    if (value2 > ASFSettings.MaxRangeConsoleMaxLine) value2 = ASFSettings.MaxRangeConsoleMaxLine;
                    ASFSettings.ConsoleMaxLine.Value = value2;
                }
            };

            SetAutoRunArchiSteamFarm();

            SetOnClickListener(
                binding.layoutRootASFSettingsAutoRunArchiSteamFarm,
                binding.layoutRootGoToWebUIGlobalSettings,
                binding.layoutRootImportASFBot,
                binding.layoutRootSetCryptKey,
                binding.layoutRootOpenDirASFConfig,
                binding.layoutRootOpenDirASFPlugin,
                binding.layoutRootOpenDirASFLog,
                binding.layoutRootOpenDirASFWebUI);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.layoutRootGoToWebUIGlobalSettings)
            {
                ViewModel?.OpenBrowser("WebConfig");
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootImportASFBot)
            {
                ViewModel?.SelectBotFiles?.Invoke();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootASFSettingsAutoRunArchiSteamFarm)
            {
                ASFSettings.AutoRunArchiSteamFarm.Value = !ASFSettings.AutoRunArchiSteamFarm.Value;
                SetAutoRunArchiSteamFarm();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootSetCryptKey)
            {
                ViewModel?.SetEncryptionKey();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOpenDirASFConfig)
            {
                OpenFolder(ASFPathFolder.Config);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOpenDirASFPlugin)
            {
                OpenFolder(ASFPathFolder.Plugin);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOpenDirASFLog)
            {
                OpenFolder(ASFPathFolder.Logs);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOpenDirASFWebUI)
            {
                OpenFolder(ASFPathFolder.WWW);
                return true;
            }
            return base.OnClick(view);
        }

        void SetAutoRunArchiSteamFarm() => binding!.swASFSettingsAutoRunArchiSteamFarm.Checked = ASFSettings.AutoRunArchiSteamFarm.Value;

        void OpenFolder(ASFPathFolder folderASFPath)
        {
            var folderASFPathValue = IArchiSteamFarmService.GetFolderPath(folderASFPath);
            GoToPlatformPages.StartActivity<ExplorerActivity, string>(Activity, folderASFPathValue);
        }
    }
}
