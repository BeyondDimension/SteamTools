using System.Collections.Generic;
using Xamarin.Essentials;

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
        /// Microsoft Windows / UWP(Universal Windows Platform)
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

        static readonly IReadOnlyDictionary<DevicePlatform, Platform> mapping = new Dictionary<DevicePlatform, Platform>
        {
            { DevicePlatform.Android, Platform.Android },
            { DevicePlatform.iOS, Platform.Apple },
            { DevicePlatform.UWP, Platform.Windows },
            { DevicePlatform.tvOS, Platform.Apple },
            { DevicePlatform.Tizen, Platform.Linux },
            { DevicePlatform.watchOS, Platform.Apple },
        };

        /// <summary>
        /// 将 <see cref="DevicePlatform"/> 转换为 <see cref="Platform"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Platform Convert(this DevicePlatform value) => mapping.ContainsKey(value) ? mapping[value] : Platform.Unknown;
    }
}