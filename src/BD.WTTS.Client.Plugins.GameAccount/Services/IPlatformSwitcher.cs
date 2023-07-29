namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    void SwapToAccount(IAccount? account, PlatformAccount platform);

    bool ClearCurrentLoginUser(PlatformAccount platform);

    bool KillPlatformProcess(PlatformAccount platform);

    bool RunPlatformProcess(PlatformAccount platform, bool isAdmin);

    void NewUserLogin(PlatformAccount platform);

    bool CurrnetUserAdd(string name, PlatformAccount platform);

    string GetCurrentAccountId(PlatformAccount platform);

    void ChangeUserRemark(IAccount account)
    {
        if (!string.IsNullOrEmpty(account.AccountId))
            GameAccountSettings.AccountRemarks.Add($"{account.PlatformName}-{account.AccountId}", account.AliasName);
        else
            Toast.Show(ToastIcon.Error, "账号 Id 为空");
    }

    bool SetPlatformPath(PlatformAccount platform);

    Task DeleteAccountInfo(IAccount account, PlatformAccount platform);

    Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform);
}
