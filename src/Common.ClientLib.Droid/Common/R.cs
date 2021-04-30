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
            public static int? ic_stat_notify_msg { get; internal set; }
        }

        public static class activities
        {
            static JClass? _entrance;

            /// <summary>
            /// 通知栏点击的入口默认值
            /// </summary>
            public static JClass entrance
            {
                get => _entrance.ThrowIsNull(nameof(entrance));
                internal set => _entrance = value;
            }
        }

        public static class @string
        {
            static string? _app_name;

            public static string app_name
            {
                get => _app_name.ThrowIsNull(nameof(app_name));
                internal set => _app_name = value;
            }
        }

#pragma warning restore IDE1006 // 命名样式
    }
}