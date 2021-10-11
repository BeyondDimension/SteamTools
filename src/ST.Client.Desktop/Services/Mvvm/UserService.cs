using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    partial class UserService
    {
        SteamUser? _SteamUser;
        public SteamUser? CurrentSteamUser
        {
            get => _SteamUser;
            set => this.RaiseAndSetIfChanged(ref _SteamUser, value);
        }

        protected override string DefaultAvaterPath => "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/avater_default.png";

        protected override void OnSignOut()
        {
            CurrentSteamUser = null;
        }

        public override async Task RefreshUserAvaterAsync()
        {
            if (User != null)
            {
                if (User.AvatarUrl.Any_Nullable())
                {
                    var settingPriority = FastLoginChannel.Steam; // 设置中优先选取头像渠道配置项
                    var order = new[] { settingPriority }.Concat(new[] {
                        FastLoginChannel.Steam,
                        FastLoginChannel.QQ,
                        FastLoginChannel.Apple,
                        FastLoginChannel.Microsoft,
                    }.Where(x => x != settingPriority));
                    foreach (var item in order)
                    {
                        if (User.AvatarUrl!.ContainsKey(item))
                        {
                            var avatarUrl = User.AvatarUrl[item];
                            if (!string.IsNullOrWhiteSpace(avatarUrl))
                            {
                                var avatarLocalFilePath = await IHttpService.Instance.GetImageAsync(avatarUrl, ImageChannelType.SteamAvatars);
                                var avaterSouce = ImageSouceHelper.TryParse(avatarLocalFilePath, isCircle: true);
                                AvaterPath = avaterSouce ?? DefaultAvaterPath;
                            }
                            return;
                        }
                        else if (item == FastLoginChannel.Steam
                            && await RefreshSteamUserAvaterAsync())
                        {
                            return;
                        }
                    }
                }
                else if (await RefreshSteamUserAvaterAsync())
                {
                    return;
                }

                async Task<bool> RefreshSteamUserAvaterAsync()
                {
                    if (User != null && User.SteamAccountId.HasValue)
                    {
                        CurrentSteamUser = await ISteamworksWebApiService.Instance.GetUserInfo(User.SteamAccountId.Value);
                        CurrentSteamUser.AvatarStream = IHttpService.Instance.GetImageAsync(CurrentSteamUser.AvatarFull, ImageChannelType.SteamAvatars);
                        var avaterSouce = ImageSouceHelper.TryParse(await CurrentSteamUser.AvatarStream, isCircle: true);
                        AvaterPath = avaterSouce ?? DefaultAvaterPath;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            AvaterPath = DefaultAvaterPath;
        }
    }
}
