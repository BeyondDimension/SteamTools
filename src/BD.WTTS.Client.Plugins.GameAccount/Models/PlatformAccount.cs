using System.Linq;
using static SteamKit2.GC.Dota.Internal.CMsgPracticeLobbyCreate;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount
{
    readonly IPlatformSwitcher platformSwitcher;

    public PlatformAccount(ThirdpartyPlatform platform)
    {
        var platformSwitchers = Ioc.Get<IEnumerable<IPlatformSwitcher>>();

        FullName = platform.ToString();
        Platform = platform;
        platformSwitcher = Platform switch
        {
            ThirdpartyPlatform.Steam => platformSwitchers.OfType<SteamPlatformSwitcher>().First(),
            _ => platformSwitchers.OfType<BasicPlatformSwitcher>().First(),
        };

        LoadUsers();
    }

    public async void LoadUsers()
    {
        var users = await platformSwitcher.GetUsers(this);
        if (users.Any_Nullable())
            Accounts = new ObservableCollection<IAccount>(users);
    }

    public bool CurrnetUserAdd(string name)
    {
        return platformSwitcher.CurrnetUserAdd(name, this);
    }
}
