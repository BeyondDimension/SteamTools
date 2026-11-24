namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorExportViewModel : ViewModelBase
{
    [Reactive]
    public partial bool HasPasswordProtection { get; set; } = true;

    [Reactive]
    public partial bool HasLocalProtection { get; set; }

    [Reactive]
    public partial string? Password { get; set; }

    [Reactive]
    public partial string? VerifyPassword { get; set; }

    [Reactive]
    public partial bool IsExporting { get; private set; }

    public async Task Export()
    {
        if (IsExporting) return;
        var exportPassword = HasPasswordProtection ? Password : null;
        var local = HasLocalProtection;
        if (HasPasswordProtection && !AuthenticatorHelper.IsValidNewPassword(exportPassword))
        {
            Toast.Show(ToastIcon.Warning, Strings.Auth_VaultPasswordGuidance);
            return;
        }
        if (HasPasswordProtection && !string.Equals(exportPassword, VerifyPassword, StringComparison.Ordinal))
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
            return;
        }

        IsExporting = true;
        try
        {
            if (!local && exportPassword == null && !await IWindowManager.Instance.ShowTaskDialogAsync(
                    new MessageBoxWindowViewModel { Content = Strings.Auth_VaultUnprotectedExport },
                    Strings.ExportAuth, isCancelButton: true)) return;

            Password = VerifyPassword = null;
            var access = await AuthenticatorHelper.ReadAuthorizedVaultAsync(await AuthenticatorHelper.GetAllSourceAuthenticatorAsync());
            if (access == null || access.Value.items.Count == 0) return;

            var exportFile = await AuthenticatorHelper.ExportAsync($"WattToolkit Authenticators {DateTime.Now.ToString(DateTimeFormat.File)}",
                local, access.Value.items, exportPassword);
            if (exportFile != null)
                Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile.ToString()));
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AuthenticatorExportViewModel), ex, "Could not export tokens.");
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_ExportAuth_Error.Format(Strings.Auth_VaultReadFailed));
        }
        finally
        {
            IsExporting = false;
        }
    }
}