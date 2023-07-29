using AppResources = BD.WTTS.Client.Resources.Strings;
using System.Linq;
using System.Drawing.Drawing2D;
using AngleSharp.Dom;

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
            await platformSwitcher.DeleteAccountInfo(acc, this);
            Toast.Show(ToastIcon.Success, $"已经删除 {FullName} 平台 {acc.DisplayName} 账号");
        });

        SetAccountAvatarCommand = ReactiveCommand.Create<IAccount>(async acc =>
        {
            FilePickerFileType fileTypes = new ValueTuple<string, string[]>[]
            {
                ("Image Files", FileEx.Images.Select(s => $"*{s}").ToArray()),
            };

            await FilePicker2.PickAsync((path) =>
            {
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(acc.AccountName))
                {
                    var imagePath = Path.Combine(PlatformLoginCache, acc.AccountName, "avatar.png");
                    File.Copy(path, imagePath, true);
                    acc.ImagePath = imagePath;
                }
            }, fileTypes);
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
                Accounts = null;
                var users = await platformSwitcher.GetUsers(this);
                if (users.Any_Nullable())
                    Accounts = new ObservableCollection<IAccount>(users);
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
