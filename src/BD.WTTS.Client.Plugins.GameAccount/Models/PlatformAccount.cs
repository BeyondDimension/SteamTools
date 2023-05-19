using BD.WTTS.Enums;

namespace BD.WTTS.Models;

public sealed class PlatformAccount
{
    public AuthenticatorPlatform Platform { get; set; }

    public List<IAccount>? Accounts { get; set; }

}
