using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount
{
    readonly IPlatformSwitcher platformSwitcher;

    public PlatformAccount(ThirdpartyPlatform platform)
    {
        Accounts = new ObservableCollection<IAccount>();
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

        DeleteAccountCommand = ReactiveCommand.Create<IAccount>(async acc =>
        {
            var r = await platformSwitcher.DeleteAccountInfo(acc, this);
            if (r)
                Toast.Show(ToastIcon.Success, Strings.Success_DeletePlatformAccount__.Format(FullName, acc.DisplayName));
        });

        SetAccountAvatarCommand = ReactiveCommand.Create<IAccount>(async acc =>
        {
            await FilePicker2.PickAsync((path) =>
            {
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(acc.AccountName))
                {
                    var imagePath = Path.Combine(PlatformLoginCache, acc.AccountName, "avatar.png");
                    File.Copy(path, imagePath, true);
                    acc.ImagePath = imagePath;
                }
            }, IFilePickerFileType.Images());
        });
        EditRemarkCommand = ReactiveCommand.Create<IAccount>(async acc =>
        {
            var text = await TextBoxWindowViewModel.ShowDialogAsync(new()
            {
                Value = acc.AliasName,
                Title = AppResources.UserChange_EditRemark,
            });
            //可将名字设置为空字符串重置
            if (text == null)
                return;
            acc.AliasName = text;
            platformSwitcher.ChangeUserRemark(acc);
        });

        CopyCommand = ReactiveCommand.Create<string>(async text => await Clipboard2.SetTextAsync(text));

        OpenLinkCommand = ReactiveCommand.Create<string>(async url => await Browser2.OpenAsync(url));

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
                Accounts.Clear();
                var users = await platformSwitcher.GetUsers(this, () =>
                {
                    if (Accounts.Any_Nullable())
                        foreach (var user in Accounts)
                        {
                            if (user is SteamAccount su)
                            {
                                su.RaisePropertyChanged(nameof(su.ImagePath));
                                su.RaisePropertyChanged(nameof(su.AvatarFramePath));
                            }
                        }
                });

                if (users.Any_Nullable())
                    Accounts = new ObservableCollection<IAccount>(users.OrderByDescending(x => x.LastLoginTime));
            }
            catch (Exception ex)
            {
                ex.LogAndShowT(nameof(PlatformAccount));
            }
            finally
            {
                IsLoading = false;
            }
        });
    }

    public async ValueTask<bool> CurrnetUserAdd(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            await platformSwitcher.NewUserLogin(this);
            return true;
        }
        return await platformSwitcher.CurrnetUserAdd(name, this);
    }
}
