using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    ValueTask SwapToAccount(IAccount? account, PlatformAccount platform);

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
}
