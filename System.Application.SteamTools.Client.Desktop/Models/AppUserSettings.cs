using MessagePack;
using System.Collections.Generic;

namespace System.Application.Models
{
    /// <summary>
    /// Steam++ 应用 用户配置项
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class AppUserSettings
    {
        /// <summary>
        /// 启动参数
        /// </summary>
        public string? StartupArgs { get; set; }

        /// <summary>
        /// 是否启用日志
        /// </summary>
        public bool EnableLogger { get; set; }

        /// <summary>
        /// 用户设置的文本阅读器提供商，根据平台值不同，值格式为 枚举字符串 或 程序路径
        /// </summary>
        public Dictionary<Platform, string>? TextReaderProvider { get; set; }
    }

#if DEBUG

    [Obsolete("use AppUserSettings", true)]
    public class SettingsModel
    {
        [Obsolete("use AppUserSettings.StartupArgs", true)]
        public string? SteamRunParam { get; set; }

        [Obsolete("use AppUserSettings.EnableTextLog", true)]
        public bool EnableTextLog { get; set; }
    }

#endif
}