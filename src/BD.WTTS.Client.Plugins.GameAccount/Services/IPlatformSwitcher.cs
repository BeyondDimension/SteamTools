namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    void SwapToAccount(IAccount account, PlatformAccount platform);

    bool ClearCurrentLoginUser(PlatformAccount platform);

    bool KillPlatformProcess();

    bool RunPlatformProcess(PlatformAccount platform, bool isAdmin);

    void NewUserLogin(PlatformAccount platform);

    bool CurrnetUserAdd(string name, PlatformAccount platform);

    string GetCurrentAccountId(PlatformAccount platform);

    void ChangeUserRemark();

    Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform);
}
