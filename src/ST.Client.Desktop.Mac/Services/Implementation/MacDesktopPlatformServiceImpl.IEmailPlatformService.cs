#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
#endif
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Application.Services.Implementation
{
    partial class MacDesktopPlatformServiceImpl : IEmailPlatformService
    {
        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Email/Email.macos.cs

        static T? InvokeOnMainThread<T>(Func<T> factory)
        {
            T? value = default;
            NSRunLoop.Main.InvokeOnMainThread(() => value = factory());
            return value;
        }

        public Task PlatformComposeAsync(EmailMessage? message)
        {
            var isComposeSupported = InvokeOnMainThread(() => NSWorkspace.SharedWorkspace.UrlForApplication(NSUrl.FromString("mailto:")) != null);
            if (!isComposeSupported)
            {
                throw new FeatureNotSupportedException();
            }

            var url = Email2.GetMailToUri(message);

            using var nsurl = NSUrl.FromString(url);
            NSWorkspace.SharedWorkspace.OpenUrl(nsurl);
            return Task.CompletedTask;
        }
    }
}