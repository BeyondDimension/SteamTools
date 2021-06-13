using System.Application;
using System.Application.UI.Activities;
using AndroidApplication = Android.App.Application;
using JClass = Java.Lang.Class;

// ReSharper disable once CheckNamespace
namespace System.Common
{
    internal static partial class R
    {
#pragma warning disable IDE1006 // 命名样式

        public static class drawable
        {
            /// <summary>
            /// 通知栏图标资源
            /// </summary>
            public static int? ic_stat_notify_msg => null; // Resource.Drawable.ic_stat_notify_msg;
        }

        public static class activities
        {
            /// <summary>
            /// 通知栏点击的入口默认值
            /// </summary>
            public static JClass entrance => typeof(SplashActivity).GetJClass();
        }

        public static class @string
        {
            public static string app_name => AndroidApplication.Context.GetString(Resource.String.app_name);
        }

#pragma warning restore IDE1006 // 命名样式
    }
}