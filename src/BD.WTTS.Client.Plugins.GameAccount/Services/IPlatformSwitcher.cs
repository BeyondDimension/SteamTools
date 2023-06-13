namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    void SwapToUser(IAccount account);

    void ClearCurrentLoginUser();

    bool KillPlatformProcess();

    void RunPlatformProcess();

    void NewUserLogin();

    bool CurrnetUserAdd(string name, PlatformAccount platform);

    void ChangeUserRemark();

    Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform);
}
