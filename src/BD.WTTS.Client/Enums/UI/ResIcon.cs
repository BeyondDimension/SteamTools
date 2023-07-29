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
    Attach,
    Contact,
    OpenFile,

    /// <summary>
    /// 根据当前平台使用平台对应的📱图标，目前支持材料设计中 Android Phone 与 iPhone
    /// </summary>
    PlatformPhone,
    Exit,
}

public static class ResIconEnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResIcon ToIcon(this ExternalLoginChannel fastLoginChannel)
        => fastLoginChannel switch
        {
            ExternalLoginChannel.Steam => ResIcon.Steam,
            ExternalLoginChannel.Microsoft => ResIcon.Xbox,
            ExternalLoginChannel.QQ => ResIcon.QQ,
            ExternalLoginChannel.Apple => ResIcon.Apple,
            _ => throw new ArgumentOutOfRangeException(nameof(fastLoginChannel), fastLoginChannel, null),
        };
}