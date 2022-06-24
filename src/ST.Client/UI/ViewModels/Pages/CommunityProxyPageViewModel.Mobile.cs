// ReSharper disable once CheckNamespace
using System.Application.Services;
using System.Application.UI.Resx;
using System.IO;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    partial class CommunityProxyPageViewModel : IActionItem<CommunityProxyPageViewModel.ActionItem>
    {
        public enum ActionItem
        {
            ProxySettings = 1,
            CertificateExport,
            GoToSystemSecuritySettings,
            CertificateInstall,
            CertificateUninstall,
            CertificateStatus,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ProxySettings => AppResources.CommunityFix_ProxySettings,
            ActionItem.CertificateExport => AppResources.CommunityFix_CertificateExport,
            ActionItem.GoToSystemSecuritySettings => AppResources.CommunityFix_GoToSystemSecuritySettings,
            ActionItem.CertificateInstall => AppResources.CommunityFix_SetupCertificate,
            ActionItem.CertificateUninstall => AppResources.CommunityFix_DeleteCertificate,
            ActionItem.CertificateStatus => AppResources.CommunityFix_CertificateStatus,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.GoToSystemSecuritySettings => "baseline_settings_applications_black_24",
            ActionItem.CertificateExport => "menu_export_certificate_file",
            ActionItem.CertificateInstall => "ic_baseline_add_circle_outline_24",
            ActionItem.CertificateUninstall => "ic_baseline_remove_circle_outline_24",
            ActionItem.CertificateStatus => "ic_fluent_certificate_24_regular",
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

        public string? CerFilePath => reverseProxyService.CertificateManager.GetCerFilePathGeneratedWhenNoFileExists();

        /// <summary>
        /// 导出证书公钥，通过委托自定义导出逻辑
        /// </summary>
        /// <param name="action"></param>
        public void ExportCertificateFile(Action<string> action)
        {
            var cefFilePath = CerFilePath;
            if (cefFilePath != null)
            {
                action(cefFilePath);
            }
        }

        public async Task<bool> ExportCertificateFileAsync(Func<string, Task<bool>> func)
        {
            var cefFilePath = CerFilePath;
            if (cefFilePath != null)
            {
                return await func(cefFilePath);
            }
            return false;
        }

        async Task<bool> ExportCertificateFileAsync(string cefFilePath)
        {
            var fileName = ICertificateManager.CerExportFileName;
            using var result = await FilePicker2.SaveAsync(new()
            {
                InitialFileName = fileName,
            });
            if (result != null)
            {
                using var dest = result.OpenWrite();
                using var source = new FileStream(cefFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                await source.CopyToAsync(dest);
                Toast.Show(AppResources.ExportedToPath_.Format(result.ToString()));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 通过保存对话框导出证书公钥
        /// </summary>
        public void ExportCertificateFile() => ExportCertificateFile(async cefFilePath =>
        {
            await ExportCertificateFileAsync(cefFilePath);
        });

        public Task<bool> ExportCertificateFileAsync() => ExportCertificateFileAsync(ExportCertificateFileAsync);
    }
}