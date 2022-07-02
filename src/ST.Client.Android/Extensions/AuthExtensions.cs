using Android.App;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class AuthExtensions
    {
        /// <summary>
        /// 当未登录账号时，将关闭当前活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static bool IsAuthenticated(this Activity activity)
        {
            var value = UserService.Current.IsAuthenticated;
            if (!value)
            {
                activity.Finish();
            }
            return value;
        }

        /// <summary>
        /// 当已登录账号时，将关闭当前活动
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static bool IsNotAuthenticated(this Activity activity)
        {
            var value = UserService.Current.IsAuthenticated;
            if (value)
            {
                activity.Finish();
            }
            return value;
        }
    }
}
