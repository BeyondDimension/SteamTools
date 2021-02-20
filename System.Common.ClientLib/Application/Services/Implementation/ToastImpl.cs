using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xamarin.Essentials;

namespace System.Application.Services.Implementation
{
    public abstract class ToastImpl : IToast
    {
        protected readonly IToastIntercept intercept;

        protected const string TAG = "Toast";

        public ToastImpl(IToastIntercept intercept)
        {
            this.intercept = intercept;
        }

        /// <summary>
        /// 根据字符串长度计算持续时间
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        protected virtual int CalcDurationByStringLength(int len)
            => len > 9 ? ToDuration(ToastLength.Long) : ToDuration(ToastLength.Short);

        public void Show(string text, int? duration)
        {
            try
            {
                if (MainThread.IsMainThread)
                {
                    Show_();
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(Show_);
                }

                void Show_()
                {
                    if (intercept.OnShowExecuting(text)) return;
                    var _duration = duration ?? CalcDurationByStringLength(text.Length);
                    PlatformShow(text, _duration);
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "ShowToast Error, text: {0}", text);
            }
        }

        public void Show(string text, ToastLength duration) => Show(text, ToDuration(duration));

        protected abstract void PlatformShow(string text, int duration);

        /// <summary>
        /// 将 <see cref="ToastLength"/> 转换为 持续时间
        /// </summary>
        /// <param name="toastLength"></param>
        /// <returns></returns>
        protected virtual int ToDuration(ToastLength toastLength) => (int)toastLength;

        protected static IServiceCollection TryAddToast<TToastImpl>(IServiceCollection services) where TToastImpl : class, IToast
        {
            services.TryAddSingleton<IToastIntercept, NoneToastIntercept>();
            services.TryAddSingleton<IToast, TToastImpl>();
            return services;
        }
    }
}