namespace BD.WTTS;

/// <summary>
/// 提供用来获取应用程序信息（如版本号、说明、加载的程序集等）的属性。
/// </summary>
public static partial class AssemblyInfo
{
    /// <summary>
    /// 与应用程序关联的产品名称。
    /// </summary>
    public const string Version = "3.0.0";

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// </summary>
    public const string Product =
#if APP_REVERSE_PROXY
        $"{Trademark} - Accelerator and script module sub-process";
#else
        Trademark;
#endif

    /// <summary>
    /// 与应用程序关联的产品名称。
    /// </summary>
    public const string Trademark = "Watt Toolkit";

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// </summary>
    public const string Description = $"「{Trademark}」是一个开源跨平台的多功能游戏工具箱。";

    /// <summary>
    /// 与该应用程序关联的公司名称。
    /// </summary>
    public const string Company = "长沙次元超越科技有限公司";

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

    public const string APPLICATION_ID = "net.steampp.app";
}