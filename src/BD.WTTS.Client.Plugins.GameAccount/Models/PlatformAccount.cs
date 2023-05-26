using System.Linq;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount
{
    IPlatformSwitcher platformSwitcher;

    public PlatformAccount()
    {
        var platformSwitchers = Ioc.Get<IPlatformSwitcher[]>();

        this.platformSwitcher = Platform switch
        {
            ThirdpartyPlatform.Steam => platformSwitchers.OfType<SteamPlatformSwitcher>().First(),
            _ => platformSwitchers.OfType<BasicPlatformSwitcher>().First(),
        };
    }
}
