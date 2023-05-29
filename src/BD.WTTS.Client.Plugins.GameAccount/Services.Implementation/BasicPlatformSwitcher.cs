using BD.WTTS.Enums;

namespace BD.WTTS.Services.Implementation;

public sealed class BasicPlatformSwitcher : IPlatformSwitcher
{
    public BasicPlatformSwitcher()
    {

    }

    public void SwapToUser(IAccount account)
    {

    }

    public bool KillPlatformProcess()
    {
        return false;
    }

    public void RunPlatformProcess()
    {

    }

    public void NewUserLogin()
    {

    }

    public bool CurrnetUserAdd(string name)
    {
        //if (isExitBeforeInteract)
        if (!KillPlatformProcess())
            return false;
        return true;
    }

    public void ChangeUserRemark()
    {

    }

    public Task<IEnumerable<IAccount>?> GetUsers()
    {
        return Task.FromResult<IEnumerable<IAccount>?>(null);
    }
}
