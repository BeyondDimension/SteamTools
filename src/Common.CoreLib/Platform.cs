using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// 平台
    /// </summary>
    [Flags]
    public enum Platform
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// Microsoft Windows(Win32)
        /// </summary>
        Windows = 4,

        /// <summary>
        /// Ubuntu / Debian / CentOS / Tizen
        /// </summary>
        Linux = 8,

        /// <summary>
        /// Android Phone / Android Pad / WearOS(Android Wear) / Android TV
        /// </summary>
        Android = 16,

        /// <summary>
        /// iOS / iPadOS / watchOS / tvOS / macOS
        /// </summary>
        Apple = 32,

        /// <summary>
        /// Universal Windows Platform
        /// </summary>
        UWP = 64,
    }

    /// <summary>
    /// Enum 扩展 <see cref="Platform"/>
    /// </summary>
    public static partial class PlatformEnumExtensions
    {
        /// <summary>
        /// 值是否在定义的范围中，排除 <see cref="Platform.Unknown"/>
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static bool IsDefined(this Platform platform)
        {
            return platform != Platform.Unknown && Enum.IsDefined(typeof(Platform), platform);
        }
    }
}