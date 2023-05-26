namespace BD.WTTS.Enums;

/// <summary>
/// 第三方平台
/// </summary>
[Flags]
public enum ThirdpartyPlatform
{
    Steam = 1 << 1,

    Origin = 1 << 2,

    EADesktop = 1 << 3,

    Uplay = 1 << 4,

    MicrosoftStore = 1 << 5,

    GOG = 1 << 6,

    Epic = 1 << 7,

    PlayStation = 1 << 8,

    Xbox = 1 << 9,

    NintendoSwitch = 1 << 10,

    BattleNet = 1 << 11,

    Google = 1 << 12,

    RiotGames = 1 << 13,
}
