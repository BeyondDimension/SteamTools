namespace BD.WTTS;

/// <summary>
/// 提供用来获取应用程序信息（如版本号、说明、加载的程序集等）的属性。
/// </summary>
public static partial class AssemblyInfo
{
    /// <summary>
    /// 语义化应用程序版本
    /// https://semver.org/lang/zh-CN/
    /// </summary>
    public const string Version = "3.0.0";

    /// <summary>
    /// 预览版本号，范围 1~9
    /// </summary>
    const string ver_for_preview = "1";

    /// <summary>
    /// RC 版本号，范围 0~9
    /// </summary>
    const string ver_for_rc = "0";

    public const string FileVersion = $"{Version}.{ver_for_rc}0{ver_for_preview}";

    //public const string InformationalVersion = Version;
    public const string InformationalVersion = $"{Version}-preview.{ver_for_preview}";
    //public const string InformationalVersion = $"{Version}-rc.{ver_for_rc}";

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// </summary>
    public const string Trademark = "Watt Toolkit";

    public const string Title =
#if DEBUG
        $"[Debug] {Trademark}";
#else
        Trademark;
#endif

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// </summary>
    public const string Product = Trademark;

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// </summary>
    public const string Description = $"「{Trademark}」是一个开源跨平台的多功能游戏工具箱。";

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// </summary>
    public const string Company = "江苏蒸汽凡星科技有限公司";

    /// <summary>
    /// 与应用程序关联的版权声明。
    /// </summary>
    public const string Copyright = $"©️ {Company}. All rights reserved.";

    /// <summary>
    /// 简体中文的区域性名称。
    /// </summary>
    public const string CultureName_SimplifiedChinese = "zh-Hans";
    public const string CultureName_PRC = "zh-CN";
    public const string CultureName_TraditionalChinese = "zh-Hant";
    public const string CultureName_English = "en";
    public const string CultureName_Korean = "ko";
    public const string CultureName_Japanese = "ja";
    public const string CultureName_Russian = "ru";
    public const string CultureName_Spanish = "es";
    public const string CultureName_Italian = "it";

    public const bool Debuggable =
#if DEBUG
true
#else
false
#endif
        ;

#if !APP_HOST

    public const string APPLICATION_ID = "net.steampp.app";

    /// <inheritdoc cref="dotnetCampus.Ipc.CompilerServices.Attributes.IpcPublicAttribute.Timeout"/>
    public const int IpcTimeout =
#if DEBUG
        8000000;
#else
        8000;
#endif

#endif

    #region Modules/Plugins

    public const string Accelerator = "Accelerator";

#if !APP_HOST

    public const string GameAccount = "GameAccount";

    public const string GameList = "GameList";

    public const string SteamIdleCard = "SteamIdleCard";

#endif

    public const string ArchiSteamFarmPlus = "ArchiSteamFarmPlus";

#if !APP_HOST

    public const string Authenticator = "Authenticator";

    public const string GameTools = "GameTools";
    //public const string Update = "Update";

    public const string Plugins = "Plugins";

    public const string AcceleratorId = "00000000-0000-0000-0000-000000000001";
    public const string GameAccountId = "00000000-0000-0000-0000-000000000002";
    public const string GameListId = "00000000-0000-0000-0000-000000000003";
    public const string ArchiSteamFarmPlusId = "00000000-0000-0000-0000-000000000004";
    public const string AuthenticatorId = "00000000-0000-0000-0000-000000000005";
    public const string GameToolsId = "00000000-0000-0000-0000-000000000006";
    public const string SteamIdleCardId = "00000000-0000-0000-0000-000000000007";
#endif

    #endregion

}