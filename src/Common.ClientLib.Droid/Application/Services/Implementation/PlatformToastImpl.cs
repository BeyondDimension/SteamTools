using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using AndroidApplication = Android.App.Application;
using AndroidToast = Android.Widget.Toast;
using AndroidToastLength = Android.Widget.ToastLength;
using JException = Java.Lang.Exception;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// https://developer.android.google.cn/guide/topics/ui/notifiers/toasts
    /// </summary>
    internal sealed class PlatformToastImpl : ToastImpl
    {
        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
        {
        }

        static AndroidToast? toast;

        protected override void PlatformShow(string text, int duration)
        {
            var context = AndroidApplication.Context;
            var duration2 = (AndroidToastLength)duration;

            // https://blog.csdn.net/android157/article/details/80267737
            try
            {
                if (toast == null)
                {
                    toast = AndroidToast.MakeText(context, text, duration2);
                    if (toast == null) throw new NullReferenceException("toast markeText Fail");
                }
                else
                {
                    toast.Duration = duration2;
                }
                SetTextAndShow(toast, text);
            }
            catch (JException e)
            {
                Log.Error(TAG, e, "ShowDroidToast Error, text: {0}", text);
                // 解决在子线程中调用Toast的异常情况处理
                Looper.Prepare();
                var _toast = AndroidToast.MakeText(context, text, duration2)
                    ?? throw new NullReferenceException("toast markeText Fail(2)");
                SetTextAndShow(_toast, text);
                Looper.Loop();
            }

            static void SetTextAndShow(AndroidToast t, string text)
            {
                // 某些定制ROM会更改内容文字，例如MIUI，重新设置可强行指定文本
                t.SetText(text);
                t.Show();
            }
        }

        protected override int ToDuration(ToastLength toastLength) => toastLength switch
        {
            ToastLength.Short => (int)AndroidToastLength.Short,
            ToastLength.Long => (int)AndroidToastLength.Long,
            _ => base.ToDuration(toastLength),
        };

        internal static IServiceCollection TryAddToast(IServiceCollection services)
            => TryAddToast<PlatformToastImpl>(services);
    }
}