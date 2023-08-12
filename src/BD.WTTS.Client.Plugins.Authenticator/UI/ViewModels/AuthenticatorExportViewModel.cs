using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorExportViewModel : ViewModelBase
{
    SaveFileResult? _exportFile;

    [Reactive]
    public bool HasPasswordProtection { get; set; }

    [Reactive]
    public bool HasLocalProtection { get; set; }

    [Reactive]
    public string? Password { get; set; }

    [Reactive]
    public string? VerifyPassword { get; set; }

    /// <summary>
    /// 默认导出文件名
    /// </summary>
    public string DefaultExportAuthFileName =>
        string.Format($"Watt Toolkit Authenticators {{0}}{FileEx.MPO}",
            DateTime.Now.ToString(DateTimeFormat.File));

    public SaveFileResult? ExportFile
    {
        get => _exportFile;
        set
        {
            if (_exportFile != null)
            {
                _exportFile.Dispose();
            }
            _exportFile = value;
        }
    }

    string? _currentPassword;

    public AuthenticatorExportViewModel()
    {

    }

    public AuthenticatorExportViewModel(string? password = null)
    {
        _currentPassword = password;
    }

    public async Task Export()
    {
        if (HasPasswordProtection)
        {
            if (string.IsNullOrWhiteSpace(VerifyPassword) && VerifyPassword != Password)
            {
                Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                return;
            }
        }

        var sourceData = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();

        var auths = await AuthenticatorHelper.GetAllAuthenticatorsAsync(sourceData, _currentPassword);

        var exportFile = await AuthenticatorHelper.ExportAsync(DefaultExportAuthFileName, HasLocalProtection, auths, VerifyPassword);

        Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile?.ToString()));

        ExportFile = null;
    }
}