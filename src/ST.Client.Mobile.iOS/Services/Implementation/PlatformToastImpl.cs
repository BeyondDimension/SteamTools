extern alias NUGET_PACKAGE_TOAST_IOS;

using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using iOSToast = NUGET_PACKAGE_TOAST_IOS::GlobalToast.Toast;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// https://github.com/andrius-k/Toast
    /// </summary>
    internal sealed class PlatformToastImpl : ToastImpl
    {
        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
        {
        }

        protected override async void PlatformShow(string text, int duration)
        {
            var toast = iOSToast.ShowToast(text)
                .SetDuration(duration)
                .SetShowShadow(false)
                .SetAutoDismiss(true)
                .Show();
            await Task.Delay(duration);
            toast.Dismiss();
        }

        internal static IServiceCollection TryAddToast(IServiceCollection services)
            => TryAddToast<PlatformToastImpl>(services);
    }
}