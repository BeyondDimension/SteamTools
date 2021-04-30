using Microsoft.Extensions.DependencyInjection;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformToastImpl : ToastImpl
    {
        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
        {
        }

        protected override bool IsMainThread => MainThreadDesktop.IsMainThread;

        protected override void BeginInvokeOnMainThread(Action action) => MainThreadDesktop.BeginInvokeOnMainThread(action);

        protected override void PlatformShow(string text, int duration)
        {
            ToastService.Current.Notify(text);
        }

        internal static IServiceCollection TryAddToast(IServiceCollection services)
            => TryAddToast<PlatformToastImpl>(services);
    }
}