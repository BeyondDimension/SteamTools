// ReSharper disable once CheckNamespace
using System.Application.Services;
using System.Application.UI.Resx;
using System.IO;

namespace System.Application.UI.ViewModels
{
    partial class CommunityProxyPageViewModel : IActionItem<CommunityProxyPageViewModel.ActionItem>
    {
        public enum ActionItem
        {
            ProxySettings = 1,
            CertificateExport,
            GoToSystemSecuritySettings,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ProxySettings => AppResources.CommunityFix_ProxySettings,
            ActionItem.CertificateExport => AppResources.CommunityFix_CertificateExport,
            ActionItem.GoToSystemSecuritySettings => AppResources.CommunityFix_GoToSystemSecuritySettings,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.GoToSystemSecuritySettings => "baseline_settings_applications_black_24",
            _ => "baseline_settings_black_24",
        };

        public void MenuItemClick(ActionItem id)
        {
            switch (id)
            {
                case ActionItem.ProxySettings:
                    ProxySettingsCommand?.Invoke();
                    break;
            }
        }

        public async void ExportCertificateFile()
        {
            var cefFilePath = httpProxyService.CerFilePath;
            if (File.Exists(cefFilePath))
            {
                var fileName = IHttpProxyService.CerExportFileName;
                using var result = await FilePicker2.SaveAsync(new() { InitialFileName = fileName });
                if (result != null)
                {
                    using var dest = result.OpenWrite();
                    using var source = new FileStream(cefFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    await source.CopyToAsync(dest);
                    Toast.Show(AppResources.ExportedToPath_.Format(result.ToString()));
                }
            }
            else
            {
                Toast.Show(AppResources.CommunityFix_ExportCertificateFileNotExists);
            }
        }
    }
}