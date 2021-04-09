using System.ComponentModel;

namespace System.Application.Models
{
    public enum SteamAppType : byte
    {
        [Description("未知")]
        Unknown = 0,

        [Description("游戏")]
        Game = 1,

        [Description("软件")]
        Application = 2,

        [Description("工具")]
        Tool = 3,

        [Description("Demo")]
        Demo = 4,

        [Description("预告视频")]
        Media = 5,

        [Description("DLC")]
        DLC = 6,

        [Description("指南")]
        Guide = 7,

        [Description("驱动")]
        Driver = 8,

        [Description("隐藏软件")]
        Config = 9,

        [Description("硬件")]
        Hardware = 10,

        [Description("授权")]
        Franchise = 11,

        [Description("视频")]
        Video = 12,

        [Description("插件")]
        Plugin = 13,

        [Description("原声音轨")]
        Music = 14,

        [Description("影视集")]
        Series = 15,

        [Description("快捷方式")]
        Shortcut = 16,

        [Description("仓库")]
        DepotOnly = 17,

    }

#if DEBUG

    [Obsolete("use SteamAppType", true)]
    public enum SteamAppTypeEnum
    {
    }

#endif
}