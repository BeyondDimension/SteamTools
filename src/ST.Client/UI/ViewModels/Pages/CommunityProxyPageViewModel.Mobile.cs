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
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ProxySettings => AppResources.CommunityFix_ProxySettings,
            ActionItem.CertificateExport => AppResources.CommunityFix_CertificateExport,
            ActionItem.GoToSystemSecuritySettings => AppResources.CommunityFix_GoToSystemSecuritySettings,
            ActionItem.CertificateInstall => AppResources.CommunityFix_SetupCertificate,
            ActionItem.CertificateUninstall => AppResources.CommunityFix_DeleteCertificate,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.GoToSystemSecuritySettings => "baseline_settings_applications_black_24",
            ActionItem.CertificateExport => "menu_export_certificate_file",
            ActionItem.CertificateInstall => "ic_baseline_add_circle_outline_24",
            ActionItem.CertificateUninstall => "ic_baseline_remove_circle_outline_24",
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

        public string? CerFilePath => httpProxyService.GetCerFilePathGeneratedWhenNoFileExists();

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

        /// <summary>
        /// 通过保存对话框导出证书公钥
        /// </summary>
        public void ExportCertificateFile() => ExportCertificateFile(async cefFilePath =>
        {
            var fileName = IHttpProxyService.CerExportFileName;
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
            }
        });

        /// <summary>
        /// 移除证书弹窗提示，在一些平台上不能自动操作，弹窗提示操作步骤文本
        /// </summary>
        public void UninstallCertificateShowTips()
        {
            string title = AppResources.CommunityFix_DeleteCertificateTipTitle;
            string text = AppResources.CommunityFix_DeleteCertificateTipText_.Format(IHttpProxyService.RootCertificateName); ;
            MessageBox.Show(text, title);
        }
    }
}