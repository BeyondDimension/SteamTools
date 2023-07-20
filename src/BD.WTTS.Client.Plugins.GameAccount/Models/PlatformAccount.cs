using AppResources = BD.WTTS.Client.Resources.Strings;
using System.Linq;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount
{
    readonly IPlatformSwitcher platformSwitcher;

    public ICommand SwapToAccountCommand { get; }

    public ICommand OpenUrlToBrowserCommand { get; }

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
            Toast.Show(ToastIcon.Success, AppResources.Success_SwitchAccount__.Format(FullName, acc.DisplayName));
        });

        OpenUrlToBrowserCommand = ReactiveCommand.Create<IAccount>(acc =>
        {
            //Browser2.Open(acc);
        });

        if (!Directory.Exists(PlatformLoginCache))
            Directory.CreateDirectory(PlatformLoginCache);

        //LoadUsers();
    }

    public void LoadUsers()
    {
        Task2.InBackground(async () =>
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                Accounts = null;
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
        });
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
