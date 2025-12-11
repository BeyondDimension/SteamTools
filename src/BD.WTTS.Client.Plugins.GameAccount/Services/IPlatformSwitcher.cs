using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    ValueTask<bool> SwapToAccount(IAccount? account, PlatformAccount platform);

    ValueTask<bool> ClearCurrentLoginUser(PlatformAccount platform);

    ValueTask<bool> KillPlatformProcess(PlatformAccount platform);

    bool RunPlatformProcess(PlatformAccount platform, bool isAdmin);

    ValueTask NewUserLogin(PlatformAccount platform);

    ValueTask<bool> CurrnetUserAdd(string name, PlatformAccount platform);

    string GetCurrentAccountId(PlatformAccount platform);

    void ChangeUserRemark(IAccount account)
    {
        if (!string.IsNullOrEmpty(account.AccountId))
            GameAccountSettings.AccountRemarks.Add($"{account.PlatformName}-{account.AccountId}", account.AliasName);
        else
            Toast.Show(ToastIcon.Error, AppResources.Error_AccountIdIsEmpty);
    }

    bool SetPlatformPath(PlatformAccount platform);

    Task<bool> DeleteAccountInfo(IAccount account, PlatformAccount platform);

    Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform, Action? refreshUsers = null);

    public void CreateSystemProtocol(string targetPath)
    {
#if WINDOWS
        using var key = Registry.ClassesRoot.CreateSubKey(Constants.CUSTOM_URL_SCHEME_NAME);
        key.SetValue("URL Protocol", "");
        using var shellKey = key.CreateSubKey("shell");
        using RegistryKey openKey = shellKey.CreateSubKey("open");
        using RegistryKey commandKey = openKey.CreateSubKey("command");
        commandKey.SetValue("", "\"" + targetPath + "\" \"%1\"");
#endif
    }

    public async Task<bool> CreateLoginShortcut(
        string pathLink,
        string targetPath,
        string? arguments,
        string? description,
        string? hotkey,
        string? iconLocation,
        string? workingDirectory,
        CancellationToken cancellationToken = default)
    {
#if WINDOWS
        var s = await IPlatformService.IPCRoot.Instance;
        s.CreateShortcut(pathLink, targetPath, arguments, description, hotkey, iconLocation, workingDirectory);
        return true;
#else
        await Task.CompletedTask;
        return false;
#endif
    }
}
