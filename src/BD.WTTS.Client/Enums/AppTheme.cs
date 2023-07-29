using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Enums;

/// <summary>
/// 应用程序主题
/// </summary>
public enum AppTheme : byte
{
    /// <summary>
    /// 跟随系统
    /// </summary>
    FollowingSystem,

    /// <summary>
    /// 亮色/浅色主题
    /// </summary>
    Light,

    /// <summary>
    /// 暗色/深色主题
    /// </summary>
    Dark,

    /// <summary>
    /// 高对比度主题
    /// </summary>
    HighContrast,

    /// <summary>
    /// 自定义主题
    /// </summary>
    [Obsolete("In development…")]
    Custom,
}

public static partial class AppThemeEnumExtensions
{
#if DEBUG
    /// <summary>
    /// auto/light/dark
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("use ToEnglishString", true)]
    public static string ToString2(this AppTheme value) => value switch
    {
        AppTheme.FollowingSystem => "auto",
        AppTheme.Light => "light",
        AppTheme.HighContrast => "highContrast",
        _ => "dark",
    };

    /// <summary>
    /// Resx / AppResources
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("use ToDisplayString", true)]
    public static string ToString3(this AppTheme value) => value switch
    {
        AppTheme.FollowingSystem => AppResources.Settings_UI_SystemDefault,
        AppTheme.Light => AppResources.Settings_UI_Light,
        AppTheme.HighContrast => AppResources.Settings_UI_HighContrast,
        _ => AppResources.Settings_UI_Dark,
    };
#endif

    /// <summary>
    /// auto/light/dark/highContrast
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToEnglishString(this AppTheme value) => value switch
    {
        AppTheme.FollowingSystem => "auto",
        AppTheme.Light => "light",
        AppTheme.HighContrast => "highContrast",
        _ => "dark",
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayString(this AppTheme value) => value switch
    {
        AppTheme.FollowingSystem => AppResources.Settings_UI_SystemDefault,
        AppTheme.Light => AppResources.Settings_UI_Light,
        AppTheme.HighContrast => AppResources.Settings_UI_HighContrast,
        _ => AppResources.Settings_UI_Dark,
    };
}