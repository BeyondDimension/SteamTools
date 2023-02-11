// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    /// <summary>
    /// 获取或设置当前应用的主题，例如跟随系统，浅色，深色
    /// </summary>
    AppTheme Theme { get; set; }

    /// <summary>
    /// 获取当前应用的实际主题
    /// </summary>
    AppTheme ActualTheme => Theme switch
    {
        AppTheme.FollowingSystem => GetActualThemeByFollowingSystem(),
        AppTheme.Light => AppTheme.Light,
        AppTheme.Dark => AppTheme.Dark,
        _ => DefaultActualTheme,
    };

    /// <summary>
    /// 获取当前应用的默认主题
    /// </summary>
    protected AppTheme DefaultActualTheme { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static AppTheme GetAppThemeByIsLightOrDarkTheme(bool isLightOrDarkTheme) => isLightOrDarkTheme ? AppTheme.Light : AppTheme.Dark;

    /// <summary>
    /// 获取当前应用主题跟随系统时的实际主题
    /// </summary>
    /// <returns></returns>
    protected AppTheme GetActualThemeByFollowingSystem()
    {
        var dps = IPlatformService.Instance;
        var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
        if (isLightOrDarkTheme.HasValue)
        {
            return GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
        }
        return DefaultActualTheme;
    }
}
