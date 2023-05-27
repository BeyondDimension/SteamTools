namespace BD.WTTS.Services;

public interface IPlatformSwitcher
{
    void SwapToUser(IAccount account);

    bool KillPlatformProcess();

    void RunPlatformProcess();

    void NewUserLogin();

    bool CurrnetUserAdd(string name);

    void ChangeUserRemark();

    Task<IEnumerable<IAccount>?> GetUsers();
}
