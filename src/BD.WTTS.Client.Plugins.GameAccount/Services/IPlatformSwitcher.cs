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

    void ChangeUserRemark();

    bool SetPlatformPath(PlatformAccount platform);

    void DeleteAccountInfo(IAccount account, PlatformAccount platform);

    Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform);
}
