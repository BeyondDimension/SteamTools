using System.Linq;
using static SteamKit2.GC.Dota.Internal.CMsgPracticeLobbyCreate;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount
{
    readonly IPlatformSwitcher platformSwitcher;

    public ICommand SwapToAccountCommand { get; set; }

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

        SwapToAccountCommand = ReactiveCommand.Create<IAccount>(acc =>
        {
            platformSwitcher.SwapToAccount(acc, this);
        });

        LoadUsers();
    }

    public async void LoadUsers()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var users = await platformSwitcher.GetUsers(this);
            if (users.Any_Nullable())
                Accounts = new ObservableCollection<IAccount>(users);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(PlatformAccount), ex, "LoadUsers Faild");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public bool CurrnetUserAdd(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            platformSwitcher.NewUserLogin(this);
            return true;
        }
        return platformSwitcher.CurrnetUserAdd(name, this);
    }
}
