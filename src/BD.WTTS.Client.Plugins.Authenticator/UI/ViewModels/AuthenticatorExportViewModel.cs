using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorExportViewModel : ViewModelBase
{
    bool _hasPasswordProtection;
    bool _hasLocalProtection;
    string? _password;
    string? _verifyPassword;
    SaveFileResult? _exportFile;

    public bool HasPasswordProtection
    {
        get => _hasPasswordProtection;
        set
        {
            if (value == _hasPasswordProtection) return;
            _hasPasswordProtection = value;
            this.RaisePropertyChanged();
        }
    }

    public bool HasLocalProtection
    {
        get => _hasLocalProtection;
        set
        {
            if (value == _hasLocalProtection) return;
            _hasLocalProtection = value;
            this.RaisePropertyChanged();
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (value == _password) return;
            _password = value;
            this.RaisePropertyChanged();
        }
    }

    public string? VerifyPassword
    {
        get => _verifyPassword;
        set
        {
            if (value == _verifyPassword) return;
            _verifyPassword = value;
            this.RaisePropertyChanged();
        }
    }

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

        var sourceData = await AuthenticatorService.GetAllSourceAuthenticatorAsync();

        var auths = await AuthenticatorService.GetAllAuthenticatorsAsync(sourceData, _currentPassword);

        var exportFile = await AuthenticatorService.ExportAsync(DefaultExportAuthFileName, HasLocalProtection, auths, VerifyPassword);

        Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile?.ToString()));

        ExportFile = null;
    }
}