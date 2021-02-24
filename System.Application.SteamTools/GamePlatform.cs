using System.ComponentModel;

namespace System.Application
{
    public enum GamePlatform : byte
    {
        [Description("Steam")]
        Steam = 1,

        [Description("Origin")]
        Origin = 2,

        [Description("EA Desktop")]
        EADesktop = 10,

        [Description("Uplay")]
        Uplay = 3,

        [Description("Microsoft Store")]
        WinStore = 4,

        [Description("GOG")]
        GOG = 5,

        [Description("EPIC")]
        Epic = 6,

        [Description("PlayStation")]
        PlayStation = 7,

        [Description("Xbox")]
        Xbox = 8,

        [Description("NintendoSwitch")]
        NintendoSwitch = 9,
    }

#if DEBUG

    [Obsolete("use GamePlatform", true)]
    public enum GamePlatformEnum
    {
    }

#endif
}