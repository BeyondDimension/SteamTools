using Microsoft.Extensions.DependencyInjection;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformToastImpl : ToastImpl
    {
        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
        {
        }

        protected override void PlatformShow(string text, int duration)
        {
            ToastService.Current.Notify(text);
        }

        internal static IServiceCollection TryAddToast(IServiceCollection services)
            => TryAddToast<PlatformToastImpl>(services);
    }
}