using System.ComponentModel;

namespace System.Application.Models
{
    public enum SteamAppType : byte
    {
        [Description("SteamAppType_Unknown")]
        Unknown = 0,

        [Description("SteamAppType_Game")]
        Game = 1,

        [Description("SteamAppType_Application")]
        Application = 2,

        [Description("SteamAppType_Tool")]
        Tool = 3,

        [Description("SteamAppType_Demo")]
        Demo = 4,

        [Description("SteamAppType_Media")]
        Media = 5,

        [Description("SteamAppType_DLC")]
        DLC = 6,

        [Description("SteamAppType_Guide")]
        Guide = 7,

        [Description("SteamAppType_Driver")]
        Driver = 8,

        [Description("SteamAppType_Config")]
        Config = 9,

        [Description("SteamAppType_Hardware")]
        Hardware = 10,

        [Description("SteamAppType_Franchise")]
        Franchise = 11,

        [Description("SteamAppType_Video")]
        Video = 12,

        [Description("SteamAppType_Plugin")]
        Plugin = 13,

        [Description("SteamAppType_Music")]
        Music = 14,

        [Description("SteamAppType_Series")]
        Series = 15,

        [Description("SteamAppType_Shortcut")]
        Shortcut = 16,

        [Description("SteamAppType_DepotOnly")]
        DepotOnly = 17,

        [Description("SteamAppType_Mod")]
        Mod = 18,

        [Description("SteamAppType_Beta")]
        Beta = 19,
    }
}