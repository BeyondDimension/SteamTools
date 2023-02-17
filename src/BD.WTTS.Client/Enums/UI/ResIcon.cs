// ReSharper disable once CheckNamespace
namespace BD.WTTS.Enums;

/// <summary>
/// 资源图标
/// </summary>
public enum ResIcon
{
    None,
    AvatarDefault,
    AccountBox,
    Info,
    Person,
    Settings,
    SportsEsports,
    VerifiedUser,
    Steam,
    Xbox,
    Apple,
    QQ,
    Phone,

    /// <summary>
    /// 根据当前平台使用平台对应的📱图标，目前支持材料设计中 Android Phone 与 iPhone
    /// </summary>
    PlatformPhone,
    Exit,
}

public static class ResIconEnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResIcon ToIcon(this FastLoginChannel fastLoginChannel)
        => fastLoginChannel switch
        {
            FastLoginChannel.Steam => ResIcon.Steam,
            FastLoginChannel.Microsoft => ResIcon.Xbox,
            FastLoginChannel.QQ => ResIcon.QQ,
            FastLoginChannel.Apple => ResIcon.Apple,
            _ => throw new ArgumentOutOfRangeException(nameof(fastLoginChannel), fastLoginChannel, null),
        };
}