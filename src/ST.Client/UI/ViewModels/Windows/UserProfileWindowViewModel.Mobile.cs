using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq.Expressions;

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

        public static Expression<Func<UserService, object?>> GetIsBindOrUnbundleExpression(FastLoginChannel channel) => channel switch
        {
            FastLoginChannel.Steam => x => x.User!.SteamAccountId,
            FastLoginChannel.Microsoft => x => x.User!.MicrosoftAccountEmail,
            FastLoginChannel.Apple => x => x.User!.AppleAccountEmail,
            FastLoginChannel.QQ => x => x.User!.QQNickName,
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