// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 是否应使用 亮色主题 <see langword="true"/> / 暗色主题 <see langword="false"/>
    /// </summary>
    bool? IsLightOrDarkTheme { get; }

    /// <summary>
    /// 设置或关闭使用跟随系统的主题
    /// </summary>
    /// <param name="enable"></param>
    void SetLightOrDarkThemeFollowingSystem(bool enable);
}