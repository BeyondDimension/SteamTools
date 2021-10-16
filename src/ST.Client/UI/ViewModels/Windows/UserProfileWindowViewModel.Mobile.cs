using System.Application.Services;
using System.Application.UI.Resx;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class UserProfileWindowViewModel
    {
        public static string GetIsBindOrUnbundleBtnText(FastLoginChannel channel) => IsBindOrUnbundle(channel) ? AppResources.Bind : AppResources.Unbundling;

        public static bool IsBindOrUnbundle(FastLoginChannel channel) => channel switch
        {
            FastLoginChannel.Steam => UserService.Current.User?.SteamAccountId == null,
            FastLoginChannel.Microsoft => UserService.Current.User?.MicrosoftAccountEmail == null,
            FastLoginChannel.Apple => UserService.Current.User?.AppleAccountEmail == null,
            FastLoginChannel.QQ => UserService.Current.User?.QQNickName == null,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel.ToString()),
        };

        public static string? GetIsBindOrUnbundleTbText(FastLoginChannel channel) => channel switch
        {
            FastLoginChannel.Steam => UserService.Current.User?.SteamAccountId?.ToString(),
            FastLoginChannel.Microsoft => UserService.Current.User?.MicrosoftAccountEmail,
            FastLoginChannel.Apple => UserService.Current.User?.AppleAccountEmail,
            FastLoginChannel.QQ => UserService.Current.User?.QQNickName,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel.ToString()),
        };

        public void OnBindOrUnbundleFastLoginClick(FastLoginChannel channel)
        {
            if (IsBindOrUnbundle(channel))
            {
                OnBindFastLoginClick.Invoke(channel.ToString());
            }
            else
            {
                OnUnbundleFastLoginClick.Invoke(channel.ToString());
            }
        }
    }
}