#if __ANDROID__
using Android.App;
using Android.Content;
using Com.Tencent.Tauth;
#endif

namespace System.Application
{
    public static partial class TencentOpenApiSDK
    {
        public const string APP_ID = "101953775";

#if __ANDROID__
        /// <summary>
        /// https://wiki.connect.qq.com/qq%e7%99%bb%e5%bd%95
        /// <para>Tencent类是SDK的主要实现类，开发者可通过Tencent类访问腾讯开放的OpenAPI。</para>
        /// <para>其中APP_ID是分配给第三方应用的appid，类型为String。</para>
        /// <para>其中Authorities为 Manifest文件中注册FileProvider时设置的authorities属性值 </para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Tencent GetTencent(Context context)
        {
            var tencent = Tencent.CreateInstance(
                APP_ID,
                context.ApplicationContext,
                GoToPlatformPages.GetAuthority(context));
            return tencent!;
        }

        /// <summary>
        /// https://wiki.connect.qq.com/server-side%E7%99%BB%E5%BD%95%E6%A8%A1%E5%BC%8F
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="tencent"></param>
        /// <param name="activity"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static int LoginServerSide<TActivity>(this Tencent tencent, TActivity activity, string scope = "get_user_info") where TActivity : Activity, IUiListener => tencent.LoginServerSide(activity, scope, activity);

        public static void ShowError(this UiError? error)
        {
            if (error != null)
            {
                Toast.Show($"QQ login fail, code: {error.ErrorCode}, msg: {error.ErrorMessage}, detail:{error.ErrorDetail}");
            }
        }
#endif
    }
}