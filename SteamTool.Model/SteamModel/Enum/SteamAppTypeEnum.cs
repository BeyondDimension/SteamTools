using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SteamTool.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SteamAppTypeEnum : byte
    {
        [Description("应用")]
        Application = 1,
        [Description("Config")]
        Config = 2,
        [Description("Demo")]
        Demo = 3,
        [Description("DLC")]
        DLC = 4,
        [Description("游戏")]
        Game = 5,
        [Description("媒体")]
        Media = 6,
        [Description("音乐")]
        Music = 7,
        [Description("工具")]
        Tool = 8,
        [Description("视频")]
        Video = 9,
        [Description("未知")]
        Unknown = 10,
    }
}
